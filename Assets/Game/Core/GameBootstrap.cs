using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

            // Boot scene owns the startup flow; gameplay/menu scenes should not get a boot overlay.
            if (SceneManager.GetActiveScene().name == "Boot" && FindFirstObjectByType<BootScreen>() == null)
            {
                gameObject.AddComponent<BootScreen>();
            }

            EnsureEventSystemForSceneButtons();
        }

        private static void EnsureEventSystemForSceneButtons()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            if (FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
                return;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private void Update()
        {
            // ScoreManager.Update 已由 GameLoop 统一驱动，不再在此重复调用
        }

        private void OnApplicationQuit()
        {
            SaveSystem.SetQuitting();
        }
    }
}
