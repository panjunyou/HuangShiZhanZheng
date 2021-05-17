using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;

public partial class LogoPage
{
	//private float showSeconds = 2f;


	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
		//加载主场景
		//this.progressSlider.DOValue(1, showSeconds).OnComplete(()=> {
		//	//UIPage.ShowPageAsync<MainPage>();
		//	Addressables.LoadSceneAsync("Main");
		//});
	}

	protected override void OnActive()
	{
		UIPage.ShowPageAsync<LoginPage>();
	}

	protected override void OnHide()
	{
		UIPage.ClosePage<LoginPage>();
	}
	//public void MyEventHandler()
	//{
	//}
}
