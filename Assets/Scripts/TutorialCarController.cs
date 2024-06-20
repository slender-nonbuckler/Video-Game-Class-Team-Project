using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.PlayerPrefs;

public class TutorialCarController : MonoBehaviour
{
    public List<GameObject> carPrefabs;
    private GameObject selectedCar;
    public CinemachineVirtualCamera virtualCamera;
    // Start is called before the first frame update
    void Start()
    {
        string selectedCarPrefabName = PlayerPrefs.GetString("SelectedCarPrefab");
        selectedCar = InstantiateSelectedCar(selectedCarPrefabName, new Vector3(17f, 0f, -4.6f));
        SetUpDriver(selectedCar);
    }

    private GameObject InstantiateSelectedCar(string prefabName, Vector3 position)
    {
        foreach (GameObject prefab in carPrefabs)
        {
            if (prefab.name == prefabName)
            {
                return Instantiate(prefab, position, prefab.transform.rotation);
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