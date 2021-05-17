
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityRoyale;

using Lockstep.Math;



[Serializable]
public partial class MyPlaceable
{
		public uint id;

		public string name;

		public Placeable.PlaceableType pType;

		public string associatedPrefab;

		public string alternatePrefab;

		public ThinkingPlaceable.AttackType attackType;

		public Placeable.PlaceableTarget targetType;

		public LFloat attackRatio;

		public LFloat damagePerAttack;

		public LFloat attackRange;

		public LFloat hitPoints;

		public string attackClip;

		public string dieClip;

		public LFloat speed;

		public LFloat lifeTime;

		public LFloat damagePerSecond;

		public string redProjPrefab;

		public string blueProjPrefab;

		public string firePos;

		public LFloat dieDuration;

		public bool isAttackable;


}

[Serializable]
public partial class MyPlaceableModel
{
	public List<MyPlaceable> list = new List<MyPlaceable>();

    public MyPlaceable FindById(int id)
    {
       return list.Find((c)=> c.id == id).As<MyPlaceable>();
    }

	public MyPlaceableModel()
	{
		list.Add(new MyPlaceable(){
			id = 10000,
			name = "弓箭手",
			pType = Placeable.PlaceableType.Unit,
			associatedPrefab = "弓箭手-红",
			alternatePrefab = "弓箭手-蓝",
			attackType = ThinkingPlaceable.AttackType.Ranged,
			targetType = Placeable.PlaceableTarget.Both,
			attackRatio = new LFloat(true,(long)(1.5f * LFloat.Precision)),
			damagePerAttack = new LFloat(true,(long)(1f * LFloat.Precision)),
			attackRange = new LFloat(true,(long)(6f * LFloat.Precision)),
			hitPoints = new LFloat(true,(long)(8f * LFloat.Precision)),
			attackClip = "",
			dieClip = "",
			speed = new LFloat(true,(long)(4f * LFloat.Precision)),
			lifeTime = new LFloat(true,(long)(0f * LFloat.Precision)),
			damagePerSecond = new LFloat(true,(long)(1f * LFloat.Precision)),
			redProjPrefab = "剑",
			blueProjPrefab = "剑",
			firePos = "",
			dieDuration = new LFloat(true,(long)(10f * LFloat.Precision)),
			isAttackable = true,
		});

		list.Add(new MyPlaceable(){
			id = 10001,
			name = "法师",
			pType = Placeable.PlaceableType.Unit,
			associatedPrefab = "法师-红",
			alternatePrefab = "法师-蓝",
			attackType = ThinkingPlaceable.AttackType.Ranged,
			targetType = Placeable.PlaceableTarget.Both,
			attackRatio = new LFloat(true,(long)(1.5f * LFloat.Precision)),
			damagePerAttack = new LFloat(true,(long)(3f * LFloat.Precision)),
			attackRange = new LFloat(true,(long)(4f * LFloat.Precision)),
			hitPoints = new LFloat(true,(long)(10f * LFloat.Precision)),
			attackClip = "",
			dieClip = "",
			speed = new LFloat(true,(long)(3f * LFloat.Precision)),
			lifeTime = new LFloat(true,(long)(0f * LFloat.Precision)),
			damagePerSecond = new LFloat(true,(long)(1f * LFloat.Precision)),
			redProjPrefab = "火球-红",
			blueProjPrefab = "火球-蓝",
			firePos = "",
			dieDuration = new LFloat(true,(long)(10f * LFloat.Precision)),
			isAttackable = true,
		});

		list.Add(new MyPlaceable(){
			id = 10002,
			name = "战士",
			pType = Placeable.PlaceableType.Unit,
			associatedPrefab = "战士-红",
			alternatePrefab = "战士-蓝",
			attackType = ThinkingPlaceable.AttackType.Melee,
			targetType = Placeable.PlaceableTarget.Both,
			attackRatio = new LFloat(true,(long)(2.5f * LFloat.Precision)),
			damagePerAttack = new LFloat(true,(long)(4f * LFloat.Precision)),
			attackRange = new LFloat(true,(long)(2.5f * LFloat.Precision)),
			hitPoints = new LFloat(true,(long)(20f * LFloat.Precision)),
			attackClip = "",
			dieClip = "",
			speed = new LFloat(true,(long)(2f * LFloat.Precision)),
			lifeTime = new LFloat(true,(long)(0f * LFloat.Precision)),
			damagePerSecond = new LFloat(true,(long)(1f * LFloat.Precision)),
			redProjPrefab = "",
			blueProjPrefab = "",
			firePos = "",
			dieDuration = new LFloat(true,(long)(10f * LFloat.Precision)),
			isAttackable = true,
		});

		list.Add(new MyPlaceable(){
			id = 10003,
			name = "国王塔弓箭手",
			pType = Placeable.PlaceableType.Guard,
			associatedPrefab = "弓箭手-红",
			alternatePrefab = "弓箭手-蓝",
			attackType = ThinkingPlaceable.AttackType.Ranged,
			targetType = Placeable.PlaceableTarget.Both,
			attackRatio = new LFloat(true,(long)(1.5f * LFloat.Precision)),
			damagePerAttack = new LFloat(true,(long)(1f * LFloat.Precision)),
			attackRange = new LFloat(true,(long)(6f * LFloat.Precision)),
			hitPoints = new LFloat(true,(long)(2f * LFloat.Precision)),
			attackClip = "",
			dieClip = "",
			speed = new LFloat(true,(long)(0f * LFloat.Precision)),
			lifeTime = new LFloat(true,(long)(0f * LFloat.Precision)),
			damagePerSecond = new LFloat(true,(long)(1f * LFloat.Precision)),
			redProjPrefab = "剑",
			blueProjPrefab = "剑",
			firePos = "",
			dieDuration = new LFloat(true,(long)(10f * LFloat.Precision)),
			isAttackable = false,
		});

		list.Add(new MyPlaceable(){
			id = 10004,
			name = "国王塔",
			pType = Placeable.PlaceableType.Castle,
			associatedPrefab = "国王塔-红",
			alternatePrefab = "国王塔-蓝",
			attackType = ThinkingPlaceable.AttackType.Melee,
			targetType = Placeable.PlaceableTarget.None,
			attackRatio = new LFloat(true,(long)(0f * LFloat.Precision)),
			damagePerAttack = new LFloat(true,(long)(0f * LFloat.Precision)),
			attackRange = new LFloat(true,(long)(0f * LFloat.Precision)),
			hitPoints = new LFloat(true,(long)(2f * LFloat.Precision)),
			attackClip = "",
			dieClip = "",
			speed = new LFloat(true,(long)(0f * LFloat.Precision)),
			lifeTime = new LFloat(true,(long)(0f * LFloat.Precision)),
			damagePerSecond = new LFloat(true,(long)(1f * LFloat.Precision)),
			redProjPrefab = "",
			blueProjPrefab = "",
			firePos = "",
			dieDuration = new LFloat(true,(long)(10f * LFloat.Precision)),
			isAttackable = true,
		});


	}

	public static MyPlaceableModel instance = new MyPlaceableModel();
}
