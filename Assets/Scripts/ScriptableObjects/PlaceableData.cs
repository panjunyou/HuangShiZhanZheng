using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityRoyale
{
	// 可放置兵种数据
    [CreateAssetMenu(fileName = "NewPlaceable", menuName = "Unity Royale/Placeable Data")]
    public class PlaceableData : ScriptableObject
    {
        [Header("Common")]
        public Placeable.PlaceableType pType; // 可放置兵种类型
        public GameObject associatedPrefab; // 兵种关联的预制体（红方，己方）
        public GameObject alternatePrefab; // 兵种关联的预制体（蓝方，敌方）

		[Header("Units and Buildings")]
        public ThinkingPlaceable.AttackType attackType = ThinkingPlaceable.AttackType.Melee; // 攻击类型(近程/远程)
        public Placeable.PlaceableTarget targetType = Placeable.PlaceableTarget.Both; // 攻击目标类型
        public float attackRatio = 1f; // 每次攻击之间的时间间隔
        public float damagePerAttack = 2f; // 每次攻击造成的伤害点数
        public float attackRange = 1f; // 攻击范围
        public float hitPoints = 10f; // ?生命值
		public AudioClip attackClip, dieClip; // 攻击/死亡时播放的音效

        [Header("Units")]
        public float speed = 5f; // 移动速度
        
        [Header("Obstacles and Spells")]
        public float lifeTime = 5f; // 障碍物和法术的存在时间.此属性对障碍物和法术类型尤其重要(它们过一段时间以后就消失了)
        
        [Header("Spells")]
        public float damagePerSecond = 1f; // 每秒攻击伤害
    }
}