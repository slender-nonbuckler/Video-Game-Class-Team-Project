using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutorialCarController : MonoBehaviour
{
    public List<GameObject> carPrefabs;
    private GameObject selectedCar;
    public CinemachineVirtualCamera virtualCamera;
    // Start is called before the first frame update
    void Start()
    {
        if (carPrefabs == null)
        {
            Debug.Log("CarPrefabs were null.");
        }
        else
        {
            Debug.Log("Length of car prefabs: " + carPrefabs.Count);
        }
        foreach (GameObject prefab in carPrefabs)
        {
            Debug.Log("car name: " + prefab.name);
        }

        
        string selectedCarPrefabName = PlayerPrefs.GetString("SelectedCarPrefab");
        selectedCar = InstantiateSelectedCar(selectedCarPrefabName, new Vector3(-0.41f, 13f, -13f));
        SetUpDriver(selectedCar);
    }

    private GameObject InstantiateSelectedCar(string prefabName, Vector3 position)
    {
        foreach (GameObject prefab in carPrefabs)
        {
            if (prefab.name == prefabName)
            {
                GameObject car = Instantiate(prefab, position, prefab.transform.rotation);
                car.tag = "Player";
                return car;
            }
        }
        return null;
    }

    private void SetUpDriver(GameObject car)
    {
        
        if (car == null)
        {
            Debug.Log("car was null. init now");
            car = Instantiate(carPrefabs[0], new Vector3(0f, 10f, 0f), carPrefabs[0].transform.rotation);
        }
        else
        {
            Debug.Log(car + " was not null.");
        }
        
        PlayerDriver playerDriver = car.AddComponent<PlayerDriver>();
        CarController carController = car.GetComponent<CarController>();

        if (playerDriver == null)
        {
            Debug.Log("playerDriver was null");
        }
        
        if (carController == null)
        {
            Debug.Log("carController was null");
        }
        
        playerDriver.SetCarController(carController);

        if (car.GetComponent<AiDriver>() == null)
        { 
            Debug.Log("car.GetComponent<AiDriver>() was null");
        }
        
        car.GetComponent<AiDriver>().enabled = false;
        
        
        if (virtualCamera != null)
        {
            virtualCamera.Follow = car.transform;
            virtualCamera.LookAt = car.transform;
        }
        else
        {
            Debug.Log("virtualCamera was null");
        }
    }

    private void Update()
    {
        if(selectedCar.transform.position.y <= -10f)
        {
            selectedCar.transform.rotation = carPrefabs[0].transform.rotation;
            Rigidbody carBody = selectedCar.GetComponent<Rigidbody>();
            if(carBody != null)
            {
                carBody.velocity = new Vector3(0, 0, 0);
            }
            selectedCar.transform.position = new Vector3(-0.41f, 20f, -5f);
        }
    }

}