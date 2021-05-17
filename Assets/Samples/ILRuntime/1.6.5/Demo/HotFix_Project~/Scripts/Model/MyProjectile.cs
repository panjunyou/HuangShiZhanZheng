using Lockstep.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProjectile : MyEntity
{
    public MyPlaceable caster;//投放者
    public MyPlaceable target;//目标

    public  LFloat progress=0;

    public LFloat Speed= 0.25f.ToLFloat();//速度，按秒

    public bool isUse = false;//这投掷物是否使用，使用才能执行它


    #region 重写的虚方法
    public override void OnDestroy()
    {
        MyClient.projMgr.MineProjList.Remove(this);
        MyClient.projMgr.HisProjList.Remove(this);
    }
    #endregion
}
