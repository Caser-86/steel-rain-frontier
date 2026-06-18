using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
            // 启动画面由 BootScreen 负责延时跳转
            if (FindObjectOfType<BootScreen>() == null)
            {
                gameObject.AddComponent<BootScreen>();
            }
        }

        private void Update()
        {
            ScoreManager.Update();
        }
    }
}
