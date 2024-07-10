using UnityEngine;

public class TriggerableObject : MonoBehaviour
{
    public GameObject triggerableGameObject;

    private bool isObjectFallen = false;
    [SerializeField] private Animator _animator;

    private static readonly int CauseObjectToFall = Animator.StringToHash("CauseObjectToFall");

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        var carController = other.attachedRigidbody.gameObject.GetComponent<CarController>();

        if (carController && !isObjectFallen)
        {
            _animator.SetBool(CauseObjectToFall, true);
            isObjectFallen = true;
        }
    }
}