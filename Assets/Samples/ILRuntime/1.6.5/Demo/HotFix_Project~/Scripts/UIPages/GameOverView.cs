using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityRoyale.Placeable;
using DG.Tweening;

public partial class GameOverPage
{


	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		oKButton.onClick.AddListener(()=> {
			UIPage.CloseAllPages();
			//Addressables.LoadSceneAsync("Main");
			UIPage.ShowPageAsync<MainPage>();

			//复位相机
			Camera.main.transform.position = MyClient.sceneInfo.GetObjPos("Battle", "Main Camera").ToVector3();
			Camera.main.transform.rotation= MyClient.sceneInfo.GetObjRot("Battle", "Main Camera").ToQuaternion();
		});
	}

	protected override void OnActive()
	{
		//每次显示该UI,都要显示获胜方动画
		Debug.Log($"OnActive(): {data}");
		var faction=(Faction)data;
		Transform winner =  null;

		winImage.gameObject.SetActive(false);

		kingBlue.GetComponent<CanvasGroup>().alpha=0;
		kingRed.GetComponent<CanvasGroup>().alpha = 0;

		if (faction==Faction.Red)
		{
			winner = kingRed;
		}else if (faction == Faction.Blue)
		{
			winner = kingBlue;
		}
		else
		{
			return;
		}

		var cg = winner.GetComponent<CanvasGroup>();
		cg.alpha = 0;
		cg.DOFade(1, 4f);//淡入

		winner.transform.DOShakeScale(1.5f);//震动

		winImage.gameObject.SetActive(true);

		winImage.transform.localPosition = winner.localPosition;//胜利文字设为胜利方		
	}

	//public void MyEventHandler()
	//{
	//}
}
