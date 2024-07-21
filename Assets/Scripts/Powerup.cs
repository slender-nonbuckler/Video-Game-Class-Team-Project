using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour
{
    private PowerupManager.PowerupInfo powerupInfo;
    private bool isCollected = false;
    private static float cooldownTime = 1f; // 1 second cooldown
    private static float lastCollectionTime = -1f;
    [SerializeField] private float spinSpeed = 180f;
    [SerializeField] private float colorChangeSpeed = 1f;
    private Renderer powerupRenderer;
    private Material powerupMaterial;
    private float hue = 0f;

    private void Start()
    {
        InitializeRenderer();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        UpdateColor();
    }
    private void UpdateColor()
    {
        if (powerupMaterial)
        {
            hue += colorChangeSpeed * Time.deltaTime;
            if (hue > 1f) hue -= 1f;
            Color newColor = Color.HSVToRGB(hue, 1f, 1f);
            powerupMaterial.color = newColor;
        }
    }
    private void InitializeRenderer()
    {
        powerupRenderer = GetComponent<Renderer>();
        if (powerupRenderer == null)
        {
            // If not found on this GameObject, look in children
            powerupRenderer = GetComponentInChildren<Renderer>();
            if (powerupRenderer == null)
            {
                return;
            }
        }

        if (powerupRenderer.sharedMaterial == null)
        {
            return;
        }

        // Create a material instance to avoid changing shared materials
        powerupMaterial = new Material(powerupRenderer.sharedMaterial);
        powerupRenderer.material = powerupMaterial;
    }


    private void OnTriggerEnter(Collider other)
    {
        CarController carController = other.GetComponentInParent<CarController>();
        if (carController != null && !isCollected && Time.time - lastCollectionTime > cooldownTime)
        {
            PowerupManager.PowerupInfo powerupInfo = AssignRandomPowerup(carController);
            if (powerupInfo != null)
            {
                ApplyPowerup(carController, powerupInfo);
                isCollected = true;
                lastCollectionTime = Time.time;
                StartCoroutine(DeactivateAfterDelay());
            }
        }
    }

    private PowerupManager.PowerupInfo AssignRandomPowerup(CarController carController)
    {
        if (PowerupManager.Instance == null)
        {
            return null;
        }
        PowerupManager.PowerupInfo powerupInfo = PowerupManager.Instance.GetRandomPowerup(carController);
        if (powerupInfo == null)
        {
            return null;
        }
        return powerupInfo;
    }

    private void ApplyPowerup(CarController carController, PowerupManager.PowerupInfo powerupInfo)
    {
        if (powerupInfo == null || powerupInfo.applyEffect == null)
        {
            return;
        }
        carController.AddActivePowerup(powerupInfo);
        powerupInfo.applyEffect(carController);
    }

    private IEnumerator DeactivateAfterDelay()
    {
        if (powerupRenderer)
        {
            powerupRenderer.enabled = false;
        }
        
        Collider collider = GetComponent<Collider>();
        if (collider)
        {
            collider.enabled = false;
        }
        
        yield return new WaitForSeconds(0.1f); // Short delay before deactivating
        gameObject.SetActive(false);
    }
}