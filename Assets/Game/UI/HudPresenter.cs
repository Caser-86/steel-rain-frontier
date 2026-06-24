using SteelRain.Core;
using SteelRain.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    /// <summary>
    /// HUD 显示（合金弹头简化版）。
    /// 仅显示：血量、弹药、角色名、小队存活、分数、连击。
    /// 无技能、无武器等级、无军票。
    /// </summary>
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text characterText;
        [SerializeField] private Text squadText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text waveText;

        private PlayerSquad squad;
        private string currentWeaponName = "";
        private int currentAmmo = 0;
        private string currentFormName = "";

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += OnHealthChanged;
            GameEvents.AmmoChanged += OnAmmoChanged;
            GameEvents.WeaponFormChanged += OnWeaponFormChanged;
            GameEvents.PlayerCharacterChanged += OnCharacterChanged;
            GameEvents.EndlessWaveChanged += OnWaveChanged;
            ScoreManager.ScoreChanged += OnScoreChanged;
            ScoreManager.ComboChanged += OnComboChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= OnHealthChanged;
            GameEvents.AmmoChanged -= OnAmmoChanged;
            GameEvents.WeaponFormChanged -= OnWeaponFormChanged;
            GameEvents.PlayerCharacterChanged -= OnCharacterChanged;
            GameEvents.EndlessWaveChanged -= OnWaveChanged;
            ScoreManager.ScoreChanged -= OnScoreChanged;
            ScoreManager.ComboChanged -= OnComboChanged;
        }

        private void Start()
        {
            squad = FindFirstObjectByType<PlayerSquad>();
            if (scoreText != null)
                scoreText.text = $"SCORE: {ScoreManager.Score}";
            // 初始绘制一次小队名单（后续由事件驱动刷新）
            UpdateSquadRoster();
        }

        private void UpdateSquadRoster()
        {
            if (squadText == null || squad == null) return;
            // 使用 StringBuilder 避免每帧多次字符串分配
            var sb = new System.Text.StringBuilder(64);
            for (int i = 0; i < 4; i++)
            {
                var runtime = squad.GetRuntime(i);
                if (runtime == null) continue;
                var isActive = squad.ActiveIndex == i;
                var marker = isActive ? "> " : "  ";
                sb.Append(marker).Append(runtime.Definition.displayName).Append('\n');
            }
            squadText.text = sb.ToString();
        }

        private void OnScoreChanged(int newScore)
        {
            if (scoreText != null)
                scoreText.text = $"SCORE: {newScore}";
        }

        private void OnComboChanged(int combo)
        {
            if (comboText == null) return;
            if (combo >= 2)
            {
                comboText.text = $"x{combo} COMBO!";
                comboText.enabled = true;
            }
            else
            {
                comboText.enabled = false;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            if (healthText != null)
                healthText.text = $"HP {current}/{max}";
        }

        private void OnAmmoChanged(string weaponName, int ammo)
        {
            currentWeaponName = weaponName;
            currentAmmo = ammo;
            UpdateAmmoDisplay();
        }

        private void OnWeaponFormChanged(string formName)
        {
            currentFormName = formName ?? "";
            UpdateAmmoDisplay();
        }

        private void UpdateAmmoDisplay()
        {
            if (ammoText == null) return;
            var ammoStr = currentAmmo == int.MaxValue || currentAmmo < 0 ? "INF" : currentAmmo.ToString();
            var formStr = string.IsNullOrEmpty(currentFormName) ? "" : $" [{currentFormName}]";
            ammoText.text = $"{currentWeaponName} {ammoStr}{formStr}";
        }

        private void OnCharacterChanged(string displayName)
        {
            if (characterText != null)
                characterText.text = displayName;
            // 角色切换/死亡时刷新小队名单（事件驱动，替代每帧轮询）
            UpdateSquadRoster();
        }

        private void OnWaveChanged(int wave)
        {
            if (waveText == null) return;
            waveText.gameObject.SetActive(true);
            waveText.enabled = true;
            var isBoss = wave % 5 == 0;
            waveText.text = isBoss ? $"WAVE {wave} - BOSS!" : $"WAVE {wave}";
        }
    }
}
