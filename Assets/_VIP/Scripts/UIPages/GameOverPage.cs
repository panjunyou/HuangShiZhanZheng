using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class GameOverPage : UIPage
{
	public Button oKButton;
	public Transform kingRed;
	public Image winImage;
	public Transform kingBlue;


	protected override string uiPath => "GameOverPage";

	protected override void OnAwake()
	{
		oKButton = transform.Find("OKButton").GetComponent<Button>();
		kingRed = transform.Find("KingRed").GetComponent<Transform>();
		winImage = transform.Find("WinImage").GetComponent<Image>();
		kingBlue = transform.Find("KingBlue").GetComponent<Transform>();

		OnStart();
	}
}