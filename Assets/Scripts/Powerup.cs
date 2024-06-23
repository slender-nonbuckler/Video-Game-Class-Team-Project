// Powerup.cs
using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour
{
    private PowerupManager.PowerupInfo powerupInfo;
    private bool isCollected = false;
    private static float cooldownTime = 1f; // 1 second cooldown
    private static float lastCollectionTime = -1f;
    [SerializeField] private float spinSpeed = 180f;

    private void Start()
    {
        AssignRandomPowerup();
    }
    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    private void AssignRandomPowerup()
    {
        if (PowerupManager.Instance == null)
        {
            Debug.LogError("PowerupManager.Instance is null!");
            return;
        }
        powerupInfo = PowerupManager.Instance.GetRandomPowerup();
        if (powerupInfo == null)
        {
            Debug.LogError("Failed to get a random powerup from PowerupManager!");
            return;
        }
        Debug.Log($"Powerup {gameObject.name} assigned powerup: {powerupInfo.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController carController = other.GetComponentInParent<CarController>();
        if (carController != null && !isCollected && Time.time - lastCollectionTime > cooldownTime)
        {
            Debug.Log($"Powerup {gameObject.name} triggered. Applying powerup: {powerupInfo.name}");
            ApplyPowerup(carController);
            isCollected = true;
            lastCollectionTime = Time.time;
            StartCoroutine(DeactivateAfterDelay());
        }
    }

    private void ApplyPowerup(CarController carController)
    {
        if (powerupInfo == null || powerupInfo.applyEffect == null)
        {
            Debug.LogError($"Powerup {gameObject.name}: PowerupInfo or applyEffect is null!");
            return;
        }
        carController.AddActivePowerup(powerupInfo);
        powerupInfo.applyEffect(carController);
    }

    private IEnumerator DeactivateAfterDelay()
    {
        Debug.Log($"Powerup {gameObject.name}: Deactivating visuals");
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(0.1f); // Short delay before destroying
        Debug.Log($"Powerup {gameObject.name}: Destroying gameObject");
        Destroy(gameObject);
    }
}
