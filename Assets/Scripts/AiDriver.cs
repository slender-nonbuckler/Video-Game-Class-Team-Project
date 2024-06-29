using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiDriver : MonoBehaviour {
    [Header("References")] [SerializeField]
    private CarController carController;

    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private Transform target;

    [Header("Parameters")] [SerializeField] [Range(0f, 1f)]
    private float difficulty = 0f;

    [Range(0f, 1f)]
    [SerializeField] private float noiseLerpFactor = 0f;
    [SerializeField] private float timeBetweenNoiseChanges = 0.3f;
    private float timeTillNextNoiseChange = 0f;

    Vector2 INVALID_VECTOR2 = Vector2.positiveInfinity;
    private Vector2 currNoise = Vector2.zero;
    private Vector2 targetNoise = Vector2.zero;
    
    
    private Vector2 input;
    void Start() {
        FindWaypoints();
        FindStartTarget();
    }

    void Update() {
        if (!carController) {
            return;
        }

        List<Vector2> desiredInputs = new List<Vector2> {
            GetInputToMatchNearestWaypoint(),
            GetInputToNextTarget()
        };
        
        Vector2 carInput = GetWeightedInput(desiredInputs) + (GetInputNoise() * Mathf.Abs(1 - difficulty));
        input = carInput.normalized;
        carController.SetInputs(carInput);
    }

    private void OnDrawGizmos() {
        Vector3 relativeInput = transform.forward * input.y + transform.right * input.x;
        Debug.DrawLine(transform.position, transform.position + relativeInput * 10, Color.red);
    }

    private Vector2 GetInputNoise() {
        UpdateInputNoise();
        currNoise = Vector2.Lerp(currNoise, targetNoise, noiseLerpFactor);
        Vector3 relativeNoise = transform.forward * currNoise.y + transform.right * currNoise.x;
        Debug.DrawLine(transform.position, transform.position + relativeNoise * 10);
        return currNoise;
    }

    private void UpdateInputNoise() {
        if (Time.time < timeTillNextNoiseChange) {
            return;
        }

        targetNoise = Random.insideUnitCircle;
        timeTillNextNoiseChange = Time.time + timeBetweenNoiseChanges;
    }

    private void OnTriggerEnter(Collider other) {
        AiTarget aiTarget = other.GetComponent<AiTarget>();
        if (aiTarget) {
            UpdateTarget(aiTarget);
        }
    }

    private void FindWaypoints() {
        GameObject[] waypointGameObjects = GameObject.FindGameObjectsWithTag(AiWaypoint.TAG);
        foreach (GameObject waypoint in waypointGameObjects) {
            waypoints.Add(waypoint.transform);
        }
    }

    private void FindStartTarget() {
        GameObject[] targetGameObjects = GameObject.FindGameObjectsWithTag(AiTarget.START_TAG);
        if (targetGameObjects == null) {
            return;
        }

        target = targetGameObjects[0].transform;
    }

    private Vector2 GetInputToMatchNearestWaypoint() {
        Transform nearestWaypoint = GetNearestWaypoint();
        if (!nearestWaypoint) {
            return INVALID_VECTOR2;
        }

        return GetSteerToMatchRotation(nearestWaypoint);
    }

    private Transform GetNearestWaypoint() {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform waypoint in waypoints) {
            float distance = Vector3.Distance(transform.position, waypoint.position);
            if (distance < minDistance) {
                nearest = waypoint;
                minDistance = distance;
            }
        }

        return nearest;
    }

    private Vector2 GetSteerToMatchRotation(Transform other) {
        float steeringDotProduct = Vector3.Dot(transform.right, other.forward);
        Debug.DrawLine(transform.position, transform.position + transform.right * (steeringDotProduct * 10), Color.red);
        steeringDotProduct = Mathf.Clamp(steeringDotProduct, -1f, 1f);

        float accelerationDotProduct = Vector3.Dot(transform.forward, other.forward);
        Debug.DrawLine(transform.position, transform.position + transform.forward * (accelerationDotProduct * 10),
            Color.red);
        float minAcceleration = 0.1f;
        float unsignedAcceleration = Mathf.Max(Mathf.Abs(accelerationDotProduct), minAcceleration);
        float signedAcceleration = unsignedAcceleration * Mathf.Sign(accelerationDotProduct);
        signedAcceleration = Mathf.Clamp(signedAcceleration, -1f, 1f);

        return new Vector2(steeringDotProduct, signedAcceleration);
    }

    private Vector2 GetInputToNextTarget() {
        if (!target) {
            return INVALID_VECTOR2;
        }

        Vector3 directionToMove = (target.position - transform.position).normalized;
        float steeringAngle = Vector3.SignedAngle(transform.forward, directionToMove, Vector3.up) / 90;
        float acceleration = Vector3.Dot(transform.forward, directionToMove);

        return new Vector2(steeringAngle, acceleration);
    }

    private void UpdateTarget(AiTarget aiTarget) {
        target = aiTarget.GetNextTarget();
    }

    private bool IsInfinity(Vector2 vector) {
        return float.IsInfinity(vector.x) && float.IsInfinity(vector.y);
    }

    private Vector2 GetWeightedInput(List<Vector2> inputs) {
        inputs.RemoveAll(IsInfinity);

        if (inputs.Count == 0) {
            return Vector2.zero;
        }

        Vector2 summedInputs = Vector2.zero;
        foreach (Vector2 input in inputs) {
            summedInputs += input;
        }

        return summedInputs / inputs.Count;
    }
}