using SteelRain.Core;
using UnityEngine;

namespace SteelRain.Game
{
    /// <summary>
    /// 全局游戏循环驱动器：每帧驱动需要帧更新的静态系统。
    /// 挂载在场景根对象上，确保连击计时器、游戏时间统计等正常工作。
    /// </summary>
    public sealed class GameLoop : MonoBehaviour
    {
        private static GameLoop instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // 驱动连击计时器（静态类无 MonoBehaviour 生命周期）
            ScoreManager.Update();

            // 游戏时间统计由 AchievementTracker.Update 驱动，此处不重复累加

            // 每10秒保存一次统计数据
            if (Time.frameCount % 600 == 0)
                AchievementManager.SaveAll();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                AchievementManager.SaveAll();
                instance = null;
            }
        }
    }
}
