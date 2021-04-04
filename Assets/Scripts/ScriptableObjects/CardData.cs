using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityRoyale
{
	// 卡牌数据类
    [CreateAssetMenu(fileName = "NewCard", menuName = "Unity Royale/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Card graphics")]
        //public Sprite cardImage;
        public GameObject cardPrefab; // 游戏中卡牌预制体

        [Header("List of Placeables")]
        public PlaceableData[] placeablesData; // 链接到所有可以放置的卡牌（放置以后变成游戏单位）
        public Vector3[] relativeOffsets; // 每一个游戏单位之间应该保持一定的距离，是他们不要重叠在一起
    }
}