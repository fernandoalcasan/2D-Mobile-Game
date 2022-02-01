/*
 * This script contains the Save Manager methods, which make sure to save the player data container locally.
 */

using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    //Method to save the player's data container with encryption
    public static void SavePlayerData(Data playerData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/data.pak";
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            binaryFormatter.Serialize(stream, playerData);
        }
    }

    //Method to deserialize and return the player's encrypted local data
    public static Data LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/data.pak";

        if(File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                Data playerData = binaryFormatter.Deserialize(stream) as Data;
                return playerData;
            }
        }
        else
        {
            Debug.LogError("The file " + path + " wasn't found");
            return null;
        }
    }
}
