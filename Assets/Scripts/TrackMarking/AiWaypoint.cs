using UnityEngine;

[ExecuteInEditMode]
public class AiWaypoint : MonoBehaviour {
    [SerializeField] private Transform pointTowards;
    private static float rayLength = 5;
    private static float sphereRadius = 2;

    public const string TAG = "AiWaypoint";

    private AiDataManager aiDataManager;

    private void Awake() {
        if (!CompareTag(TAG)) {
            tag = TAG;
        }
    }

    private void Start() {
        FindAiDataManager();
        aiDataManager?.AddWaypoint(this);
    }

    private void Update() {
        if (!pointTowards) {
            return;
        }

        Vector3 relativePos = pointTowards.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = rotation;
    }

    void OnDestroy() {
        aiDataManager?.RemoveWaypoint(this);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector3 direction = transform.forward;
        Vector3 rayEnd = transform.position + direction * rayLength;
        Gizmos.DrawLine(transform.position, rayEnd);
        Gizmos.DrawLine(rayEnd, rayEnd + transform.right + -transform.forward);
        Gizmos.DrawLine(rayEnd, rayEnd + -transform.right + -transform.forward);
        Gizmos.DrawLine(rayEnd, rayEnd + transform.up + -transform.forward);
        Gizmos.DrawLine(rayEnd, rayEnd + -transform.up + -transform.forward);
    }

    private void FindAiDataManager() {
        GameObject gameObjectWithTag = GameObject.FindGameObjectWithTag(AiDataManager.TAG);
        if (!gameObjectWithTag) {
            return;
        }

        aiDataManager = gameObjectWithTag.GetComponent<AiDataManager>();
    }
}