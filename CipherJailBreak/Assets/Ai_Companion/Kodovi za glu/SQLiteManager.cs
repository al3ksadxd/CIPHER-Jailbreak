using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MemoryPair
{
    public string key;
    public string value;
}

[System.Serializable]
public class MemoryDatabase
{
    public List<MemoryPair> memories = new List<MemoryPair>();
}

public class SQLiteManager : MonoBehaviour
{
    private string filePath;
    private MemoryDatabase db = new MemoryDatabase();

    private void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "companion_memory.json");
        LoadFromFile();
    }

    public void SaveMemory(string key, string value)
    {
        // Proveri da li ključ već postoji, ako postoji ažuriraj ga
        MemoryPair existing = db.memories.Find(m => m.key == key);
        if (existing != null)
        {
            existing.value = value;
        }
        else
        {
            db.memories.Add(new MemoryPair { key = key, value = value });
        }
        SaveToFile();
    }

    public string GetMemory(string key, string defaultValue = "")
    {
        MemoryPair found = db.memories.Find(m => m.key == key);
        return found != null ? found.value : defaultValue;
    }

    public string GetAllMemoriesFormatted()
    {
        string result = "";
        foreach (var m in db.memories)
        {
            result += $"- {m.key}: {m.value}\n";
        }
        return result;
    }

    private void SaveToFile()
    {
        string json = JsonUtility.ToJson(db, true);
        File.WriteAllText(filePath, json);
    }

    private void LoadFromFile()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            db = JsonUtility.FromJson<MemoryDatabase>(json);
            if (db == null) db = new MemoryDatabase();
        }
        else
        {
            db = new MemoryDatabase();
        }
    }
}