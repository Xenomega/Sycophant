using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

sealed internal class GameSaveManager : MonoBehaviour
{
    internal const string SAVE_NAME = "save.dat";
    internal const string HASH = "HAD#28jK329*AITTOVVAF23KF";

    #region Values
    internal static GameSaveManager singleton;
    internal static bool IsDirty
    {
        get
        {
            return data.Values.Count > 0;
        }
    }

    [Serializable]
    internal class SaveData
    {
        internal string Checksum = string.Empty;
        [Serializable]
        internal class SaveValue
        {
            internal string Key;
            internal object Value;
            internal SaveValue(string key, object value)
            {
                Key = key;
                Value = value;
            }
        }
        internal List<SaveValue> Values = new List<SaveValue>();
    }
    internal static SaveData data = null;

    private static bool _locked;
    private static BinaryFormatter _binaryFormatter;
    #endregion

    #region Functions

    internal static void Initialize()
    {
        _binaryFormatter = new BinaryFormatter();
        _locked = false;
        Load();
    }

    internal static void Lock(bool state)
    {
        _locked = state;
    }

    internal static void DeleteAll()
    {
        data.Values.Clear();
        Save();
    }
    internal static void Delete(string key)
    {
        data.Values.RemoveAll(d => d.Key == key);
        Save();
    }

    internal static void SetString(string key, string value)
    {
        SetObject(key, value);
    }
    internal static void SetInt(string key, int value)
    {
        SetObject(key, value);
    }
    internal static void SetFloat(string key, float value)
    {
        SetObject(key, value);
    }
    internal static void SetObject(string key, object value)
    {
        if (_locked)
            return;
        bool exists = Exists(key);
        if (!exists)
            data.Values.Add(new SaveData.SaveValue(key, value));
        else
        {
            SaveData.SaveValue existingValue = data.Values.Find(d => d.Key == key);
            existingValue.Value = value;
        }

        Save();
    }

    internal static string GetString(string key)
    {
        object value = GetObject(key);

        if (value == null)
            return string.Empty;
        return (string)value;
    }
    internal static int GetInt(string key)
    {
        object value = GetObject(key);

        if (value == null)
            return 0;
        return (int)value;
    }
    internal static float GetFloat(string key)
    {
        object value = GetObject(key);

        if (value == null)
            return 0;
        return (float)value;
    }
    internal static object GetObject(string key)
    {
        if (data == null || data.Values.Count == 0)
            return null;

        return data.Values.Find(d => d.Key == key).Value;
    }
    internal static bool Exists(string key)
    {
        return data.Values.Find(d => d.Key == key) != null;
    }

    // If Load / Save changes in a future build, attempt to load using the then deprecated method to prevent false save data loss
    internal static void Save()
    {
        if (_locked)
            return;

        string saveLocation = Application.persistentDataPath + "/" + SAVE_NAME;

        // In the case of a save before a load, load
        if (GameSaveManager.data == null)
            Load();

        MemoryStream stream = new MemoryStream();

        // Clear checksum if there is on, serialize data to a byte array, set checksum
        GameSaveManager.data.Checksum = string.Empty;
        _binaryFormatter.Serialize(stream, GameSaveManager.data);
        byte[] data = stream.ToArray();
        GameSaveManager.data.Checksum = GetHashString(data);
        stream.Close();

        // Serialize data with checksum set
        stream = new MemoryStream();
        _binaryFormatter.Serialize(stream, GameSaveManager.data);
        data = stream.ToArray();

        // Write file
        FileStream fileStream = File.Open(saveLocation, FileMode.OpenOrCreate);
        fileStream.Write(data, 0, data.Length);

        // Dispose
        fileStream.Close();
        stream.Close();
    }
    private static void Load()
    {
        string saveLocation = Application.persistentDataPath + "/" + SAVE_NAME;

        if (!File.Exists(saveLocation))
        {
            // If there isn't a file, create one, set data anew
            RenewSave();
        }
        try
        {
            MemoryStream stream = new MemoryStream();
            // Read file, de-serialize
            FileStream fileStream = File.Open(saveLocation, FileMode.Open);
            SaveData dataCheck = (SaveData)_binaryFormatter.Deserialize(fileStream);

            // Define the checksum of the data on save
            string checksum = dataCheck.Checksum;
            // Clear checksum to its default state before verifying hash
            dataCheck.Checksum = string.Empty;
            // Serialize data with checksum removed
            _binaryFormatter.Serialize(stream, dataCheck);
            byte[] data = stream.ToArray();
            // Determine if checksum is a match, if so continue with save data, otherwise set data anew
            string dataHash = GetHashString(data);
            bool valid = checksum == dataHash;
            GameSaveManager.data = valid ? dataCheck : new SaveData();

            // Dispose
            fileStream.Close();
            stream.Close();
        }
        catch (Exception e)
        {
            // TODO: Handle or show message related to exception so the user knows the data is corrupt.
            data = new SaveData();
        }
    }

    private static void RenewSave()
    {
        string saveLocation = Application.persistentDataPath + "/" + SAVE_NAME;
        File.Create(saveLocation);
        data = new SaveData();
    }

    private static string GetHashString(byte[] data)
    {
        // Define salt
        byte[] salt = Encoding.ASCII.GetBytes(SystemInfo.graphicsDeviceID.ToString() + HASH + SystemInfo.deviceUniqueIdentifier + SAVE_NAME);
        // Add salt to data, define hash
        byte[] saltedValue = data.Concat(salt).ToArray();
        byte[] hash = new SHA256Managed().ComputeHash(saltedValue);

        // Create hash string
        string hashString = string.Empty;
        for (int i = 0; i < hash.Length; i++)
            hashString += hash[i].ToString();
        return hashString;
    }
    #endregion
}
