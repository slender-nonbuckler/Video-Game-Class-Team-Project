using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialMenu : MonoBehaviour, IDataPersistence
{
    public Canvas pauseMenuCanvas;
    public Button finishTutorial;
    private bool isPaused = false;
    private int currentMoney = 0;

    void Start()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.gameObject.SetActive(isPaused);
        }
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ReturnToMainMenu()
    {
        SaveAndExit();
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        SaveAndExit();
        Application.Quit();
    }

    public void FinishTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        SaveAndExit();
        SceneManager.LoadScene("Track1v2"); // Update this later when race scene exists
    }

    private void SaveAndExit()
    {
        Time.timeScale = 1f;
        DataPersistentManager.instance.SaveGame();
    }

    public void LoadData(GameData data)
    {
        this.currentMoney = data.Money;
        Debug.Log($"Loaded Money: {this.currentMoney}");
    }

    public void SaveData(ref GameData data)
    {
        // We don't need to modify the money here, as the Collectible script will handle it
        Debug.Log($"Current Money: {data.Money}");
    }
}