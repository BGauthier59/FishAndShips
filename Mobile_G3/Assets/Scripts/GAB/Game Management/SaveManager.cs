using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("Data")]
public class SaveManager : MonoSingleton<SaveManager>
{
    [SerializeField] private string path;
    private string filePath;

    private Encoding encoding = Encoding.UTF8;

    [SerializeField] private SaveData currentData;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void SetCurrentData()
    {
        currentData = LoadDataFromDirectory();
        LevelManager.instance.RefreshLevels(currentData);
    }

    public SaveData GetData()
    {
        return currentData;
    }

    public void UpdateCurrentLevelData(SaveData.LevelData levelData, int levelIndex)
    {
        currentData.levelsData[levelIndex] = levelData;
        LevelManager.instance.RefreshLevels(currentData);
    }
    
    public void UpdateCurrentSchemeData(int data)
    {
        currentData.controls = data;
    }

    public void SaveCurrentData()
    {
        SaveDataOnMainDirectory(currentData);
    }

    private void SaveDataOnMainDirectory(SaveData data)
    {
        path = Application.persistentDataPath;
        SaveGameData(data);
    }

    private void SaveGameData(SaveData data)
    {
        StreamWriter streamWriter = new StreamWriter(Path.Combine(path, $"{nameof(SaveData)}.xml"), false, encoding);
        XmlSerializer dataSerializer = new XmlSerializer(typeof(SaveData));

        dataSerializer.Serialize(streamWriter, data);
        streamWriter.Close();

        Debug.Log("Saved worked!");
    }

    public SaveData LoadDataFromDirectory()
    {
        path = Application.persistentDataPath;
        return LoadData();
    }

    private SaveData LoadData()
    {
        filePath = Path.Combine(path, $"{nameof(SaveData)}.xml");

        if (File.Exists(filePath))
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            XmlSerializer dataSerializer = new XmlSerializer(typeof(SaveData));

            var data = (SaveData) dataSerializer.Deserialize(fileStream);
            fileStream.Close();

            Debug.Log("Data loaded!");
            return data;
        }

        Debug.Log("No data were found. Generate the default data.");

        SaveData defaultData = new SaveData
        {
            levelsData = new SaveData.LevelData[LevelManager.instance.allLevels.Length],
        };

        for (int i = 0; i < defaultData.levelsData.Length; i++)
        {
            defaultData.levelsData[i] = new SaveData.LevelData();
        }
        
        return defaultData;
    }
}

[Serializable]
public class SaveData
{
    public LevelData[] levelsData; // Stands for levels
    public int controls;
    
    [Serializable]
    public class LevelData
    {
        public LevelData()
        {
            starCount = 0;
        }
        
        public int starCount = 0;
    }
}