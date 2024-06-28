using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler 
{   
    private string dataDirPath = "";
    private string dataFileName = "";

    public FileDataHandler(String dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }
    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {   
            
            try{
                string dataToLoad = "";
                dataToLoad = File.ReadAllText(fullPath);
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from:" + fullPath + "\n" + e);
            }
        }
        return loadedData;
        
    }
    public void Save(GameData data) 
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, dataToStore);
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to:" + fullPath + "\n" + e);
        }
    }
    public bool FileExists()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        return File.Exists(fullPath);
    }
}
