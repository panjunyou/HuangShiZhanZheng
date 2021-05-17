using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRoyale;

public class MyPlaceableView : MyView 
{
    public MyPlaceable data
    {
        get
        {
            //从基类的MyEntity转换到派生类，可以获得更多数据成员的访问
            return dataBase as MyPlaceable;
        }
    }

    public float DieDuration=>data.dieDuration.ToFloat();//死亡时长

    public float DieProgress=0;//死亡进度

    #region 视图层状态更新
    public override void OnViewUpdate(float lerpT)
    {
        //调用基类法发做位置和朝向
        base.OnViewUpdate(lerpT);

        //绘制寻路路径（调试）
        DrawPath();
        //绘制攻击目标（调试）
       // DrawTarget();

        //视图层状态更新
        switch (data.state)
        {
            case AIState.Idle:
                {
                   
                }
                break;
            case AIState.Seek:
                {
                   
                }
                break;
            case AIState.Attack:
                {
                   
                }
                break;
            case AIState.Die:
                {
                    //更新死亡时做一个溶解动画
                    var rds = this.transform.GetComponentsInChildren<Renderer>();
                    DieProgress += Time.deltaTime * (1/DieDuration);
                    foreach (var rd in rds)
                    {
                        rd.material.SetFloat("_DissolveFactor",DieProgress);
                    }
                }
                break;
        }
    }

    private void DrawPath()
    {
        if (data.path==null )
        {
            return;
        }

        for (int i = 0; i < data.path.Count - 1; i++)
        {
            Debug.DrawLine(data.path[i].ToVector3(), data.path[i + 1].ToVector3(), data.faction == Placeable.Faction.Red ? Color.red : Color.blue);
        }
    }
    private void DrawTarget()
    {
        if (data .target==null )
        {
            return;
        }

        Debug.DrawLine(data.worldPos.ToVector3(), data.target.worldPos.ToVector3(), data.faction == Placeable.Faction.Red ? Color.red : Color.blue);
    }
    #endregion

    #region 视图层状态变迁处理
    public virtual void OnEnterIdle()
    {
        this.transform.GetComponent<Animator>().SetBool("IsMoving",false );
    }
    public virtual void OnLeaveIdle()
    {

    }
    public virtual void OnEnterSeek()
    {
        //行走动画
        this.transform.GetComponent<Animator>().SetBool("IsMoving", true);
    }
    public virtual void OnLeaveSeek()
    {

    }
    public virtual void OnEnterAttack()
    {
        this.transform.GetComponent<Animator>().SetBool("IsMoving", false);
    }
    public virtual void OnLeaveAttack()
    {

    }
    public virtual void OnEnterDie()
    {
       // print($"{this.gameObject.name } is dead!");
        Debug.Log($"{this.gameObject.name } is dead!");

        //播放死亡动画
        if ( this.transform.GetComponent <Animator>()!=null )
        {
            this.transform.GetComponent<Animator>().SetTrigger("IsDead");
        }
        //初始化死亡溶解
        var rds = this.transform.GetComponentsInChildren<Renderer>();
       // var view = this.transform.GetComponent<MyPlaceableView>();
        var color = this.data.faction == Placeable.Faction.Red ? Color.red : Color.blue;
        this.DieProgress = 0;
        foreach (var rd in rds)
        {
            rd.material.SetColor("_EdgeColor", color * 8);
            rd.material.SetFloat("_EdgeWidth",0.1f);
            rd.material.SetFloat("_DissolveFactor", this.DieProgress);
        }       
    }
    public virtual void OnLeaveDie()
    {

    }
   
    #endregion

    #region Messages
    public void OnPlayAttackAnim()
    {
        this.transform.GetComponent<Animator>().SetTrigger("Attack");
    }
    #endregion

}
