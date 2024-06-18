using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionManager : MonoBehaviour
{

    public List<GameObject> carPrefabs;
    public TMPro.TextMeshProUGUI carInfoText;
    private int currentCarIndex = 0; 
    private Vector3 carDisplayPosition = new Vector3(0f, 0.5f, 0f);

    void Start()
    {
        foreach (GameObject carPrefab in carPrefabs)
        {
            carPrefab.transform.position = new Vector3(-1000, 0, 0);
            Rigidbody carRigidbody = carPrefab.GetComponent<Rigidbody>();
            carRigidbody.isKinematic = true; // Disable physics so the car does not fall through the screen
        }
        //For the first car
        carPrefabs[0].transform.position = carDisplayPosition;
        CarController carInfo = carPrefabs[currentCarIndex].GetComponent<CarController>();
        carInfoText.text = "Top Speed: " + carInfo.TopSpeed + "\n" +
                    "Grip: " + carInfo.TireRadius + "\n" +
                   "Handling: " + System.Math.Round(((double)carInfo.MaxSteeringAngle * 0.4 + (double)carInfo.TireRadius * 0.3 + (double)carInfo.Strength * 0.2 + (double)carInfo.Damping * 0.1) / 10, 2);
        StartCoroutine(RotateCarPrefab(carPrefabs[0]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CycleCarPrefabs(bool isNextCar)
    {
        // Stop the current coroutine, this stops the cars from spinning faster when it is selected repeatedly
        StopAllCoroutines();

        carPrefabs[currentCarIndex].transform.rotation = Quaternion.identity;
        carPrefabs[currentCarIndex].transform.position = new Vector3(-1000, 0, 0);

        currentCarIndex = isNextCar ? (currentCarIndex + 1) % carPrefabs.Count : (currentCarIndex - 1 + carPrefabs.Count) % carPrefabs.Count;

        carPrefabs[currentCarIndex].transform.position = carDisplayPosition;
        CarController carInfo = carPrefabs[currentCarIndex].GetComponent<CarController>();
        carInfoText.text = "Top Speed: " + carInfo.TopSpeed + "\n" +
                    "Grip: " + carInfo.TireRadius + "\n" +
                   "Handling: " + System.Math.Round(((double)carInfo.MaxSteeringAngle * 0.4 + (double)carInfo.TireRadius * 0.3 + (double)carInfo.Strength * 0.2 + (double)carInfo.Damping * 0.1) / 10, 2);

        // Spin!
        StartCoroutine(RotateCarPrefab(carPrefabs[currentCarIndex]));
    }
    private IEnumerator RotateCarPrefab(GameObject carPrefab)
    {
        float rotationSpeed = 150f; 
        float elapsedTime = 0f;

        while (true)
        {
            if (Vector3.Distance(carPrefab.transform.position, carDisplayPosition) < 0.1f)
            {
                elapsedTime += Time.deltaTime;
                float rotationAmount = rotationSpeed * Time.deltaTime;
                carPrefab.transform.Rotate(Vector3.up, rotationAmount);

                if (elapsedTime >= 4f)
                {
                    elapsedTime = 0f;
                }
            }
            else
            {
                elapsedTime = 0f;
                carPrefab.transform.rotation = Quaternion.identity; // Reset rotation
            }

            yield return null;
        }
    }
    public void SelectCurrentCar()
    {
        GameObject selectedCarPrefab = carPrefabs[currentCarIndex];
        foreach (GameObject carPrefab in carPrefabs)
        {
            carPrefab.transform.position = new Vector3(-1000, 0, 0);
            Rigidbody carRigidbody = carPrefab.GetComponent<Rigidbody>();
            carRigidbody.isKinematic = false;  //Re-enable all rigidbodies
        }
        
        // Ideally, this would go into some sort of state manager for the game
        // Then, cut to a new scene for the game
        Debug.Log("Selected Car: " + selectedCarPrefab.name);
    }
}
