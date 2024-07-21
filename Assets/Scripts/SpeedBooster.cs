using UnityEngine;

public class SpeedBooster : MonoBehaviour {
    [SerializeField] private float boostForce;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("TriggerColliders")) {
            return;
        }
        
        other.attachedRigidbody.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);
    }
}
