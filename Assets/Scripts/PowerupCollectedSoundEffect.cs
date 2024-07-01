using UnityEngine;

public class PowerupCollectedSoundEffect : MonoBehaviour
{
    public AudioSource powerupCollectAudioSource;
    public AudioClip powerupCollectSound;
    private bool isCollected = false;
    private static float cooldownTime = 0.5f;
    private static float lastCollectionTime = -1f;

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
        PlayerDriver playerDriver = other.GetComponentInParent<PlayerDriver>();
        if (playerDriver != null && !isCollected && Time.time - lastCollectionTime > cooldownTime)
        {
            if (playerDriver.gameObject.CompareTag("Player") && !powerupCollectAudioSource.isPlaying)
            {
                powerupCollectAudioSource.clip = powerupCollectSound;
                powerupCollectAudioSource.PlayOneShot(powerupCollectAudioSource.clip);
            }
        }
    }
}