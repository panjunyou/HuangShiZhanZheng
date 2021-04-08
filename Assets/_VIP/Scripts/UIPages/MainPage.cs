using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class MainPage : UIPage
{
	public Button playerInfoButton;
	public Image forceIconImage;
	public Text playerNameText;
	public Text forceNameText;
	public Text cupNumText;
	public Button settingsButton;
	public Button freeChestButton;
	public Text waitTimeText;
	public Button royalChestButton;
	public Slider chestNumSlider;
	public Text chestNumText;
	public Button battleButton;


	protected override string uiPath => "MainPage";

	protected override void OnAwake()
	{
		playerInfoButton = transform.Find("Top1/PlayerInfoButton").GetComponent<Button>();
		forceIconImage = transform.Find("Top1/ForceIconImage").GetComponent<Image>();
		playerNameText = transform.Find("Top1/PlayerNameText").GetComponent<Text>();
		forceNameText = transform.Find("Top1/ForceNameText").GetComponent<Text>();
		cupNumText = transform.Find("Top1/CupNumText").GetComponent<Text>();
		settingsButton = transform.Find("Top1/SettingsButton").GetComponent<Button>();
		freeChestButton = transform.Find("Top2/FreeChestButton").GetComponent<Button>();
		waitTimeText = transform.Find("Top2/FreeChestButton/WaitTimeText").GetComponent<Text>();
		royalChestButton = transform.Find("Top2/RoyalChestButton").GetComponent<Button>();
		chestNumSlider = transform.Find("Top2/RoyalChestButton/ChestNumSlider").GetComponent<Slider>();
		chestNumText = transform.Find("Top2/RoyalChestButton/ChestNumText").GetComponent<Text>();
		battleButton = transform.Find("MidBottom/BattleButton").GetComponent<Button>();

		OnStart();
	}
}