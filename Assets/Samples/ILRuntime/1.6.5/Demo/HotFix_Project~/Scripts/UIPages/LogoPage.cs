using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class LogoPage : UIPage
{
public LogoPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None){ }

	public Slider progressSlider;


	protected override string uiPath => "LogoPage";

	protected override void OnAwake()
	{
		progressSlider = transform.Find("ProgressSlider").GetComponent<Slider>();

		OnStart();
	}
}