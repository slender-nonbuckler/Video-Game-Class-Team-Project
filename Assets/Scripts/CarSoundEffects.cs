using UnityEngine;

public class CarSoundEffects : MonoBehaviour
{
    public Rigidbody car;
    public float carCollisionPitchMin = 1.0f;
    public float carCollisionPitchMax = 3.0f;
    [SerializeField] private float carSpeed;
    [SerializeField] private float previousCarSpeed;
    public AudioSource engineAudioSource;
    public AudioSource collisionAudioSource;
    public AudioClip[] carCollisionSounds;
    
    // Start is called before the first frame update
    void Start()
    {
        carSpeed = car.velocity.magnitude * 4.0f / 100;
        previousCarSpeed = 0;
        carSpeed = 0;
        engineAudioSource.pitch = 1;
    }
    
    // Update is called once per frame
    void Update()
    {
        carSpeed = car.velocity.magnitude * 4.0f / 100;
        print("cur speed: " + carSpeed);
        print("previous speed: " + previousCarSpeed);

        engineAudioSource.pitch = carSpeed * 3;
        if (carSpeed * 3 < 1.0)
        {
            engineAudioSource.pitch = 1;
        }
    }
    
    private void OnCollisionEnter(Collision otherEntity)
    {
        if (otherEntity.gameObject.CompareTag("Car"))
        {
            collisionAudioSource.clip = carCollisionSounds[Random.Range(0, carCollisionSounds.Length)];
            collisionAudioSource.pitch = Random.Range(carCollisionPitchMin, carCollisionPitchMax);
            collisionAudioSource.PlayOneShot(collisionAudioSource.clip);
        }
    }
}
