using UnityEngine;
using UnityEngine.SceneManagement;

namespace SteelRain.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializePhysics()
        {
            // 强制设置Layer碰撞矩阵：所有层都允许碰撞
            // Layer 6 = Ground, Layer 7 = Player, Layer 8 = Enemy, Layer 9 = Projectile
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    Physics2D.IgnoreLayerCollision(i, j, false);
                }
            }
        }

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
