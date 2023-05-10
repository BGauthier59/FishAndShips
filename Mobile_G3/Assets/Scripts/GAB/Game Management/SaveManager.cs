using System;
using System.Collections.Generic;
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

    public void SaveDataOnMainDirectory<T>(T data)
    {
        if (data is not global::SaveData)
        {
            Debug.LogWarning("You're saving a data that is not a SaveData class");
        }
        
        path = Application.persistentDataPath;
        SaveData(data);
    }

    private void SaveData<T>(T data)
    {
        StreamWriter streamWriter = new StreamWriter(Path.Combine(path, $"{typeof(T).Name}.xml"), false, encoding);
        XmlSerializer dataSerializer = new XmlSerializer(typeof(T));

        dataSerializer.Serialize(streamWriter, data);
        streamWriter.Close();

        Debug.Log("Saved worked!");
    }

    public T LoadDataFromDirectory<T>()
    {
        path = Application.persistentDataPath;
        return LoadData<T>();
    }

    private T LoadData<T>()
    {
        filePath = Path.Combine(path, $"{typeof(T).Name}.xml");

        if (File.Exists(filePath))
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            XmlSerializer dataSerializer = new XmlSerializer(typeof(T));

            var data = (T) dataSerializer.Deserialize(fileStream);
            fileStream.Close();

            Debug.Log("Data loaded!");
            return data;
        }

        Debug.LogWarning("Path doesn't exist!");
        return default;
    }
}

[Serializable]
public class SaveData
{
    public LevelData[] levelsData; // Stands for levels
    public List<int> charactersData = new(); // Stands for characters index
    public List<int> sailsData = new(); // Stands for sails index

    [Serializable]
    public struct LevelData
    {
        public bool isUnlocked;
        public bool isWon;
        public int starCount;
    }
}