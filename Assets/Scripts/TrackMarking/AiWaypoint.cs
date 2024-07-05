using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AiWaypoint : MonoBehaviour {
    [SerializeField] private Transform pointTowards;
    private static float rayLength = 5;
    private static float sphereRadius = 2;

    public const string TAG = "AiWaypoint";

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

    private void Update() {
        if (!CompareTag(TAG)) {
            tag = TAG;
        }

        if (!pointTowards) {
            return;
        }

        Vector3 relativePos = pointTowards.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = rotation;
    }
}