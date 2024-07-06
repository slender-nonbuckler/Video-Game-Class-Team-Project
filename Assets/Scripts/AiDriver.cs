using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiDriver : MonoBehaviour {
    public enum Difficulty {
        Pushover,
        Easy,
        Medium,
        Hard,
        Expert
    }
    
    Vector2 INVALID_VECTOR2 = Vector2.positiveInfinity;
    
    [Header("References")] [SerializeField]
    private CarController carController;

    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private Transform target;

    [Header("Parameters")] 
    [SerializeField]
    private Difficulty difficulty = Difficulty.Medium;
    private Difficulty prevDifficulty = Difficulty.Pushover;
    

    [Range(0f, 1f)]
    [SerializeField] private float noiseLerpFactor = 0f;
    [SerializeField] private float timeBetweenNoiseChanges = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float noiseWeight = 0f;
    
    private float timeTillNextNoiseChange = 0f;
    private Vector2 currNoise = Vector2.zero;
    private Vector2 targetNoise = Vector2.zero;
    
    private Vector2 input;

    private AiDataManager aiDataManager;
    
    void Start() {
        FindAiDataManager();
        FindStartTarget();
        SyncDifficultySettings();
    }

    void Update() {
        if (!carController) {
            return;
        }

        List<Vector2> desiredInputs = new List<Vector2> {
            GetInputToMatchNearestWaypoint(),
            GetInputToNextTarget()
        };
        
        Vector2 carInput = GetWeightedInput(desiredInputs) + GetDifficultyInputNoise();
        input = carInput;
        carController.SetInputs(carInput);
    }

    public void SetDifficulty(Difficulty difficulty) {
        this.difficulty = difficulty;
    }

    private void OnDrawGizmos() {
        Vector3 relativeInput = transform.forward * input.y + transform.right * input.x;
        Debug.DrawLine(transform.position, transform.position + relativeInput * 10, Color.red);
        Vector3 relativeNoise = transform.forward * currNoise.y + transform.right * currNoise.x;
        Debug.DrawLine(transform.position, transform.position + relativeNoise * 10 * noiseWeight);
    }

    private Vector2 GetDifficultyInputNoise() {
        if (difficulty != prevDifficulty) {
            SyncDifficultySettings();
        }
        UpdateInputNoise();
        currNoise = Vector2.Lerp(currNoise, targetNoise, noiseLerpFactor);
        return currNoise * noiseWeight;
    }

    private void SyncDifficultySettings() {
        Debug.Log("Syncing difficulty: " + difficulty);
        switch (difficulty) {
            case Difficulty.Pushover:
                noiseWeight = 1f;
                timeBetweenNoiseChanges = 1f;
                noiseLerpFactor = 0.05f;
                break;
            
            case Difficulty.Easy:
                noiseWeight = 0.5f;
                timeBetweenNoiseChanges = 0.75f;
                noiseLerpFactor = 0.1f;
                break;
            
            case Difficulty.Medium:
                noiseWeight = 0.25f;
                timeBetweenNoiseChanges = 0.5f;
                noiseLerpFactor = 0.2f;
                break;
            
            case Difficulty.Hard:
                noiseWeight = 0.125f;
                timeBetweenNoiseChanges = 0.25f;
                noiseLerpFactor = 0.4f;
                break;
            
            case Difficulty.Expert:
                noiseWeight = 0f;
                break;
        }

        prevDifficulty = difficulty;
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

    private void FindAiDataManager() {
        GameObject gameObjectWithTag = GameObject.FindGameObjectWithTag(AiDataManager.TAG);
        if (!gameObjectWithTag) {
            return;
        }

        aiDataManager = gameObjectWithTag.GetComponent<AiDataManager>();
    }

    private void FindStartTarget() {
        GameObject[] targetGameObjects = GameObject.FindGameObjectsWithTag(AiTarget.START_TAG);
        if (targetGameObjects == null || targetGameObjects.Length == 0) {
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
        if (!aiDataManager) {
            return null;
        }
        
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform waypoint in aiDataManager.GetWaypointsNearby(transform.position, 20f)) {
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