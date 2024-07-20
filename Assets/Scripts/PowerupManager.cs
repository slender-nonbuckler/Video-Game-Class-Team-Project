using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerupManager : MonoBehaviour
{
    [System.Serializable]
    public class PowerupInfo
    {
        public string name;
        public float duration = 5f;
        public System.Func<CarController, bool> isAvailable;
        public System.Action<CarController> applyEffect;
        public System.Action<CarController> removeEffect;
    }

    private List<PowerupInfo> availablePowerups = new List<PowerupInfo>();
    public static PowerupManager Instance { get; private set; }
    [Header("Car Prefabs for Powerup")]
    public List<GameObject> carPrefabs;
    public TextMeshProUGUI powerupText;

    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    public float defaultFOV = 60f;
    public Vector3 defaultFollowOffset = new Vector3(0f, 5f, -10f);

    private Coroutine activePowerupCoroutine;

    private void OnEnable()
    {
        ResetManager();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetManager();
    }

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

    private void ResetManager()
    {
        if (activePowerupCoroutine != null)
        {
            StopCoroutine(activePowerupCoroutine);
            //resetting powerupmanager on scene restart
            activePowerupCoroutine = null;
        }

        FindCamera();
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.FieldOfView = defaultFOV;
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = defaultFollowOffset;
            }
        }
    }

    private void FindCamera()
    {
        if (!virtualCamera)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
    }

    private void InitializePowerups()
    {
        // Size Increase
        availablePowerups.Add(new PowerupInfo
        {
            name = "Size Increase",
            duration = 7f,
            applyEffect = (car) =>
            {
                StartCoroutine(ApplyPowerupEffect(car, car.transform.localScale * 2f, true));
            },
            removeEffect = (car) =>
            {
                StartCoroutine(SmoothScale(car.transform, Vector3.one, 0.5f));
                StartCoroutine(ResetCameraEffect(car));
            }
        });
        
        // Size Decrease
        availablePowerups.Add(new PowerupInfo
        {
            name = "Size Decrease",
            duration = 7f,
            applyEffect = (car) =>
            {
                StartCoroutine(ApplyPowerupEffect(car, car.transform.localScale * 0.5f, false));
            },
            removeEffect = (car) =>
            {
                StartCoroutine(SmoothScale(car.transform, Vector3.one, 0.5f));
                StartCoroutine(ResetCameraEffect(car));
            }
        });

        // Car Switch
        availablePowerups.Add(new PowerupInfo
        {
            name = "Car Switch",
            duration = 10f,
            isAvailable = (car) => car.gameObject.CompareTag("Player"),  //AI cars shouldn't get a car switch (for now)
            applyEffect = (car) =>
            {
                StartCoroutine(SwitchCarCoroutine(car));
            },
            removeEffect = (car) =>
            {
            }
        });
    }

    private IEnumerator CameraUpdate(float targetFOV, Vector3 targetOffset, float duration)
    {
        FindCamera();
        if (virtualCamera != null)
        {
            float startFOV = virtualCamera.m_Lens.FieldOfView;
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                Vector3 startOffset = transposer.m_FollowOffset;

                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
                    transposer.m_FollowOffset = Vector3.Lerp(startOffset, targetOffset, t);

                    yield return null;
                }

                virtualCamera.m_Lens.FieldOfView = targetFOV;
                transposer.m_FollowOffset = targetOffset;
            }
        }
    }

    public PowerupInfo GetRandomPowerup(CarController car)
    {
        List<PowerupInfo> availableForCar = availablePowerups.Where(p => p.isAvailable == null || p.isAvailable(car)).ToList();
        if (availableForCar.Count > 0)
        {
            int randomIndex = Random.Range(0, availableForCar.Count);
            return availableForCar[randomIndex];
        }
        return null;
    }

    private IEnumerator SwitchCarCoroutine(CarController currentCar)
    {
        GameObject originalCar = currentCar.gameObject;
        Vector3 currentPosition = originalCar.transform.position;
        Quaternion currentRotation = originalCar.transform.rotation;
        Vector3 currentVelocity = originalCar.GetComponent<Rigidbody>().velocity;
        Vector3 currentAngularVelocity = originalCar.GetComponent<Rigidbody>().angularVelocity;

        RaceId originalRaceId = originalCar.GetComponent<RaceId>();

        // Get the name of the current car prefab, which has "clone" for some reason
        string currentCarPrefabName = originalCar.name.Replace("(Clone)", "").Trim();

        GameObject newCarPrefab = GetRandomDifferentCarPrefab(currentCarPrefabName);
        if (!newCarPrefab)
        {
            yield break;
        }

        Vector3 spawnPosition = currentPosition + Vector3.up * 0.3f;
        GameObject newCar = Instantiate(newCarPrefab, spawnPosition, currentRotation);
        newCar.tag = "Player";

        // Set up the new car
        SetUpCar(newCar, originalRaceId);

        Rigidbody newCarRigidbody = newCar.GetComponent<Rigidbody>();
        newCarRigidbody.velocity = currentVelocity;
        newCarRigidbody.angularVelocity = currentAngularVelocity;

        originalCar.SetActive(false);

        yield return new WaitForSeconds(10f);

        currentPosition = newCar.transform.position;
        currentRotation = newCar.transform.rotation;
        currentVelocity = newCarRigidbody.velocity;
        currentAngularVelocity = newCarRigidbody.angularVelocity;

        spawnPosition = currentPosition + Vector3.up * 0.3f;
        originalCar.transform.position = spawnPosition;
        originalCar.transform.rotation = currentRotation;
        originalCar.SetActive(true);

        Rigidbody originalCarRigidbody = originalCar.GetComponent<Rigidbody>();
        originalCarRigidbody.velocity = currentVelocity;
        originalCarRigidbody.angularVelocity = currentAngularVelocity;

        // Update camera and controls for the original car
        SetUpCar(originalCar, originalRaceId);

        newCar.SetActive(false);
        Destroy(newCar);
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

    private void SetUpCar(GameObject car, RaceId originalRaceId)
    {
        PlayerDriver playerDriver = car.AddComponent<PlayerDriver>();
        CarController carController = car.GetComponent<CarController>();
        playerDriver.SetCarController(carController);

        if (originalRaceId) {
            RaceId[] raceIds = car.GetComponents<RaceId>();
            foreach (RaceId raceId in raceIds) {
                raceId.id = originalRaceId.id;
            }
            
            if (raceIds.Length <= 0) {
                car.AddComponent<RaceId>().id = originalRaceId.id;
            }
        }

        AiDriver aiDriver = car.GetComponent<AiDriver>();
        if (aiDriver)
        {
            aiDriver.enabled = false;
        }

        FindCamera();
        // Update camera
        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera)
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

        target.localScale = endScale;
    }

    public PowerupInfo GetRandomPowerup()
    {
        if (availablePowerups.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePowerups.Count);
            return availablePowerups[randomIndex];
        }
        return null;
    }

    private IEnumerator ApplyPowerupEffect(CarController car, Vector3 targetScale, bool isIncreasing)
    {
        yield return StartCoroutine(SmoothScale(car.transform, targetScale, 0.5f));

        if (car.gameObject.CompareTag("Player"))
        {
            float targetFOV;
            Vector3 targetOffset;

            if (isIncreasing)
            {
                targetFOV = 90f;
                targetOffset = new Vector3(defaultFollowOffset.x, 8f, defaultFollowOffset.z * 1.5f);
            }
            else
            {
                targetFOV = 30f;
                targetOffset = new Vector3(defaultFollowOffset.x, 3f, defaultFollowOffset.z * 0.5f);
            }

            yield return StartCoroutine(CameraUpdate(targetFOV, targetOffset, 0.5f));
        }
    }

    public void ApplyPowerup(PowerupInfo powerup, CarController car)
    {
        FindCamera();
        if (activePowerupCoroutine != null)
        {
            StopCoroutine(activePowerupCoroutine);
        }
        activePowerupCoroutine = StartCoroutine(PowerupSequence(powerup, car));
    }

    private IEnumerator PowerupSequence(PowerupInfo powerup, CarController car)
    {
        powerup.applyEffect(car);
        yield return new WaitForSeconds(powerup.duration);
        powerup.removeEffect(car);

        activePowerupCoroutine = null;
    }

    private IEnumerator ResetCameraEffect(CarController car)
    {
        if (car.gameObject.CompareTag("Player"))
        {
            yield return StartCoroutine(CameraUpdate(defaultFOV, defaultFollowOffset, 0.5f));
        }
    }
}