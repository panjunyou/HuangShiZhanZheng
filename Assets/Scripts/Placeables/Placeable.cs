using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityRoyale
{
    //Base class for all objects that can be placed on the play area: units, obstacles, structures, etc.
    public class Placeable : MonoBehaviour
    {
        public PlaceableType pType;
		
        [HideInInspector] public Faction faction;
        [HideInInspector] public PlaceableTarget targetType; //TODO: move to ThinkingPlaceable?
		[HideInInspector] public AudioClip dieAudioClip;

        public UnityAction<Placeable> OnDie;

        public enum PlaceableType
        {
            Unit, // 游戏单位（特指那些可移动的物体）
            Obstacle, // 障碍物（陨石）
            Building, // 建筑物（生产游戏单位）
            Spell, // 法术（对目标造成瞬时或者持续性的伤害）
            Castle, // 城堡（特殊类型的游戏单位）
        }

        public enum PlaceableTarget
        {
            OnlyBuildings, // 仅建筑
            Both, // 建筑和敌人
            None, // 无法攻击
        }

        public enum Faction
        {
            Player, //Red
            Opponent, //Blue
            None,
        }
    }
}