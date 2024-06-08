using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDriver : MonoBehaviour{
    [Header("References")]
    [SerializeField] private CarController carController;
    private Vector2 inputs = Vector2.up * 1f;
    [SerializeField] private List<Transform> waypoints;

    void Start() {
        GetWaypoints();
    }

    void Update() {
        SteerTowardsNearestWaypoint();
        if (carController != null) {
            carController.SetInputs(inputs);
        }
    }

    private void GetWaypoints() {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(AiWaypoint.TAG);

        foreach (GameObject gameObject in gameObjects) {
            waypoints.Add(gameObject.transform);
        }
    }

    private void SteerTowardsNearestWaypoint() {
        Transform nearestWaypoint = GetNearestWaypoint();
        SteerToMatchRotation(nearestWaypoint);
    }

    private Transform GetNearestWaypoint() {
        Transform nearest = transform;
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

    private void SteerToMatchRotation(Transform other) {
        float steeringDotProduct = Vector3.Dot(transform.right, other.forward);
        Debug.DrawLine(transform.position, transform.position + transform.right * steeringDotProduct * 10, Color.red);
        inputs.x = Mathf.Clamp(steeringDotProduct, -1f, 1f);

        float accelerationDotProduct = Vector3.Dot(transform.forward, other.forward);
        Debug.DrawLine(transform.position, transform.position + transform.forward * accelerationDotProduct * 10, Color.red);
        float minAcceleration = 0.1f;
        float unsignedAcceleration = Mathf.Max(Mathf.Abs(accelerationDotProduct), minAcceleration);
        float signedAcceleration = unsignedAcceleration * Mathf.Sign(accelerationDotProduct);
        inputs.y = Mathf.Clamp(signedAcceleration, -1f, 1f);
    }
}
