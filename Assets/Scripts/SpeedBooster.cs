using UnityEngine;

public class SpeedBooster : MonoBehaviour {
    [SerializeField] private float boostForce;

    private void OnTriggerEnter(Collider other) {
        other.attachedRigidbody.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);
    }
}
