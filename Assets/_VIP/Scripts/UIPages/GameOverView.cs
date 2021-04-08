using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityRoyale.Placeable;
using DG.Tweening;

public partial class GameOverPage
{
	public GameOverPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None)
	{
		Debug.LogWarning("TODO: 请修改GameOverPage页面类型等参数，或注释此行");
	}

	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		oKButton.onClick.AddListener(()=> {
			UIPage.CloseAllPages();
			Addressables.LoadSceneAsync("Main");
		});
	}

	protected override void OnActive()
	{
		//每次显示该UI,都要显示获胜方动画
		var faction=(Faction)data;
		var winner = faction == Faction.Player ? kingBlue : kingRed;

		var cg = winner.GetComponent<CanvasGroup>();
		cg.alpha = 0;
		cg.DOFade(1, 4f);//淡入

		winner.transform.DOShakeScale(1.5f);//震动

		winImage.transform.localPosition = winner.localPosition;//胜利文字设为胜利方		
	}

	//public void MyEventHandler()
	//{
	//}
}
