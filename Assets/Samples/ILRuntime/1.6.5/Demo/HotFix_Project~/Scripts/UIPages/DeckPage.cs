using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class DeckPage : UIPage
{
public DeckPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None){ }

	public Image panel;
	public Transform startPos;
	public Transform endPos;
	public Transform myCardMgr;


	protected override string uiPath => "DeckPage";

	protected override void OnAwake()
	{
		panel = transform.Find("Panel").GetComponent<Image>();
		startPos = transform.Find("StartPos").GetComponent<Transform>();
		endPos = transform.Find("EndPos").GetComponent<Transform>();
		myCardMgr = transform.Find("MyCardMgr").GetComponent<Transform>();

		OnStart();
	}
}