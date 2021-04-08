using System.Collections.Generic;
using UnityEngine;

public partial class DeckPage
{
	public DeckPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None)
	{
		Debug.LogWarning("TODO: 请修改DeckPage页面类型等参数，或注释此行");
	}

	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		MyCardMgr.Instance.canvas = this.transform;
		MyCardMgr.Instance.startPos = startPos;
		MyCardMgr.Instance.endPos = endPos;

		for (int i = 0; i < panel.transform.childCount; i++)
		{
			MyCardMgr.Instance.cards[i] = panel.transform.GetChild(i).transform;
		}
		
	}

	//public void MyEventHandler()
	//{
	//}
}
