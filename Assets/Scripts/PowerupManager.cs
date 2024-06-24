using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [Header("Car Prefabs for Powerup")]
    public List<GameObject> carPrefabs;
    public TextMeshProUGUI powerupText;


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
        // Speed Boost (unchanged)
        availablePowerups.Add(new PowerupInfo
        {
            name = "Speed Boost",
            duration = 5f,
            applyEffect = (car) =>
            {
                float boostedSpeed = car.TopSpeed * 1.5f;
                powerupText.text = "Speed boost applied!";
                Debug.Log($"PowerupManager: Applying speed boost. Current speed: {car.TopSpeed}, Boosted speed: {boostedSpeed}");
                car.SetTemporaryTopSpeed(boostedSpeed);
            },
            removeEffect = (car) =>
            {
                Debug.Log($"PowerupManager: Removing speed boost. Current speed before reset: {car.TopSpeed}");
                car.ResetTopSpeed();
                powerupText.text = "";
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
                powerupText.text = "Size increase buff applied!";
                Debug.Log($"PowerupManager: Applying size increase. Current scale: {car.transform.localScale}");
                StartCoroutine(SmoothScale(car.transform, car.transform.localScale * 2f, 0.5f));
            },
            removeEffect = (car) =>
            {
                powerupText.text = "";
                Debug.Log($"PowerupManager: Removing size increase. Current scale: {car.transform.localScale}");
                StartCoroutine(SmoothScale(car.transform, car.transform.localScale * 0.5f, 0.5f));
            }
        });

        // Size Decrease
        availablePowerups.Add(new PowerupInfo
        {
            name = "Size Decrease",
            duration = 7f,
            applyEffect = (car) =>
            {
                powerupText.text = "Size decrease debuff applied!";
                Debug.Log($"PowerupManager: Applying size decrease. Current scale: {car.transform.localScale}");
                StartCoroutine(SmoothScale(car.transform, car.transform.localScale * 0.5f, 0.5f));
            },
            removeEffect = (car) =>
            {
                powerupText.text = "";
                Debug.Log($"PowerupManager: Removing size decrease. Current scale: {car.transform.localScale}");
                StartCoroutine(SmoothScale(car.transform, car.transform.localScale * 2f, 0.5f));
            }
        });

        availablePowerups.Add(new PowerupInfo
        {
            name = "Car Switch",
            duration = 10f,
            applyEffect = (car) =>
            {
                powerupText.text = "Your car has transformed into a race car!";
                StartCoroutine(SwitchCarCoroutine(car));
            },
            removeEffect = (car) =>
            {
               
            }
        });


        Debug.Log($"PowerupManager: Initialized {availablePowerups.Count} powerups");
    }
    private IEnumerator SwitchCarCoroutine(CarController currentCar)
    {
        GameObject originalCar = currentCar.gameObject;
        Vector3 currentPosition = originalCar.transform.position;
        Quaternion currentRotation = originalCar.transform.rotation;
        Vector3 currentVelocity = originalCar.GetComponent<Rigidbody>().velocity;
        Vector3 currentAngularVelocity = originalCar.GetComponent<Rigidbody>().angularVelocity;

        // Get the name of the current car prefab, which has "clone" for some reason
        string currentCarPrefabName = originalCar.name.Replace("(Clone)", "").Trim();

        // Randomly select a different car prefab
        GameObject newCarPrefab = GetRandomDifferentCarPrefab(currentCarPrefabName);
        if (newCarPrefab == null)
        {
            Debug.LogError("PowerupManager: No alternative car prefab found!");
            yield break;
        }

        // Instantiate the new car slightly above the current position for some collision avoidance?
        Vector3 spawnPosition = currentPosition + Vector3.up * 0.3f;
        GameObject newCar = Instantiate(newCarPrefab, spawnPosition, currentRotation);
        newCar.tag = "Player";

        // Set up the new car
        SetUpCar(newCar);

        // Set the velocity of the new car to match the original car
        Rigidbody newCarRigidbody = newCar.GetComponent<Rigidbody>();
        newCarRigidbody.velocity = currentVelocity;
        newCarRigidbody.angularVelocity = currentAngularVelocity;

        // Disable the original car
        originalCar.SetActive(false);

        Debug.Log($"PowerupManager: Switched to new car: {newCar.name}");

        // Wait for the duration of the powerup
        yield return new WaitForSeconds(10f);

        // Prepare to revert to the original car
        currentPosition = newCar.transform.position;
        currentRotation = newCar.transform.rotation;
        currentVelocity = newCarRigidbody.velocity;
        currentAngularVelocity = newCarRigidbody.angularVelocity;

        // Reactivate the original car slightly above the current position
        spawnPosition = currentPosition + Vector3.up * 0.3f;
        originalCar.transform.position = spawnPosition;
        originalCar.transform.rotation = currentRotation;
        originalCar.SetActive(true);

        // Set the velocity of the original car to match the new car
        Rigidbody originalCarRigidbody = originalCar.GetComponent<Rigidbody>();
        originalCarRigidbody.velocity = currentVelocity;
        originalCarRigidbody.angularVelocity = currentAngularVelocity;

        // Update camera and controls for the original car
        SetUpCar(originalCar);

        // Deactivate and destroy the temporary car
        newCar.SetActive(false);
        Destroy(newCar);

        Debug.Log("PowerupManager: Reverted to original car");
    }

    private GameObject GetRandomDifferentCarPrefab(string currentCarPrefabName)
    {
        List<GameObject> availablePrefabs = carPrefabs.Where(prefab => prefab.name != currentCarPrefabName).ToList();
        if (availablePrefabs.Count == 0)
        {
            return null;
        }
        return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
    }

    private void SetUpCar(GameObject car)
    {
        PlayerDriver playerDriver = car.AddComponent<PlayerDriver>();
        CarController carController = car.GetComponent<CarController>();
        playerDriver.SetCarController(carController);

        if (car.GetComponent<AiDriver>() != null)
        {
            car.GetComponent<AiDriver>().enabled = false;
        }

        // Update camera
        Cinemachine.CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Follow = car.transform;
            virtualCamera.LookAt = car.transform;
        }
    }
    private IEnumerator SmoothScale(Transform target, Vector3 endScale, float duration)
    {
        Vector3 startScale = target.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        target.localScale = endScale; // Ensure we end up at the exact desired scale
        Debug.Log($"PowerupManager: Scaling complete. New scale: {target.localScale}");
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