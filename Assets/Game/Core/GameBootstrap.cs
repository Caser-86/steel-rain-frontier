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
            SceneManager.LoadScene("MainMenu");
        }

        private void Update()
        {
            ScoreManager.Update();
        }
    }
}
