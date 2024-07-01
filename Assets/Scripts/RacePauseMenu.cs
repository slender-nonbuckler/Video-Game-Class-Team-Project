using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RacePauseMenu : MonoBehaviour
{
    public Canvas PauseCanvas;
    private bool isPaused = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PauseCanvas != null)
        {
            PauseCanvas.gameObject.SetActive(false);
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

}
