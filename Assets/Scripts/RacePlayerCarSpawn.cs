using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.PlayerPrefs;

public class RacePlayerCarSpawn : MonoBehaviour
{
    public List<GameObject> carPrefabs;
    public GameObject selectedCar;
    public CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        string selectedCarPrefabName = PlayerPrefs.GetString("SelectedCarPrefab");
        selectedCar = InstantiateSelectedCar(selectedCarPrefabName, new Vector3(200f, 10f, 105.5f));
        SetUpDriver(selectedCar);
    }
    private GameObject InstantiateSelectedCar(string prefabName, Vector3 position)
    {
        foreach (GameObject prefab in carPrefabs)
        {
            if (prefab.name == prefabName)
            {
                Quaternion rotation = Quaternion.Euler(0, 90, 0);
                GameObject car = Instantiate(prefab, position, rotation);
                car.tag = "Player";
                return car;
            }
        }
        return null;
    }
    private void SetUpDriver(GameObject car)
    {
        PlayerDriver playerDriver = car.AddComponent<PlayerDriver>();
        CarController carController = car.GetComponent<CarController>();
        playerDriver.SetCarController(carController);
        car.GetComponent<AiDriver>().enabled = false;
        if (virtualCamera != null)
        {
            virtualCamera.Follow = car.transform;
            virtualCamera.LookAt = car.transform;
        }
    }
}
