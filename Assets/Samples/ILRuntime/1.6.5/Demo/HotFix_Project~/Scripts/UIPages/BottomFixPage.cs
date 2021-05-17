using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class BottomFixPage : UIPage
{
public BottomFixPage() : base(UIType.Fixed, UIMode.DoNothing, UICollider.None){ }



	protected override string uiPath => "BottomFixPage";

	protected override void OnAwake()
	{

		OnStart();
	}
}