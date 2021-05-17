using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//继承MyMonoSingleton<MyStart>就会创建一个跨创建的物体并且添加上<MyStart>脚本
public class MyStart : MyMonoSingleton<MyStart>
{
    // Start is called before the first frame update
    async void Start()
    {
        try
        {
            //非编制器下执行
#if UNITY_STANDALONE
            Screen.SetResolution(540, 960, false);
#endif

            UIRoot.SetInitParams(new Vector2(1080, 1920));
            UIPage.ShowPageAsync<LogoPage>();

            Debug.Log("#Sequence# MyClient.Init()--->>>");
            await MyClient.Init();
            Debug.Log("#Sequence# MyClient.Init()<<<---");

        }
        catch (System.Exception e)
        {

            Debug.LogException(e);
        }

        //MonoBridge.instance.OnUpdate += MyClient.cardMgr.Update;
        //MonoBridge.instance.OnIMGUI += MyClient.cardMgr.OnGUI;

    }

    
}
