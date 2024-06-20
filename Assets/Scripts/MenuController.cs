using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private DataPersistentManager dataPersistenceManager;
    private GameData gameData;
   
   void Start()
    {
        dataPersistenceManager = FindObjectOfType<DataPersistentManager>();
        if (dataPersistenceManager == null)
        {
            Debug.LogError("DataPersistenceManager not found in the scene.");
        }
    }
    public void StartButton()
    {   
        DataPersistentManager.instance.SetShouldLoadGame();
        //DataPersistentManager.instance.NewGame();
        SceneManager.LoadScene(1);
        
    }
    public void QuitButton()
    {
        Application.Quit();
    }
   

    public void ContinueButton()
    {
        if (dataPersistenceManager != null)
        {
            
            DataPersistentManager.instance.LoadGame();
            Debug.LogError(dataPersistenceManager.shouldLoadGame);
        }
        SceneManager.LoadScene(1);
    }
    

}
