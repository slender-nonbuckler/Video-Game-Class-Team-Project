using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionManager : MonoBehaviour
{

    public List<GameObject> carPrefabs;
    public TMPro.TextMeshProUGUI carInfoText;
    private int currentCarIndex = 0; 
    private Vector3 carDisplayPosition = new Vector3(0f, 1.5f, 0f);
    public float rotationSpeed = 45f;

    void Start()
    {
        foreach (GameObject carPrefab in carPrefabs)
        {
            carPrefab.transform.position = new Vector3(-1000, 0, 0);
        }
        //For the first car
        carPrefabs[0].transform.position = carDisplayPosition;
        FakeCarInfoScript carInfo = carPrefabs[currentCarIndex].GetComponent<FakeCarInfoScript>();
        carInfoText.text = "Speed: " + carInfo.speed;
        StartCoroutine(RotateCarPrefab(carPrefabs[0]));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CycleCarPrefabs(bool isNextCar)
    {
        StopCoroutine(RotateCarPrefab(carPrefabs[currentCarIndex]));

        // Move the current car prefab out of the camera's view
        carPrefabs[currentCarIndex].transform.position = new Vector3(-1000, 0, 0);

        // Move to next car
        currentCarIndex = isNextCar ? (currentCarIndex + 1) % carPrefabs.Count : (currentCarIndex - 1 + carPrefabs.Count) % carPrefabs.Count;


        carPrefabs[currentCarIndex].transform.position = carDisplayPosition;
        FakeCarInfoScript carInfo = carPrefabs[currentCarIndex].GetComponent<FakeCarInfoScript>();
        carInfoText.text = "Speed: " + carInfo.speed;

        // Spin!
        StartCoroutine(RotateCarPrefab(carPrefabs[currentCarIndex]));
    }
    private IEnumerator RotateCarPrefab(GameObject carPrefab)
    {
        float maxAnglePerSecond = 90f; 
        float elapsedTime = 0f;
        float targetAngle = 0f;

        while (true)
        {
            
            if (carPrefab.transform.position == carDisplayPosition)
            {
                elapsedTime += Time.deltaTime;
                targetAngle = maxAnglePerSecond * elapsedTime;
                carPrefab.transform.rotation = Quaternion.Euler(carPrefab.transform.rotation.eulerAngles.x, targetAngle, carPrefab.transform.rotation.eulerAngles.z);

                if (elapsedTime >= 4f)
                {
                    elapsedTime = 0f;
                }
            }
            else
            {
                elapsedTime = 0f;
            }

            yield return null;
        }
    }
    public void SelectCurrentCar()
    {
        GameObject selectedCarPrefab = carPrefabs[currentCarIndex];

        // Ideally, this would go into some sort of state manager for the game
        // Then, cut to a new scene for the game
        Debug.Log("Selected Car: " + selectedCarPrefab.name);
    }
}
