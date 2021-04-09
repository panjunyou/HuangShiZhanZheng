using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public partial class MainPage
{
	public MainPage() : base(UIType.Normal, UIMode.HideOther, UICollider.None)
	{
		Debug.LogWarning("TODO: 请修改MainPage页面类型等参数，或注释此行");
	}

	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
		
		this.battleButton.onClick.AddListener(() => {
			Addressables.LoadSceneAsync("Battle").Completed += MainPage_Completed;			
		});		
	}

	private void MainPage_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> obj)
	{
		UIPage.CloseAllPages();
		
	}

	//public void MyEventHandler()
	//{
	//}

	protected override void OnActive()
	{
		UIPage.ShowPageAsync<TopFixPage>();
		UIPage.ShowPageAsync<BottomFixPage>();
	}
}
