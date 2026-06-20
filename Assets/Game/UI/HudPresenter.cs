using SteelRain.Core;
using SteelRain.Player;
using UnityEngine;
using UnityEngine.UI;

namespace SteelRain.UI
{
    public sealed class HudPresenter : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text weaponLevelText;
        [SerializeField] private Text characterText;
        [SerializeField] private Text skillText;
        [SerializeField] private Text squadText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text currencyText;
        [SerializeField] private CharacterSkill skill;

        private PlayerSquad squad;
        private string currentWeaponName = "";
        private int currentAmmo = 0;
        private string currentFormName = "";

        private void OnEnable()
        {
            GameEvents.PlayerHealthChanged += OnHealthChanged;
            GameEvents.AmmoChanged += OnAmmoChanged;
            GameEvents.WeaponFormChanged += OnWeaponFormChanged;
            GameEvents.WeaponLevelChanged += OnWeaponLevelChanged;
            GameEvents.PlayerCharacterChanged += OnCharacterChanged;
            GameEvents.CurrencyChanged += OnCurrencyChanged;
            ScoreManager.ScoreChanged += OnScoreChanged;
            ScoreManager.ComboChanged += OnComboChanged;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHealthChanged -= OnHealthChanged;
            GameEvents.AmmoChanged -= OnAmmoChanged;
            GameEvents.WeaponFormChanged -= OnWeaponFormChanged;
            GameEvents.WeaponLevelChanged -= OnWeaponLevelChanged;
            GameEvents.PlayerCharacterChanged -= OnCharacterChanged;
            GameEvents.CurrencyChanged -= OnCurrencyChanged;
            ScoreManager.ScoreChanged -= OnScoreChanged;
            ScoreManager.ComboChanged -= OnComboChanged;
        }

        private void Start()
        {
            squad = FindFirstObjectByType<PlayerSquad>();
            if (scoreText != null)
                scoreText.text = $"SCORE: {ScoreManager.Score}";
            OnCurrencyChanged(CurrencyManager.Balance);
        }

        private void OnCurrencyChanged(int balance)
        {
            if (currencyText != null)
                currencyText.text = $"军票: {balance}";
        }

        private void Update()
        {
            if (skill == null || skillText == null) return;
            if (skill.IsReady)
            {
                skillText.text = "Skill: READY";
                skillText.color = new Color(0.2f, 1f, 0.3f, 1f);
            }
            else
            {
                var pct = Mathf.RoundToInt(skill.CooldownPercent * 100f);
                skillText.text = $"Skill: {pct}%";
                skillText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            }

            UpdateSquadRoster();
        }

        private void UpdateSquadRoster()
        {
            if (squadText == null || squad == null) return;
            squadText.text = "";
            for (int i = 0; i < 4; i++)
            {
                var runtime = squad.GetRuntime(i);
                if (runtime == null) continue;
                var isActive = squad.ActiveIndex == i;
                var marker = isActive ? "> " : "  ";
                squadText.text += $"{marker}{runtime.Definition.displayName}\n";
            }
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
            // int.MaxValue表示无限弹药
            var ammoStr = currentAmmo == int.MaxValue || currentAmmo < 0 ? "INF" : currentAmmo.ToString();
            var formStr = string.IsNullOrEmpty(currentFormName) ? "" : $" [{currentFormName}]";
            ammoText.text = $"{currentWeaponName} {ammoStr}{formStr}";
        }

        private void OnWeaponLevelChanged(int level)
        {
            if (weaponLevelText != null)
                weaponLevelText.text = $"Weapon Lv{level}";
        }

        private void OnCharacterChanged(string displayName)
        {
            if (characterText != null)
                characterText.text = displayName;
        }
    }
}
