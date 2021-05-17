using KBEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class LoginPage
{	

	public void OnStart()
	{
		this.accInput.text = "111111";
		this.pwdInput.text = "111111";
		//KB的系统事件，登录成功会受到通知
		KBEngine.Event.registerOut(KBEngine.EventOutTypes.onLoginBaseapp,this, "OnLoginBaseapp");

		this.accPwSlider.onValueChanged.AddListener((value) => {
			//字符串替换
			string s=new string('x', 6).Replace("x", value.ToString());
			this.accInput.text = s;
			this.pwdInput.text = s;
		});

		this.loginButton.onClick.AddListener(()=> {
			Dbg.DEBUG_MSG("loginButton.onClick");

			KBEngine.Event.fireIn(KBEngine.EventInTypes.login,this.accInput.text,this.pwdInput.text,Encoding.ASCII.GetBytes("panjunyou"));
		});
	}
	/// <summary>
	/// 当登录成功时调用
	/// </summary>
	public void OnLoginBaseapp()
	{
		UIPage.ClosePage<LogoPage>();
		//登录成功加载主场景
		//Addressables.LoadSceneAsync("Main");
		UIPage.ShowPageAsync<MainPage>();
	
	}


}
