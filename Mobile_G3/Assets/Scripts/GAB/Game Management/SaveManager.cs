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

    public void SaveDataOnMainDirectory<T>(T data)
    {
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

            var data = (T)dataSerializer.Deserialize(fileStream);
            fileStream.Close();
            
            Debug.Log("Data loaded!");
            return data;
        }

        Debug.LogWarning("Path doesn't exist!");
        return default;
    }
}