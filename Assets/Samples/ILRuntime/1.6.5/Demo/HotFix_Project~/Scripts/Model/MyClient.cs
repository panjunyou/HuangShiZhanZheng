using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

/// <summary>
/// 一个全局静态类，可以视为客户端单件对象
/// </summary>
public static  class MyClient 
{
    public static MyCardMgr cardMgr;//场景信息模型
    public static MyScenelnfoModel sceneInfo;//场景信息模型
    public static MyPlaceableMgr placeableMgr;//兵种管理器
    public static MyProjectileMgr projMgr;//子弹管理器
    public static CPU cpu;//自动出兵

    public static bool isInit = false;//标记客户端是否初始化完毕

    /// <summary>
    /// 客户端初始化
    /// </summary>
    /// <returns></returns>
    public static async Task Init()
    {
        cardMgr = await MyEntity.Instanciate<MyCardMgr, MyView>(null,null,null,null,false,true);
        Debug.Log("#Sequence# cardMgr insstanciate ok");

        sceneInfo = await MyEntity.Instanciate<MyScenelnfoModel, MyView>(null, null, null, null, false, true);
        Debug.Log("#Sequence# sceneInfo insstanciate ok");

        placeableMgr = await MyEntity.Instanciate<MyPlaceableMgr, MyView>(null, null, null, null, false, true);
        Debug.Log("#Sequence# placeableMgr insstanciate ok");

        projMgr = await MyEntity.Instanciate<MyProjectileMgr, MyView>(null, null, null, null, false, true);
        Debug.Log("#Sequence# projMgr insstanciate ok");

        bool useCpu = PlayerPrefs.GetInt("CpuEnabled") != 0;
        if (useCpu)
        {
            cpu = await MyEntity.Instanciate<CPU, MyView>(null, null, null, null, false, true);
            Debug.Log("#Sequence# CPU insstanciate ok");
        }


        isInit = true;
    }

    
}