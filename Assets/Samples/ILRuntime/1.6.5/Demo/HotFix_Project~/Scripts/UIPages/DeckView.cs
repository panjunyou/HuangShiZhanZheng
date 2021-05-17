using System.Collections.Generic;
using UnityEngine;

public partial class DeckPage
{


	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");

		//MyCardMgr.Instance.canvas = this.transform;
		//MyCardMgr.Instance.startPos = startPos;
		//MyCardMgr.Instance.endPos = endPos;

		//for (int i = 0; i < panel.transform.childCount; i++)
		//{
		//	MyCardMgr.Instance.cards[i] = panel.transform.GetChild(i).transform;
		//}
		
	}

	//public void MyEventHandler()
	//{
	//}

	protected override void OnActive()
	{
		MyClient.cardMgr.myCardMgr = this.myCardMgr;
		MyClient.cardMgr.startPos = startPos;
		MyClient.cardMgr.endPos = endPos;

		MyClient.cardMgr.cards = new Transform[panel.transform.childCount];

		for (int i = 0; i < panel.transform.childCount; i++)
		{
			MyClient.cardMgr.cards[i] = panel.transform.GetChild(i).transform;
		}
	}

	protected override void OnHide()
	{
		foreach (Transform card in myCardMgr)
		{
			GameObject.Destroy(card.gameObject);
		}
	}
}
