using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveManager : MonoBehaviour
{
    public static void Save<T>(T objectToSave, string key, string fileName)
    {
        string path = Application.persistentDataPath + "/saves/" + fileName;
        Directory.CreateDirectory(path);  // Creates the folder if it isn't there
        BinaryFormatter formatter = new BinaryFormatter();  // Creates a binary formatter
        using (FileStream fileStream = new FileStream(path + key + ".txt", FileMode.Create))
        {
            formatter.Serialize(fileStream, objectToSave);  // Saves
        }
    }


    public static T Load<T>(string key, string fileName)
    {
        string path = Application.persistentDataPath + "/saves/" + fileName;
        BinaryFormatter formatter = new BinaryFormatter();  // Creates a binary formatter
        T returnValue = default(T);  // Gives the default for a value if it can't be found
        using (FileStream fileStream = new FileStream(path + key + ".txt", FileMode.Open))
        {
            returnValue = (T)formatter.Deserialize(fileStream);  // Gets the data
        }

        return returnValue;
    }


    public static bool SaveExists(string key, string fileName)
    {
        string path = Application.persistentDataPath + "/saves/" + fileName + key + ".txt";
        return File.Exists(path);
    }

    public static void DeleteSaveFile(string fileName)
    {
        string path = Application.persistentDataPath + "/saves/" + fileName;
        DirectoryInfo directory = new DirectoryInfo(path);
        directory.Delete(true);
    }
}
