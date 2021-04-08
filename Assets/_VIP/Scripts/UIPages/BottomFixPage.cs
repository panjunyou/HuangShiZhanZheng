using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class BottomFixPage : UIPage
{


	protected override string uiPath => "BottomFixPage";

	protected override void OnAwake()
	{

		OnStart();
	}
}