
using System;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public partial class MyCard
{
		public uint id;

		public string name;

		public string cardPrefab;

		public int[] placeablesIndices;

		public Vector3[] relativeOffsets;


}

[Serializable]
public partial class MyCardModel
{
	public List<MyCard> list = new List<MyCard>();

	public MyCardModel()
	{
		list.Add(new MyCard(){
			id = 20000,
			name = "Archers",
			cardPrefab = "弓箭手卡牌",
			placeablesIndices = new []{10000, 10000, 10000},
			relativeOffsets = new []{new Vector3(0.87f, 0f, 0.5f), new Vector3(0f, 0f, 0f), new Vector3(-0.87f, 0f, 0.5f)},
		});

		list.Add(new MyCard(){
			id = 20001,
			name = "Mage",
			cardPrefab = "法师卡牌",
			placeablesIndices = new []{10001},
			relativeOffsets = new []{new Vector3(0f, 0f, 0f)},
		});

		list.Add(new MyCard(){
			id = 20002,
			name = "Warrior",
			cardPrefab = "战士卡牌",
			placeablesIndices = new []{10002},
			relativeOffsets = new []{new Vector3(0f, 0f, 0f)},
		});


	}

	public static MyCardModel instance = new MyCardModel();
}
