using UnityEngine;

public class TriggerableObject : MonoBehaviour {
    private bool isObjectFallen;
    [SerializeField] private Animator _animator;

    private static readonly int CauseObjectToFall = Animator.StringToHash("CauseObjectToFall");

    private void OnTriggerEnter(Collider other) {
        var carController = other.attachedRigidbody.gameObject.GetComponent<CarController>();

        if (carController && !isObjectFallen) {
            _animator.SetBool(CauseObjectToFall, true);
            isObjectFallen = true;
        }
    }
}