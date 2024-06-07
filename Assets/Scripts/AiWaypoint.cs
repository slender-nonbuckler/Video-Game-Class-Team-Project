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
        Gizmos.DrawRay(transform.position, direction * rayLength);
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }

    private void Update() {
        if (pointTowards == null) {
            return;
        }

        Vector3 relativePos = pointTowards.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = rotation;
    }
}
