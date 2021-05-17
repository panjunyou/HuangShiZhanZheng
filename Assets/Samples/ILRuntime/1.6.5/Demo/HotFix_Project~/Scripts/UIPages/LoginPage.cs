using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class LoginPage : UIPage
{
public LoginPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None){ }

	public InputField accInput;
	public InputField pwdInput;
	public Button loginButton;
	public Slider accPwSlider;


	protected override string uiPath => "LoginPage";

	protected override void OnAwake()
	{
		accInput = transform.Find("AccInput").GetComponent<InputField>();
		pwdInput = transform.Find("PwdInput").GetComponent<InputField>();
		loginButton = transform.Find("LoginButton").GetComponent<Button>();
		accPwSlider = transform.Find("AccPwSlider").GetComponent<Slider>();

		OnStart();
	}
}