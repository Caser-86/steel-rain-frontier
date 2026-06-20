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

        public CharacterRuntime Runtime => runtime;

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

            // 统一管理 ShieldTimer：当 shield 由拾取物激活（非技能协程）时递减并关闭
            if (ShieldActive && !active && ShieldTimer > 0f)
            {
                ShieldTimer -= Time.deltaTime;
                if (ShieldTimer <= 0f)
                {
                    ShieldTimer = 0f;
                    ShieldActive = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1))
                TryActivate();
        }

        public void AssignRuntime(CharacterRuntime r)
        {
            if (runtime != null)
                runtime.SkillCooldownRemaining = cooldownRemaining;
            runtime = r;
            if (runtime != null) cooldownRemaining = runtime.SkillCooldownRemaining;

            // 切换角色时重置活跃的技能状态，防止跨角色保留效果
            if (active)
            {
                StopAllCoroutines();
                active = false;
                BreakthroughBuff = false;
                ShieldActive = false;
                ShieldTimer = 0f;
                TimeRiftActive = false;
                TimeRiftTimer = 0f;
                if (controller != null) controller.SetSkillLock(false);
            }
        }

        private void TryActivate()
        {
            if (runtime == null || combat == null) return;
            if (cooldownRemaining > 0f) return;

            var skillId = runtime.Definition.skillId;
            StartCoroutine(ExecuteSkill(skillId));

            cooldownRemaining = runtime.Definition.skillCooldown;
        }

        private IEnumerator ExecuteSkill(CharacterSkillId skillId)
        {
            active = true;
            AudioManager.Play("sfx_upgrade", 1f);

            // 技能释放爆发特效（每个角色不同颜色）
            var buffColor = skillId switch
            {
                CharacterSkillId.BreakthroughFire => new Color(1f, 0.9f, 0.2f, 1f),   // 金黄
                CharacterSkillId.TrenchShield => new Color(0.4f, 0.7f, 1f, 1f),        // 蓝
                CharacterSkillId.BombardmentMatrix => new Color(1f, 0.3f, 0.3f, 1f),   // 红
                CharacterSkillId.TimeRift => new Color(0.6f, 0.3f, 1f, 1f),            // 紫
                _ => new Color(1f, 1f, 1f, 1f)
            };
            SkillVFX.SpawnBurst(transform.position, buffColor, 2f);

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
            var health = GetComponent<Health>();
            var dir = controller != null && controller.AimDirection.x < 0 ? -1f : 1f;
            var duration = 0.45f;
            var elapsed = 0f;
            var fireTimer = 0f;
            var afterimageTimer = 0f;

            if (controller != null) controller.SetSkillLock(true);
            // 突破火线期间无敌
            if (health != null) health.SetInvincible(true);

            while (elapsed < duration)
            {
                body.linearVelocity = new Vector2(dir * 18f, 0f);

                // 残影特效：每 0.03 秒留一个金色残影
                afterimageTimer += Time.deltaTime;
                if (afterimageTimer >= 0.03f)
                {
                    afterimageTimer = 0f;
                    SkillVFX.SpawnAfterimage(transform.position, new Color(1f, 0.9f, 0.2f, 0.6f));
                }

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
            // 结束无敌
            if (health != null) health.SetInvincible(false);

            // 突破姿态光环（持续 4 秒）
            BreakthroughBuff = true;
            StartCoroutine(SkillVFX.AuraLoop(transform, new Color(1f, 0.9f, 0.2f, 0.3f), 4f, 1.5f));
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

            // 盾牌光环（跟随玩家，蓝色，持续 5 秒）
            StartCoroutine(SkillVFX.AuraLoop(transform, new Color(0.4f, 0.7f, 1f, 0.35f), 5f, 1.8f));

            while (ShieldTimer > 0f)
            {
                ShieldTimer -= Time.deltaTime;
                yield return null;
            }

            ShieldActive = false;

            // 盾击冲撞：前方 3 单位范围伤害
            var dir = controller != null && controller.AimDirection.x < 0 ? -1f : 1f;
            var hitPos = transform.position + Vector3.right * dir * 1.5f;
            SkillVFX.SpawnBurst(hitPos, new Color(0.4f, 0.7f, 1f, 1f), 3f);
            var hits = Physics2D.OverlapCircleAll(hitPos, 2.5f);
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

            // 标记区域：红色警告圈持续闪烁 1 秒
            SkillVFX.SpawnWarningZone(center, 4f, 1f);
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

                // 每发炸弹落点爆炸特效
                SkillVFX.SpawnBurst(new Vector3(pos.x, center.y, 0f), new Color(1f, 0.3f, 0.3f, 1f), 1.5f);
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

            // 紫色减速光环（跟随玩家，持续 4 秒）
            StartCoroutine(SkillVFX.AuraLoop(transform, new Color(0.6f, 0.3f, 1f, 0.3f), 4f, 2.5f));

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

    /// <summary>
    /// 技能视觉特效工具：爆发圈、残影、跟随光环、警告区域。
    /// 全部用代码生成圆形 Sprite，无需外部资源。
    /// </summary>
    public static class SkillVFX
    {
        private static Sprite sharedCircleSprite;

        private static Sprite GetCircleSprite()
        {
            if (sharedCircleSprite != null) return sharedCircleSprite;

            int size = 64;
            var tex = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(1f - dist / (size / 2f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.filterMode = FilterMode.Bilinear;
            tex.Apply();
            sharedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            sharedCircleSprite.name = "SkillVFX_Circle";
            return sharedCircleSprite;
        }

        /// <summary>
        /// 爆发特效：从中心向外扩散并淡出的圆圈。
        /// </summary>
        public static void SpawnBurst(Vector3 position, Color color, float scale)
        {
            var go = new GameObject("SkillBurst");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = color;
            sr.sortingOrder = 25;
            var runner = go.AddComponent<SkillVFXRunner>();
            runner.StartCoroutine(BurstRoutine(go, sr, scale));
            // 第二层外圈，增强视觉冲击
            var go2 = new GameObject("SkillBurstOuter");
            go2.transform.position = position;
            var sr2 = go2.AddComponent<SpriteRenderer>();
            sr2.sprite = GetCircleSprite();
            sr2.color = new Color(color.r, color.g, color.b, 0.5f);
            sr2.sortingOrder = 24;
            var runner2 = go2.AddComponent<SkillVFXRunner>();
            runner2.StartCoroutine(BurstRoutine(go2, sr2, scale * 1.8f));
        }

        private static IEnumerator BurstRoutine(GameObject go, SpriteRenderer sr, float scale)
        {
            float elapsed = 0f;
            float duration = 0.7f;  // 延长持续时间
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                go.transform.localScale = Vector3.one * (scale * (0.2f + t * 1.5f));
                var c = sr.color;
                c.a = (1f - t) * 0.9f;
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(go);
        }

        /// <summary>
        /// 残影特效：在指定位置留下一个快速淡出的圆圈。
        /// </summary>
        public static void SpawnAfterimage(Vector3 position, Color color)
        {
            var go = new GameObject("Afterimage");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = color;
            sr.sortingOrder = 15;
            var runner = go.AddComponent<SkillVFXRunner>();
            runner.StartCoroutine(AfterimageRoutine(go, sr));
        }

        private static IEnumerator AfterimageRoutine(GameObject go, SpriteRenderer sr)
        {
            float elapsed = 0f;
            float duration = 0.3f;
            var startScale = Vector3.one * 1.2f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                go.transform.localScale = startScale * (1f - t * 0.3f);
                var c = sr.color;
                c.a = (1f - t) * 0.6f;
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(go);
        }

        /// <summary>
        /// 跟随光环：跟随指定 Transform，持续指定时间后自动销毁。
        /// </summary>
        public static IEnumerator AuraLoop(Transform target, Color color, float duration, float radius)
        {
            var go = new GameObject("SkillAura");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = color;
            sr.sortingOrder = 5;
            go.transform.localScale = Vector3.one * radius;

            float elapsed = 0f;
            while (elapsed < duration && target != null)
            {
                go.transform.position = target.position;
                // 脉动效果
                float pulse = 1f + Mathf.Sin(elapsed * 8f) * 0.08f;
                go.transform.localScale = Vector3.one * (radius * pulse);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(go);
        }

        /// <summary>
        /// 警告区域：在指定位置显示闪烁的红色圆圈，持续指定时间。
        /// </summary>
        public static void SpawnWarningZone(Vector3 center, float radius, float duration)
        {
            var go = new GameObject("WarningZone");
            go.transform.position = center;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetCircleSprite();
            sr.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            sr.sortingOrder = 8;
            go.transform.localScale = Vector3.one * radius;
            var runner = go.AddComponent<SkillVFXRunner>();
            runner.StartCoroutine(WarningRoutine(go, sr, duration));
        }

        private static IEnumerator WarningRoutine(GameObject go, SpriteRenderer sr, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // 快速闪烁
                float blink = Mathf.Abs(Mathf.Sin(elapsed * 12f));
                var c = sr.color;
                c.a = 0.2f + blink * 0.4f;
                sr.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }
            Object.Destroy(go);
        }
    }

    /// <summary>
    /// 辅助组件：让静态 SkillVFX 能启动协程。
    /// </summary>
    internal sealed class SkillVFXRunner : MonoBehaviour { }
}
