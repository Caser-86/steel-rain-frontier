using System;
using UnityEngine;

namespace SteelRain.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public string levelId = "Level01";
        public Vector3 checkpoint;
        public int selectedCharacterIndex;
        public int[] characterHealth = Array.Empty<int>();
        public string weaponId = "assault_rifle";
        public int weaponLevel;
        public bool level01Cleared;
        public string savedAtUtc;
    }
}
