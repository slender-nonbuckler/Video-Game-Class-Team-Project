using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private DataPersistentManager dataPersistenceManager;

    void Start()
    {
        Debug.Log("MenuController Start");
        dataPersistenceManager = DataPersistentManager.instance;
        if (dataPersistenceManager == null)
        {
           // Debug.LogError("DataPersistenceManager not found. Initialize it");
        }
    }

    public void StartNewGame()
    {
        if (dataPersistenceManager != null)
        {
            dataPersistenceManager.NewGame();
            PlayerPrefs.SetInt("TutorialCompleted", 0);
            PlayerPrefs.Save();
            SceneManager.LoadScene("CarSelection");
        }
        else
        {
            //Debug.LogError("DataPersistenceManager is null.");
        }
    }

    public void ContinueGame()
    {
        Debug.Log("Continuing Game");
        if (dataPersistenceManager != null)
        {
            if (dataPersistenceManager.HasGameData())
            {
                dataPersistenceManager.LoadGame();
                SceneManager.LoadScene("CarSelection");
            }
            else
            {
               // Debug.LogWarning("No saved game found. Starting a new game instead.");
                StartNewGame();
            }
        }
        else
        {
            //Debug.LogError("DataPersistenceManager is null.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}