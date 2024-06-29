using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionManager : MonoBehaviour, IDataPersistence
{
    public List<GameObject> allCarPrefabs; // Remember to add all unlockable prefabs to this
    private List<GameObject> unlockedCarPrefabs; // The only ones the player has
    public TMPro.TextMeshProUGUI carInfoText;
    private int currentCarIndex = 0;
    private Vector3 carDisplayPosition = new Vector3(0f, 0.5f, 0f);
    public GameObject uiCanvas;

    private GameData gameData;

    void Start()
    {
        Debug.Log("CarSelectionManager Start");
        unlockedCarPrefabs = new List<GameObject>();
        SetupCars();

        if (DataPersistentManager.instance != null)
        {
            DataPersistentManager.instance.LoadGame();
        }
        else
        {
            Debug.LogError("DataPersistentManager not found");
        }
    }

    public void LoadData(GameData data)
    {
        Debug.Log($"CarSelectionManager LoadData: {data.DebugString()}");
        this.gameData = data;
        UpdateUnlockedCars();
        DisplayCurrentCar();
    }

    public void SaveData(ref GameData data)
    {
        // For the interface, doesn't need anything here
    }

    private void UpdateUnlockedCars()
    {
        if (gameData == null)
        {
            Debug.LogError("GameData is null in UpdateUnlockedCars");
            return;
        }

        unlockedCarPrefabs.Clear();
        foreach (GameObject carPrefab in allCarPrefabs)
        {
            if (gameData.IsCarUnlocked(carPrefab.name))
            {
                unlockedCarPrefabs.Add(carPrefab);
            }
        }
        Debug.Log($"Updated unlocked cars: {unlockedCarPrefabs.Count}. Names: {string.Join(", ", unlockedCarPrefabs.Select(c => c.name))}");
    }

    private void SetupCars()
    {
        foreach (GameObject carPrefab in allCarPrefabs)
        {
            carPrefab.transform.position = new Vector3(-1000, 0, 0);
            Rigidbody carRigidbody = carPrefab.GetComponent<Rigidbody>();
            if (carRigidbody != null)
            {
                carRigidbody.isKinematic = true;
            }
        }
    }

    private void DisplayCurrentCar()
    {
        foreach (GameObject car in allCarPrefabs)
        {
            car.transform.position = new Vector3(-1000, 0, 0);
        }

        if (unlockedCarPrefabs.Count > 0)
        {
            GameObject currentCar = unlockedCarPrefabs[currentCarIndex];
            currentCar.transform.position = carDisplayPosition;
            currentCar.transform.rotation = Quaternion.identity;
            CarController carInfo = currentCar.GetComponent<CarController>();
            UpdateCarInfoText(carInfo);
            StopAllCoroutines();
            StartCoroutine(RotateCarPrefab(currentCar));
        }
        else
        {
            carInfoText.text = "No cars unlocked!";
            //Debug.LogWarning("No cars are unlocked. Check if GameData is loaded correctly and car names match.");
        }
    }

    private void UpdateCarInfoText(CarController carInfo)
    {
        if (carInfo != null)
        {
            carInfoText.text = "Top Speed: " + carInfo.TopSpeed + "\n" +
                               "Grip: " + carInfo.TireRadius + "\n" +
                               "Handling: " + System.Math.Round(((double)carInfo.MaxSteeringAngle * 0.4 + (double)carInfo.TireRadius * 0.3 + (double)carInfo.Strength * 0.2 + (double)carInfo.Damping * 0.1) / 10, 2);
        }
        else
        {
            carInfoText.text = "Car info not available";
            //Debug.LogWarning("CarController component not found on the current car prefab.");
        }
    }

    public void CycleCarPrefabs(bool isNextCar)
    {
        if (unlockedCarPrefabs.Count == 0) return;

        StopAllCoroutines();

        currentCarIndex = isNextCar ?
            (currentCarIndex + 1) % unlockedCarPrefabs.Count :
            (currentCarIndex - 1 + unlockedCarPrefabs.Count) % unlockedCarPrefabs.Count;

        DisplayCurrentCar();
    }

    private IEnumerator RotateCarPrefab(GameObject carPrefab)
    {
        float rotationSpeed = 30f; // Reduced rotation speed
        while (true)
        {
            carPrefab.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void SelectCurrentCar()
    {
        if (unlockedCarPrefabs.Count == 0) return;

        GameObject selectedCarPrefab = unlockedCarPrefabs[currentCarIndex];
        Rigidbody carRigidbody = selectedCarPrefab.GetComponent<Rigidbody>();
        if (carRigidbody != null)
        {
            carRigidbody.isKinematic = false;
        }

        uiCanvas.gameObject.SetActive(false);
        PlayerPrefs.SetString("SelectedCarPrefab", selectedCarPrefab.name);
        StartCoroutine(WaitForCarToFallOutOfScreen(selectedCarPrefab));
    }

    private IEnumerator WaitForCarToFallOutOfScreen(GameObject carPrefab)
    {
        Quaternion originalRotation = carPrefab.transform.rotation;
        float fallThreshold = -10f;

        while (carPrefab.transform.position.y > fallThreshold)
        {
            yield return null;
        }
        carPrefab.transform.rotation = originalRotation;

        UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene");
    }
}