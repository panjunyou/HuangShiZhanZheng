using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AddressableAssets;

public partial class LogoPage
{
	private float showSeconds = 2f;
	public LogoPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
	{
		Debug.LogWarning("TODO: 请修改LogoPage页面类型等参数，或注释此行");
	}

	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
		//加载主场景
		this.progressSlider.DOValue(1, showSeconds).OnComplete(()=> {
			//UIPage.ShowPageAsync<MainPage>();
			Addressables.LoadSceneAsync("Main");
		});
	}

	//public void MyEventHandler()
	//{
	//}
}
