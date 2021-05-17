using Lockstep.Math;
using UnityEngine;

/// <summary>
///对象视图 
/// </summary>
public class MyView 
{
    public MyEntity dataBase;//访问到视图对应的实体的基类部分

    public Transform transform;

    public GameObject gameObject
    {
        get { return transform.gameObject; }
    }
    /// <summary>
    /// 表现层更新的时间处理（被帧同步框架调用）
    /// </summary>
    /// <param name="lerpT">插值系数=（当前时间-上帧结束时间）/（本帧结束时间-上帧结束时间）</param>
    public virtual void OnViewUpdate(float lerpT)
    {
        #region 状态插值
        //从lastWorldPos到worldPos
        gameObject.transform.position = Vector3.Lerp(dataBase.lastWorldPos.ToVector3(), dataBase.worldPos.ToVector3(), dataBase.lerpT.ToFloat());
        Debug.DrawLine(dataBase.lastWorldPos.ToVector3(), dataBase.worldPos.ToVector3(), Color.white);

        //从lastRot到rot
        gameObject.transform.rotation = Quaternion.Lerp(dataBase.lastRot.ToQuaternion(), dataBase.rot.ToQuaternion(), 1);
        #endregion
    }

    public virtual void OnSetWorldPos(LVector3 val)
    {
        transform.position = val.ToVector3();
    }

    public virtual void OnSetRotation(LQuaternion val)
    {
        transform.rotation = val.ToQuaternion();
    }
}