using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistentManager : MonoBehaviour
{
    [Header("File Storage Config")]
    private string fileName = "Save.json";
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
        }
        instance = this;
        
    }
    
    
    private void Start()
    {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects(); 
        
        LoadGame();
    }
    
    
    public void NewGame()
    {
        this.gameData = new GameData();
        
    }
    public void LoadGame()
    {   
        this.gameData = dataHandler.Load();
        
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            
            dataPersistenceObj.LoadData(gameData);
        }
        if (this.gameData == null) {
            Debug.Log("No data was found.");
            NewGame();
        }
        
        
        //Debug.Log("points loaded = " + gameData.Score);
        
    }
    public void SaveGame()
    {
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
     private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
