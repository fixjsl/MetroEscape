using JetBrains.Annotations;
using System;
using System.IO;
using UnityEngine;

public static class JsonFileStore
{
    public static T LoadorDefault<T>(string path) where T : new()
    {
        if (!File.Exists(path)) return new T();

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) throw new Exception("Empty file");

            var data = new T();

            JsonUtility.FromJsonOverwrite(json, data);
            return data;
        }
        catch(Exception e)
        {
            Debug.LogWarning($"[JsonFilestore] read fail, use default : {path}\n {e.Message}");
            try
            {
                File.Copy(path, path + ".bak", true);
            }
            catch
            {
                
            }
            return new T();
        }

      
    }
    public static bool Save<T>(string path, T data)
    {
        string tmp = path + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Replace(tmp, path, null);
            else File.Move(tmp, path);
            return true;
        }
        catch(Exception e)
        {
            Debug.LogError($"[JsonFileStore] 저장 실패 : {path}\n{e.Message}");
            try
            {
                if (File.Exists(tmp)) File.Delete(tmp);
            }
            catch{}
            return false;
        }
    }


}
