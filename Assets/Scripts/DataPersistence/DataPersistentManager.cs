using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistentManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] public bool shouldLoadGame = true;
   
    [Header("File Storage Config")]
    [SerializeField] private string fileName = "Save.json";
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;

    //can only modify the instance in this class
    public static DataPersistentManager instance { get; private set;}
    private void Awake()
    {   
         

        //should only be one persistent manager 
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
            Destroy(this.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        if (!shouldLoadGame) 
        {
            Debug.LogWarning("Data Persistence is currently disabled!");
        }

        
    }
    
     private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects(); 
        if(shouldLoadGame)LoadGame();
        
        
        
    }
    
    
    
    public void NewGame()
    {
        this.gameData = new GameData();
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
        Debug.Log(gameData==null);
        
    }
    public void LoadGame()
    {   
        this.gameData = dataHandler.Load();
         // start a new game if the data is null and we're configured to initialize data for debugging purposes

         if (this.gameData == null) 
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            NewGame();
            Debug.Log(gameData.Score);
        }
       
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
        
        
    }
    public void SaveGame()
    {   
        
        // if we don't have any data to save, log a warning here
        if (this.gameData == null) 
        {
            Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
            
            return;
        }
        
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
        Debug.Log("points = " + gameData.Score);
    }
    /**
    AutoSave when you exit the game. 
    */
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    public bool HasGameData() 
    {
        return gameData != null;
    }
     private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
   
    public void SetShouldLoadGame()
    {
        shouldLoadGame = false;
    }
}
