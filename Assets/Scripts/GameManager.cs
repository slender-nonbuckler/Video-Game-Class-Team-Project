using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject targetGameObject; // Assign this in the Inspector
    private GameData gameData;
    void Start()
    {
        if (PlayerPrefs.GetInt("NewGame", 0) == 1)
        {
            DataPersistentManager.instance.NewGame();
            targetGameObject.SetActive(false);
            PlayerPrefs.SetInt("NewGame", 0); // Reset the value
        }
    }

   
    private void OnApplicationQuit()
    {
        targetGameObject.SetActive(true);
        DataPersistentManager.instance.SaveGame();
    }
}
