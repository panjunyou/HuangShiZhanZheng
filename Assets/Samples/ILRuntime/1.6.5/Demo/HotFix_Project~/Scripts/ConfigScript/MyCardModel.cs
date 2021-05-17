
using System;
using System.Collections.Generic;
using UnityEngine;

using Lockstep.Math;



[Serializable]
public partial class MyCard
{
		public uint id;

		public string name;

		public string cardPrefab;

		public int[] placeablesIndices;

		public LVector3[] relativeOffsets;

		public bool isPickable;


}

[Serializable]
public partial class MyCardModel
{
	public List<MyCard> list = new List<MyCard>();

    public MyCard FindById(int id)
    {
       return list.Find((c)=> c.id == id).As<MyCard>();
    }

	public MyCardModel()
	{
		list.Add(new MyCard(){
			id = 20000,
			name = "弓箭手卡牌",
			cardPrefab = "弓箭手卡牌",
			placeablesIndices = new []{10000, 10000, 10000},
			relativeOffsets = new []{new LVector3(true,(long)(0.87f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0.5f*LFloat.Precision)), new LVector3(true,(long)(0f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0f*LFloat.Precision)), new LVector3(true,(long)(-0.87f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0.5f*LFloat.Precision))},
			isPickable = true,
		});

		list.Add(new MyCard(){
			id = 20001,
			name = "法师卡牌",
			cardPrefab = "法师卡牌",
			placeablesIndices = new []{10001},
			relativeOffsets = new []{new LVector3(true,(long)(0f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0f*LFloat.Precision))},
			isPickable = true,
		});

		list.Add(new MyCard(){
			id = 20002,
			name = "战士卡牌",
			cardPrefab = "战士卡牌",
			placeablesIndices = new []{10002},
			relativeOffsets = new []{new LVector3(true,(long)(0f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0f*LFloat.Precision))},
			isPickable = true,
		});

		list.Add(new MyCard(){
			id = 20003,
			name = "国王塔弓箭手卡牌",
			cardPrefab = "弓箭手卡牌",
			placeablesIndices = new []{10003},
			relativeOffsets = new []{new LVector3(true,(long)(0f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0f*LFloat.Precision))},
			isPickable = false,
		});

		list.Add(new MyCard(){
			id = 20004,
			name = "国王塔卡牌",
			cardPrefab = "国王塔卡牌",
			placeablesIndices = new []{10004},
			relativeOffsets = new []{new LVector3(true,(long)(0f*LFloat.Precision), (long)(0f*LFloat.Precision),(long)(0f*LFloat.Precision))},
			isPickable = false,
		});


	}

	public static MyCardModel instance = new MyCardModel();
}
