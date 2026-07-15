using System;
using System.IO;
using UnityEngine;

namespace TXGame
{
    public static class SaveManager
    {
        public static readonly string[] SlotIds = { "01", "02", "03" };

        public static void Save(string slotId, SaveData data)
        {
            if (!IsValidSlot(slotId) || data == null) return;

            data.slotId = slotId;
            data.savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrEmpty(data.displayTitle))
                data.displayTitle = $"存档 {slotId}";

            Directory.CreateDirectory(SaveDirectory);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetSlotPath(slotId), json);
        }

        public static bool TryLoad(string slotId, out SaveData data)
        {
            data = null;
            if (!IsValidSlot(slotId)) return false;

            string path = GetSlotPath(slotId);
            if (!File.Exists(path)) return false;

            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SaveData>(json);
            return data != null;
        }

        public static bool Exists(string slotId)
        {
            return IsValidSlot(slotId) && File.Exists(GetSlotPath(slotId));
        }

        public static string GetSlotSummary(string slotId)
        {
            if (!TryLoad(slotId, out SaveData data))
                return $"槽位 {slotId}\n空存档\n可新建";

            string phase = string.IsNullOrEmpty(data.phaseName) ? "未知阶段" : data.phaseName;
            string time = string.IsNullOrEmpty(data.savedAt) ? "无时间" : data.savedAt;
            return $"槽位 {slotId}\n{phase}\n心数 {data.hearts}/5\n{time}";
        }

        public static string GetSlotShortSummary(string slotId)
        {
            if (!TryLoad(slotId, out SaveData data))
                return string.Empty;

            string phase = string.IsNullOrEmpty(data.phaseName) ? "未知阶段" : data.phaseName;
            return $"{phase}\n心力 {data.hearts}/5";
        }

        public static void Delete(string slotId)
        {
            if (!IsValidSlot(slotId)) return;
            string path = GetSlotPath(slotId);
            if (File.Exists(path))
                File.Delete(path);
        }

        private static bool IsValidSlot(string slotId)
        {
            foreach (string id in SlotIds)
            {
                if (id == slotId) return true;
            }
            return false;
        }

        private static string GetSlotPath(string slotId)
        {
            return Path.Combine(SaveDirectory, $"save_{slotId}.json");
        }

        private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
    }
}
