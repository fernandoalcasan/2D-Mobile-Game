using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    public static void SavePlayerData(PlayerData playerData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/data.pak";
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            binaryFormatter.Serialize(stream, playerData);
        }
    }

    public static PlayerData LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/data.pak";

        if(File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                PlayerData playerData = binaryFormatter.Deserialize(stream) as PlayerData;
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
