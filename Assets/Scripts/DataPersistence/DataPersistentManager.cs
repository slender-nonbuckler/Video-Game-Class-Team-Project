using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistentManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool initializeDataIfNull = true;
    [Header("File Storage Config")]
    [SerializeField] private string fileName = "Save.json";
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public static DataPersistentManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        if (transform.parent != null)
        {
            Debug.LogWarning("Don't put DataPersistentManager as a child of another object. It'll destroy the parent and make it an orphan.");
            transform.SetParent(null);
        }
        DontDestroyOnLoad(this.gameObject);
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void LoadGame()
    {
        //Fallback if player clicks "Continue" but there's no saved file, probably should disable the button
        this.gameData = dataHandler.Load();
        if (this.gameData == null)
        {
            NewGame();
        }
        else
        {
            Debug.Log($"Loaded game data: {gameData.DebugString()}");
        }
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void NewGame()
    {
        this.gameData = new GameData();
        SaveGame();
    }

    public void SaveGame()
    {
        if (this.gameData == null)
        {
            NewGame();
        }
        else
        {
            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.SaveData(ref gameData);
            }
            dataHandler.Save(gameData);
            Debug.Log($"Saved game data: {gameData.DebugString()}");
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return dataHandler.FileExists();
    }

    public void StartNewGame()
    {
        NewGame();
        //Assuming it'll always go back to this scene for now
        SceneManager.LoadScene("CarSelection");
    }
}