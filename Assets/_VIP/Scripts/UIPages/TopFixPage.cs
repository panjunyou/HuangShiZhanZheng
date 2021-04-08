using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class TopFixPage : UIPage
{
	public Slider expSlider;
	public Text expText;
	public Text lvText;
	public Button addGoldButton;
	public Text goldText;
	public Button addGemButton;
	public Text gemText;


	protected override string uiPath => "TopFixPage";

	protected override void OnAwake()
	{
		expSlider = transform.Find("Top/Lv/ExpSlider").GetComponent<Slider>();
		expText = transform.Find("Top/Lv/ExpSlider/ExpText").GetComponent<Text>();
		lvText = transform.Find("Top/Lv/Image/LvText").GetComponent<Text>();
		addGoldButton = transform.Find("Top/Gold/AddGoldButton").GetComponent<Button>();
		goldText = transform.Find("Top/Gold/GoldText").GetComponent<Text>();
		addGemButton = transform.Find("Top/Gem/AddGemButton").GetComponent<Button>();
		gemText = transform.Find("Top/Gem/GemText").GetComponent<Text>();

		OnStart();
	}
}