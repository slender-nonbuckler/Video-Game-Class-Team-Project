using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RacePauseMenu : MonoBehaviour
{
    public Canvas PauseCanvas;
    public Canvas EndGameCanvas;
    private bool isPaused = false;
    public RaceManager raceManager;
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
            raceManager.OnRaceEnd.AddListener(ShowEndGameCanvas);
        }
    }

    private void ShowEndGameCanvas()
    {
        if (EndGameCanvas != null)
        {
            EndGameCanvas.gameObject.SetActive(true);
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
        SceneManager.LoadScene("Menu");
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
    public void RestartRound()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Track1v2");
    }
    public void RaceAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Track1v2");
    }
    private void OnDisable()
    {
        if (raceManager != null)
        {
            raceManager.OnRaceEnd.RemoveListener(ShowEndGameCanvas);
        }
    }
}
