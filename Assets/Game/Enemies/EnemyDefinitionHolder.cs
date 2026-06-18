using UnityEngine;

namespace SteelRain.Enemies
{
    /// <summary>
    /// 挂在关卡触发器上，持有敌人定义，运行时供 LevelSegmentTrigger 初始化敌人。
    /// </summary>
    public sealed class EnemyDefinitionHolder : MonoBehaviour
    {
        public EnemyDefinition definition;
    }
}
