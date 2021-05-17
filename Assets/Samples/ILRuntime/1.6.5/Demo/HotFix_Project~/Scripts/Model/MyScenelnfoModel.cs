using ILitJson;
using Lockstep.Math;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyScenelnfoModel : MyEntity
{
    private JsonData jdRoot;

    public override async Task OnAwake()
    {
        TextAsset ta = await Addressables.LoadAssetAsync<TextAsset>("Battle").Task;
        jdRoot = JsonMapper.ToObject(ta.text);  
    }
    /// <summary>
    /// json文件采用的时二级结构
    /// 一级：场景名
    /// 二级：节点路径
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <param name="objPath">节点路径</param>
    /// <param name="pos">返回的节点位置</param>
    /// <param name="rot">返回的节点朝向</param>
    private void _GetGameObject(string sceneName,string objPath, out LVector3 pos,out LQuaternion rot)
    {
        JsonData jd = jdRoot[sceneName][objPath];

        JsonData jdPos = jd["position"];
        JsonData jdRot = jd["rotation"];

        pos = new LVector3(
            true,
            long.Parse(jdPos["x"].ToString()),
            long.Parse(jdPos["y"].ToString()),
            long.Parse(jdPos["z"].ToString())
            );
        rot = new LQuaternion(
            new LFloat(true, long.Parse(jdRot["x"].ToString())),
            new LFloat(true, long.Parse(jdRot["y"].ToString())),
            new LFloat(true, long.Parse(jdRot["z"].ToString())),
            new LFloat(true, long.Parse(jdRot["w"].ToString()))
            );
    }

    #region 工具函数
    /// <summary>
    /// 单独返回节点位置
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="objPath"></param>
    /// <returns></returns>
    public LVector3 GetObjPos(string sceneName,string objPath)
    {
        _GetGameObject(sceneName, objPath, out LVector3 pos, out _);
        return pos;
    }
    /// <summary>
    /// 单独返回节点朝向
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="objPath"></param>
    /// <returns></returns>
    public LQuaternion GetObjRot(string sceneName, string objPath)
    {
        _GetGameObject(sceneName, objPath, out _, out LQuaternion rot);
        return rot;
    }

    public override void OnDestroy()
    {
        MyClient.sceneInfo = null;
    }
    #endregion
}