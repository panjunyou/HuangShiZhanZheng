using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using KBEngine;
using UnityEngine.SceneManagement;

public partial class MainPage
{
	

	public async void OnStart()
	{
		//当本地玩家进入Space时调用
		KBEngine.Event.registerOut(EventOutTypes.onEnterSpace, this, "OnEnterSpace");
		Debug.Log("#Sequence# 主UIregisterOut(OnGameReady)");
		KBEngine.Event.registerOut("OnGameReady", this, "OnGameReady");

		#region 客户端部分
		if (SceneManager.GetSceneByName("Battle").isLoaded == false)
		{
			Debug.Log("#Sequence# 正在加载战斗场景。。。");

			//在进入主场景就提前加在（被UI遮挡），如果有多个战斗场景，就动态加载
			await Addressables.LoadSceneAsync("Battle", LoadSceneMode.Single, true).Task;

						
		}
		#endregion

		this.battleButton.onClick.AddListener( () => {           

            //TODO :显示匹配中UI
            #region 服务器部分
            Debug.Log("#Sequence# 正在准备进入房间。。。");
			KBEngine.Event.fireIn("EnterRoom",false);
            #endregion
        });
		
    }

    public void OnEnterSpace(Entity e)
	{
		Dbg.INFO_MSG("已进入房间...");
		//Addressables.LoadSceneAsync("Battle").Completed += MainPage_Completed;

		// TODO:显示匹配中
	}

	public void OnGameReady(uint sees, byte frameRate, int idx)
	{
		#region 客户端部分
		//关闭主UI
		Debug.Log("#Sequence# 正在关闭关闭主UI。。。");
		UIPage.ClosePage<MainPage>();
		#endregion

		//更新开始
		if (MonoBridge.instance.OnUpdate == null && MonoBridge.instance.OnIMGUI == null)
		{
			MonoBridge.instance.OnUpdate += MyClient.cardMgr.Update;
			MonoBridge.instance.OnIMGUI += MyClient.cardMgr.OnGUI;
			Debug.Log("#Sequence# --------------------------》》》》MonoBridge启动更新《《《《---------------------------");
		}
	}
	//private void MainPage_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> obj)
	//{
	//	UIPage.CloseAllPages();

	//}

	//public void MyEventHandler()
	//{
	//}

	protected override void OnActive()
	{
		UIPage.ShowPageAsync<TopFixPage>();
		UIPage.ShowPageAsync<BottomFixPage>();
	}

	protected override void OnHide()
	{
		//关闭上下UI
		UIPage.ClosePage<TopFixPage>();
		UIPage.ClosePage<BottomFixPage>();
	}
}
