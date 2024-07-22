using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedResetTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        ResetCarController(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ResetCarController(other);
    }

    private void ResetCarController(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("TriggerColliders")) {
            return;
        }
        
        other.attachedRigidbody.GetComponent<CarController>()?.ForceReset();
    }
}
