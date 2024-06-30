using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelectionManager : MonoBehaviour, IDataPersistence
{
    public List<GameObject> allCarPrefabs;
    private List<GameObject> unlockedCarPrefabs;
    private List<GameObject> lockedCarPrefabs;
    public TMPro.TextMeshProUGUI carInfoText;
    public TMPro.TextMeshProUGUI playerMoneyText;
    private int currentCarIndex = 0;
    private Vector3 carDisplayPosition = new Vector3(0f, 0.5f, 0f);
    public GameObject uiCanvas;

    public Button ButtonUnlock;
    public Button ButtonSelect;

    private GameData gameData;

    void Start()
    {
        Debug.Log("CarSelectionManager Start");
        unlockedCarPrefabs = new List<GameObject>();
        lockedCarPrefabs = new List<GameObject>();
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
        UpdateCarLists();
        DisplayCurrentCar();
        UpdatePlayerMoneyDisplay(); 
    }

    public void SaveData(ref GameData data)
    {
        // For the interface, doesn't need anything here
    }

    private void UpdateCarLists()
    {
        if (gameData == null)
        {
            //Debug.LogError("GameData is null in UpdateCarLists");
            return;
        }

        unlockedCarPrefabs.Clear();
        lockedCarPrefabs.Clear();
        foreach (GameObject carPrefab in allCarPrefabs)
        {
            if (gameData.IsCarUnlocked(carPrefab.name))
            {
                unlockedCarPrefabs.Add(carPrefab);
            }
            else
            {
                lockedCarPrefabs.Add(carPrefab);
            }
        }
        Debug.Log($"Updated unlocked cars: {unlockedCarPrefabs.Count}. Names: {string.Join(", ", unlockedCarPrefabs.Select(c => c.name))}");
        Debug.Log($"Updated locked cars: {lockedCarPrefabs.Count}. Names: {string.Join(", ", lockedCarPrefabs.Select(c => c.name))}");
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

        List<GameObject> allCars = new List<GameObject>(unlockedCarPrefabs);
        allCars.AddRange(lockedCarPrefabs);

        if (allCars.Count > 0)
        {
            GameObject currentCar = allCars[currentCarIndex];
            currentCar.transform.position = carDisplayPosition;
            currentCar.transform.rotation = Quaternion.identity;
            CarController carInfo = currentCar.GetComponent<CarController>();
            UpdateCarInfoText(carInfo);
            UpdateButtonStatus(currentCar);
            StopAllCoroutines();
            StartCoroutine(RotateCarPrefab(currentCar));
        }
        else
        {

            // Only in the worst case!!!
            carInfoText.text = "No cars available!";
            ButtonSelect.gameObject.SetActive(false);
            ButtonUnlock.gameObject.SetActive(false);
        }
    }

    private void UpdateCarInfoText(CarController carInfo)
    {
        if (carInfo != null)
        {
            CarCost carCost = carInfo.GetComponent<CarCost>();
            int cost = carCost != null ? carCost.Cost : 0;

            string infoText = "";

            if (cost > 0)
            {
                infoText += $"Cost: ${cost}\n";
            }

            infoText += $"Top Speed: {carInfo.TopSpeed}\n" +
                        $"Grip: {carInfo.TireRadius}\n" +
                        $"Handling: {System.Math.Round(((double)carInfo.MaxSteeringAngle * 0.4 + (double)carInfo.TireRadius * 0.3 + (double)carInfo.Strength * 0.2 + (double)carInfo.Damping * 0.1) / 10, 2)}";

            carInfoText.text = infoText;
        }
        else
        {
            carInfoText.text = "Car info not available";
        }
    }

    private void UpdateButtonStatus(GameObject car)
    {
        bool isUnlocked = unlockedCarPrefabs.Contains(car);
        ButtonSelect.gameObject.SetActive(isUnlocked);
        ButtonUnlock.gameObject.SetActive(!isUnlocked);

        if (!isUnlocked)
        {
            CarCost carCost = car.GetComponent<CarCost>();
            if (carCost != null)
            {
                ButtonUnlock.interactable = gameData.getMoney >= carCost.Cost;
            }
        }
    }

    private void UpdatePlayerMoneyDisplay()
    {
        if (playerMoneyText != null)
        {
            playerMoneyText.text = $"Money: ${gameData.getMoney}";
        }
    }

    public void CycleCarPrefabs(bool isNextCar)
    {
        List<GameObject> allCars = new List<GameObject>(unlockedCarPrefabs);
        allCars.AddRange(lockedCarPrefabs);

        if (allCars.Count == 0) return;

        StopAllCoroutines();

        currentCarIndex = isNextCar ?
            (currentCarIndex + 1) % allCars.Count :
            (currentCarIndex - 1 + allCars.Count) % allCars.Count;

        DisplayCurrentCar();
    }

    private IEnumerator RotateCarPrefab(GameObject carPrefab)
    {
        float rotationSpeed = 30f;
        while (true)
        {
            carPrefab.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void SelectCurrentCar()
    {
        List<GameObject> allCars = new List<GameObject>(unlockedCarPrefabs);
        allCars.AddRange(lockedCarPrefabs);

        if (allCars.Count == 0) return;

        GameObject selectedCarPrefab = allCars[currentCarIndex];

        if (unlockedCarPrefabs.Contains(selectedCarPrefab))
        {
            Rigidbody carRigidbody = selectedCarPrefab.GetComponent<Rigidbody>();
            if (carRigidbody != null)
            {
                carRigidbody.isKinematic = false;
            }

            uiCanvas.gameObject.SetActive(false);
            PlayerPrefs.SetString("SelectedCarPrefab", selectedCarPrefab.name);
            StartCoroutine(WaitForCarToFallOutOfScreen(selectedCarPrefab));
        }
    }

    public void UnlockCurrentCar()
    {

        List<GameObject> allCars = new List<GameObject>(unlockedCarPrefabs);
        allCars.AddRange(lockedCarPrefabs);

        if (allCars.Count == 0) return;

        GameObject selectedCarPrefab = allCars[currentCarIndex];

        if (!unlockedCarPrefabs.Contains(selectedCarPrefab))
        {
            CarCost carCost = selectedCarPrefab.GetComponent<CarCost>();
            if (carCost != null)
            {
                int cost = carCost.Cost;
                if (gameData.getMoney >= cost)
                {
                    // Take money, yoink
                    gameData.SetMoney(gameData.getMoney - cost);

                    gameData.UnlockCar(selectedCarPrefab.name);

                    // Save the game
                    DataPersistentManager.instance.SaveGame();

                    // Update lists and display
                    UpdateCarLists();
                    DisplayCurrentCar();
                    UpdatePlayerMoneyDisplay();
                    UpdateButtonStatus(selectedCarPrefab);

                    Debug.Log($"Car unlocked: {selectedCarPrefab.name}. Remaining money: ${gameData.getMoney}");
                }
                else
                {
                    Debug.Log($"Not enough money to unlock {selectedCarPrefab.name}. Required: ${cost}, Available: ${gameData.getMoney}");
                }
            }
            else
            {
                Debug.LogError($"CarCost component not found on {selectedCarPrefab.name}");
            }
        }
        else
        {
            Debug.Log($"Car {selectedCarPrefab.name} is already unlocked");
        }
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

        SceneManager.LoadScene("TutorialScene");
    }
}