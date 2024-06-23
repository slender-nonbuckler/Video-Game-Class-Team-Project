using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    [System.Serializable]
    public class PowerupInfo
    {
        public string name;
        public float duration = 5f;
        public System.Action<CarController> applyEffect;
        public System.Action<CarController> removeEffect;
    }
    private List<PowerupInfo> availablePowerups = new List<PowerupInfo>();
    public static PowerupManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePowerups();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePowerups()
    {
        // Speed Boost
        availablePowerups.Add(new PowerupInfo
        {
            name = "Speed Boost",
            duration = 5f,
            applyEffect = (car) =>
            {
                float boostedSpeed = car.TopSpeed * 1.5f;
                Debug.Log($"PowerupManager: Applying speed boost. Current speed: {car.TopSpeed}, Boosted speed: {boostedSpeed}");
                car.SetTemporaryTopSpeed(boostedSpeed);
            },
            removeEffect = (car) =>
            {
                Debug.Log($"PowerupManager: Removing speed boost. Current speed before reset: {car.TopSpeed}");
                car.ResetTopSpeed();
                Debug.Log($"PowerupManager: Speed after reset: {car.TopSpeed}");
            }
        });

        // Size Increase
        availablePowerups.Add(new PowerupInfo
        {
            name = "Size Increase",
            duration = 7f,
            applyEffect = (car) =>
            {
                Debug.Log($"PowerupManager: Applying size increase. Current scale: {car.transform.localScale}");
                car.transform.localScale *= 2f;
                Debug.Log($"PowerupManager: New scale after increase: {car.transform.localScale}");
            },
            removeEffect = (car) =>
            {
                Debug.Log($"PowerupManager: Removing size increase. Current scale: {car.transform.localScale}");
                car.transform.localScale /= 2f;
                Debug.Log($"PowerupManager: New scale after decrease: {car.transform.localScale}");
            }
        });
        // Size Decrease

        availablePowerups.Add(new PowerupInfo
        {
            name = "Size Decrease",
            duration = 7f,
            applyEffect = (car) =>
            {
                Debug.Log($"PowerupManager: Applying size decrease. Current scale: {car.transform.localScale}");
                car.transform.localScale *= 0.5f;
                Debug.Log($"PowerupManager: New scale after decrease: {car.transform.localScale}");
            },
            removeEffect = (car) =>
            {
                Debug.Log($"PowerupManager: Removing size decrease. Current scale: {car.transform.localScale}");
                car.transform.localScale *=2f;
                Debug.Log($"PowerupManager: New scale restoring og size: {car.transform.localScale}");
            }
        });

        Debug.Log($"PowerupManager: Initialized {availablePowerups.Count} powerups");
    }

    public PowerupInfo GetRandomPowerup()
    {
        if (availablePowerups.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePowerups.Count);
            Debug.Log($"PowerupManager: Returning random powerup: {availablePowerups[randomIndex].name}");
            return availablePowerups[randomIndex];
        }
        Debug.LogError("PowerupManager: No powerups available!");
        return null;
    }
}