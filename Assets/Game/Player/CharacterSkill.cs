using System.Collections;
using SteelRain.Audio;
using SteelRain.Core;
using SteelRain.VFX;
using SteelRain.Weapons;
using UnityEngine;

namespace SteelRain.Player
{
    /// <summary>
    /// 角色专属 Lv3 技能系统。
    /// 按技能键（Q 或鼠标右键）释放，需要武器达到 Lv3。
    /// 每个角色有不同效果：
    /// - Aila 突破火线：高速滑步 + 无敌 + 自动锁定扫射
    /// - Bruno 战壕巨盾：5 秒正面防弹盾
    /// - Mara 轰炸矩阵：标记区域连落 6 发炸弹
    /// - Niko 时间裂隙：4 秒减速场
    /// </summary>
    public sealed class CharacterSkill : MonoBehaviour
    {
        [SerializeField] private PlayerController2D controller;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private CharacterRuntime runtime;
        [SerializeField] private Projectile skillProjectilePrefab;

        private float cooldownRemaining;
        private bool active;

        public bool IsReady => cooldownRemaining <= 0f;
        public float CooldownPercent => runtime != null && runtime.Definition != null
            ? 1f - (cooldownRemaining / runtime.Definition.skillCooldown)
            : 0f;

        private void Awake()
        {
            ResetStatics();
        }

        private static void ResetStatics()
        {
            BreakthroughBuff = false;
            ShieldActive = false;
            ShieldTimer = 0f;
            TimeRiftActive = false;
            TimeRiftTimer = 0f;
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
                cooldownRemaining -= Time.deltaTime;

            // 暂停时不响应技能输入
            if (Time.timeScale == 0f) return;

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1))
                TryActivate();
        }

        public void AssignRuntime(CharacterRuntime r)
        {
            if (runtime != null)
                runtime.SkillCooldownRemaining = cooldownRemaining;
            runtime = r;
            if (runtime != null)
                cooldownRemaining = runtime.SkillCooldownRemaining;
        }

        private void TryActivate()
        {
            if (runtime == null || combat == null) return;
            if (cooldownRemaining > 0f) return;
            if (combat.CurrentWeaponLevel < 3) return;

            var skillId = runtime.Definition.skillId;
            StartCoroutine(ExecuteSkill(skillId));

            cooldownRemaining = runtime.Definition.skillCooldown;
        }

        private IEnumerator ExecuteSkill(CharacterSkillId skillId)
        {
            active = true;
            AudioManager.Play("sfx_upgrade", 1f);

            switch (skillId)
            {
                case CharacterSkillId.BreakthroughFire:
                    yield return BreakthroughFire();
                    break;
                case CharacterSkillId.TrenchShield:
                    yield return TrenchShield();
                    break;
                case CharacterSkillId.BombardmentMatrix:
                    yield return BombardmentMatrix();
                    break;
                case CharacterSkillId.TimeRift:
                    yield return TimeRift();
                    break;
            }

            active = false;
        }

        /// <summary>
        /// Aila 突破火线：向瞄准方向高速滑步 0.45 秒无敌，自动扫射前方 3 敌人，结束后 4 秒突破姿态。
        /// </summary>
        private IEnumerator BreakthroughFire()
        {
            var body = GetComponent<Rigidbody2D>();
            var dir = controller != null && controller.AimDirection.x < 0 ? -1f : 1f;
            var duration = 0.45f;
            var elapsed = 0f;
            var fireTimer = 0f;

            if (controller != null) controller.SetSkillLock(true);

            while (elapsed < duration)
            {
                body.linearVelocity = new Vector2(dir * 18f, 0f);

                // 自动扫射：每 0.05 秒一发
                fireTimer += Time.deltaTime;
                if (skillProjectilePrefab != null && fireTimer >= 0.05f)
                {
                    fireTimer = 0f;
                    var proj = Instantiate(skillProjectilePrefab, transform.position + Vector3.right * dir, Quaternion.identity);
                    proj.LaunchWithDamage(Vector2.right * dir, 20f, 3, 1, Team.Player);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (controller != null) controller.SetSkillLock(false);

            BreakthroughBuff = true;
            yield return new WaitForSeconds(4f);
            BreakthroughBuff = false;
        }

        public static bool BreakthroughBuff { get; private set; }

        /// <summary>
        /// Bruno 战壕巨盾：5 秒正面防弹，结束时盾击冲撞。
        /// </summary>
        private IEnumerator TrenchShield()
        {
            ShieldActive = true;
            ShieldTimer = 5f;

            while (ShieldTimer > 0f)
            {
                ShieldTimer -= Time.deltaTime;
                yield return null;
            }

            ShieldActive = false;

            // 盾击冲撞：前方 3 单位范围伤害
            var dir = controller != null && controller.AimDirection.x < 0 ? -1f : 1f;
            var hits = Physics2D.OverlapCircleAll(transform.position + Vector3.right * dir * 1.5f, 2.5f);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Health hp) && hp.Team == Team.Enemy)
                {
                    hp.ApplyDamage(new DamageInfo(5, Team.Player, Vector2.right * dir));
                }
            }
            AudioManager.Play("sfx_explosion", 0.5f);
        }

        public static bool ShieldActive { get; set; }
        public static float ShieldTimer { get; set; }

        /// <summary>
        /// Mara 轰炸矩阵：标记前方区域，1 秒后连落 6 发炸弹。
        /// </summary>
        private IEnumerator BombardmentMatrix()
        {
            var aimDir = controller != null ? controller.AimDirection : Vector2.right;
            var center = transform.position + (Vector3)aimDir * 6f;
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < 6; i++)
            {
                var offset = new Vector3(Random.Range(-3f, 3f), 8f, 0f);
                var pos = center + offset;

                if (skillProjectilePrefab != null)
                {
                    var proj = Instantiate(skillProjectilePrefab, pos, Quaternion.identity);
                    proj.LaunchWithDamage(Vector2.down, 10f, 4, 0, Team.Player);
                }

                AudioManager.Play("sfx_explosion", 0.4f);
                yield return new WaitForSeconds(0.15f);
            }
        }

        /// <summary>
        /// Niko 时间裂隙：4 秒减速场，敌人和敌方子弹速度降低 60%。
        /// </summary>
        private IEnumerator TimeRift()
        {
            TimeRiftActive = true;
            TimeRiftTimer = 4f;

            while (TimeRiftTimer > 0f)
            {
                TimeRiftTimer -= Time.deltaTime;
                yield return null;
            }

            TimeRiftActive = false;
        }

        public static bool TimeRiftActive { get; private set; }
        public static float TimeRiftTimer { get; private set; }
    }
}
