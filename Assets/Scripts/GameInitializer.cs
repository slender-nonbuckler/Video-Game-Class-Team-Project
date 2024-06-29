using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    //Just a little debugging class while working on Data Persistence
    [SerializeField] private GameObject menuUIGameObject;

    private void Awake()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (DataPersistentManager.instance == null)
        {
            return;
        }

        if (menuUIGameObject != null)
        {
            menuUIGameObject.SetActive(true);
        }
    }
}