using System;
using System.IO;
using UnityEngine;

namespace SteelRain.Save
{
    public static class SaveService
    {
        public const int MaxSlots = 3;
        private const string FileNameTemplate = "steel-rain-save-slot-{0}.json";

        private static SaveData pendingLoad;
        private static int currentSlot = 1;

        public static int CurrentSlot => currentSlot;
        public static string SavePath => GetSavePathForSlot(currentSlot);
        public static bool HasSave => File.Exists(SavePath);

        public static void SetCurrentSlot(int slot)
        {
            currentSlot = NormalizeSlot(slot);
        }

        public static int NormalizeSlot(int slot)
        {
            return Mathf.Clamp(slot, 1, MaxSlots);
        }

        public static string GetFileNameForSlot(int slot)
        {
            return string.Format(FileNameTemplate, NormalizeSlot(slot));
        }

        public static string GetSavePathForSlot(int slot)
        {
            return Path.Combine(Application.persistentDataPath, GetFileNameForSlot(slot));
        }

        public static bool HasSaveInSlot(int slot)
        {
            return File.Exists(GetSavePathForSlot(slot));
        }

        public static void Save(SaveData data)
        {
            SaveToSlot(currentSlot, data);
        }

        public static void SaveToSlot(int slot, SaveData data)
        {
            if (data == null)
                return;

            data.savedAtUtc = DateTime.UtcNow.ToString("O");
            var path = GetSavePathForSlot(slot);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
        }

        public static SaveData Load()
        {
            return LoadFromSlot(currentSlot);
        }

        public static SaveData LoadFromSlot(int slot)
        {
            var path = GetSavePathForSlot(slot);
            if (!File.Exists(path))
                return null;

            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }

        public static void Delete()
        {
            DeleteSlot(currentSlot);
        }

        public static void DeleteSlot(int slot)
        {
            var path = GetSavePathForSlot(slot);
            if (File.Exists(path))
                File.Delete(path);
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
