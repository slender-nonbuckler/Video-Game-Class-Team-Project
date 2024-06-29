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
        if (powerupMaterial != null)
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
              //  Debug.LogError($"Powerup {gameObject.name}: No Renderer component found on this GameObject or its children");
                return;
            }
            else
            {
               // Debug.Log($"Powerup {gameObject.name}: Renderer found on child object: {powerupRenderer.gameObject.name}");
            }
        }

        if (powerupRenderer.sharedMaterial == null)
        {
           // Debug.LogError($"Powerup {gameObject.name}: Renderer has no material assigned");
            return;
        }

        // Create a material instance to avoid changing shared materials
        powerupMaterial = new Material(powerupRenderer.sharedMaterial);
        powerupRenderer.material = powerupMaterial;
       // Debug.Log($"Powerup {gameObject.name}: Renderer and material initialized successfully");
    }


    private void OnTriggerEnter(Collider other)
    {
        CarController carController = other.GetComponentInParent<CarController>();
        if (carController != null && !isCollected && Time.time - lastCollectionTime > cooldownTime)
        {
            PowerupManager.PowerupInfo powerupInfo = AssignRandomPowerup(carController);
            if (powerupInfo != null)
            {
               // Debug.Log($"Powerup {gameObject.name} triggered. Applying powerup: {powerupInfo.name}");
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
          //  Debug.LogError("PowerupManager.Instance is null!");
            return null;
        }
        PowerupManager.PowerupInfo powerupInfo = PowerupManager.Instance.GetRandomPowerup(carController);
        if (powerupInfo == null)
        {
           // Debug.LogError("Failed to get a random powerup from PowerupManager!");
            return null;
        }
       // Debug.Log($"Powerup {gameObject.name} assigned powerup: {powerupInfo.name}");
        return powerupInfo;
    }

    private void ApplyPowerup(CarController carController, PowerupManager.PowerupInfo powerupInfo)
    {
        if (powerupInfo == null || powerupInfo.applyEffect == null)
        {
            //Debug.LogError($"Powerup {gameObject.name}: PowerupInfo or applyEffect is null!");
            return;
        }
        carController.AddActivePowerup(powerupInfo);
        powerupInfo.applyEffect(carController);
        //Debug.Log($"Powerup {gameObject.name}: Applied effect. Duration: {powerupInfo.duration}");
    }

    private IEnumerator DeactivateAfterDelay()
    {
       // Debug.Log($"Powerup {gameObject.name}: Starting DeactivateAfterDelay coroutine");
        if (powerupRenderer != null)
        {
            powerupRenderer.enabled = false;
           // Debug.Log($"Powerup {gameObject.name}: Disabled Renderer");
        }
        else
        {
            //Debug.LogWarning($"Powerup {gameObject.name}: No Renderer component found");
        }
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
           // Debug.Log($"Powerup {gameObject.name}: Disabled Collider");
        }
        else
        {
         //   Debug.LogWarning($"Powerup {gameObject.name}: No Collider component found");
        }
        yield return new WaitForSeconds(0.1f); // Short delay before deactivating
        //Debug.Log($"Powerup {gameObject.name}: Deactivating gameObject");
        gameObject.SetActive(false);
    }
}