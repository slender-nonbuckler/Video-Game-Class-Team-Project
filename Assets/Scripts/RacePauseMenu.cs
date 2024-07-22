using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RacePauseMenu : MonoBehaviour, IDataPersistence
{
    public Canvas PauseCanvas;
    public Canvas EndGameCanvas;
    public TMPro.TextMeshProUGUI PlacementText;
    public TMPro.TextMeshProUGUI MoneyEarnedText;
    private bool isPaused = false;
    public RaceManager raceManager;
    private int currentMoney;

    private int moneyEarned;
    private int playerPlacement;
    // Start is called before the first frame update
    void Start()
    {
        if (PauseCanvas != null)
        {
            PauseCanvas.gameObject.SetActive(false);
        }
        if (EndGameCanvas != null)
        {
            EndGameCanvas.gameObject.SetActive(false);
        }
        if (raceManager != null)
        {
            raceManager.OnPlayerFinish.AddListener(ShowEndGameCanvas);
            raceManager.OnPlayerFinish.AddListener(UpdateEndGameInfo);
        }
    }

    private void ShowEndGameCanvas()
    {
        if (EndGameCanvas != null)
        {
            EndGameCanvas.gameObject.SetActive(true);
        }
    }

    private void UpdateEndGameInfo()
    {
        // Wait for a short time to ensure RaceManager has finished calculations, had some issues with this timing before.
        Invoke("FetchAndDisplayRaceResults", 0.1f);
    }

    private void FetchAndDisplayRaceResults()
    {
        playerPlacement = raceManager.GetPlayerPlacement();
        moneyEarned = raceManager.GetMoneyEarned();

        if (PlacementText != null)
        {
            PlacementText.text = $"Well done! You placed: {GetOrdinal(playerPlacement)}";
        }
        if (MoneyEarnedText != null)
        {
            MoneyEarnedText.text = $"You earned {moneyEarned} money for your {(playerPlacement == 1 ? "win" : "race")}";
        }

        //Debug.Log($"Race Results - Placement: {playerPlacement}, Money Earned: {moneyEarned}");
    }

    private string GetOrdinal(int number)
    {
        if (number <= 0) return number.ToString();

        switch (number % 100)
        {
            case 11:
            case 12:
            case 13:
                return number + "th";
        }

        switch (number % 10)
        {
            case 1:
                return number + "st";
            case 2:
                return number + "nd";
            case 3:
                return number + "rd";
            default:
                return number + "th";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        // Players cannot pause when the race has ended (because EndGameCanvas has similar buttons)
        if (EndGameCanvas.gameObject.activeSelf)
            return;

        isPaused = !isPaused;
        if (PauseCanvas != null)
        {
            PauseCanvas.gameObject.SetActive(isPaused);
        }
        Time.timeScale = isPaused ? 0f : 1f;
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        DataPersistentManager.instance?.SaveGame();
        SceneManager.LoadScene("Menu");
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        DataPersistentManager.instance?.SaveGame();
        Application.Quit();
    }
    public void RestartRound()
    {
        Time.timeScale = 1f;
        // SceneManager.LoadScene("Track1v2");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void RaceAgain()
    {
        Time.timeScale = 1f;
        DataPersistentManager.instance?.SaveGame();
        SceneManager.LoadScene("Track1v2");
    }

    public void LoadTrack1()
    {
        Time.timeScale = 1f;
        DataPersistentManager.instance?.SaveGame();
        SceneManager.LoadScene("Track1v2");
    }

    public void LoadCrashCove()
    {
        Time.timeScale = 1f;
        DataPersistentManager.instance?.SaveGame();
        SceneManager.LoadScene("CrashCove");
    }
    private void OnDisable()
    {
        if (raceManager != null)
        {
            raceManager.OnRaceFinish.RemoveListener(ShowEndGameCanvas);
            raceManager.OnRaceFinish.RemoveListener(UpdateEndGameInfo);
        }
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
