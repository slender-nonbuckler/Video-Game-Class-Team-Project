using UnityEngine;

public class AiTarget : MonoBehaviour {
    [SerializeField] private Transform nextTarget;

    public const string START_TAG = "StartTarget";
    private void OnDrawGizmos() {
        if (!nextTarget) {
            return;
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, nextTarget.position);
    }

    public Transform GetNextTarget() {
        return nextTarget;
    } 
}