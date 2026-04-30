using System;
using System.IO;
using UnityEngine;

namespace SteelRain.Save
{
    public static class SaveService
    {
        private const string FileName = "steel-rain-save.json";

        private static SaveData pendingLoad;

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        public static bool HasSave => File.Exists(SavePath);

        public static void Save(SaveData data)
        {
            if (data == null)
                return;

            data.savedAtUtc = DateTime.UtcNow.ToString("O");
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }

        public static SaveData Load()
        {
            if (!HasSave)
                return null;

            return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        }

        public static void Delete()
        {
            if (HasSave)
                File.Delete(SavePath);
        }

        public static void QueuePendingLoad(SaveData data)
        {
            pendingLoad = data;
        }

        public static bool TryConsumePendingLoad(out SaveData data)
        {
            data = pendingLoad;
            pendingLoad = null;
            return data != null;
        }
    }
}
