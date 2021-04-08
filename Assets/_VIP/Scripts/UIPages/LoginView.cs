using System.Collections.Generic;
using UnityEngine;

public partial class LoginPage
{
	public LoginPage() : base(UIType.Normal, UIMode.DoNothing, UICollider.None)
	{
		Debug.LogWarning("TODO: 请修改LoginPage页面类型等参数，或注释此行");
	}

	public void OnStart()
	{
		//KBEngine.Event.registerOut("MyEventName", this, "MyEventHandler");
	}

	//public void MyEventHandler()
	//{
	//}
}
