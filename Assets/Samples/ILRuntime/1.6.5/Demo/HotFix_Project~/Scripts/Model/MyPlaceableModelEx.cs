using UnityEngine;
using static UnityRoyale.Placeable;
using Lockstep.Math;
using System.Linq;
using System.Collections.Generic;

public enum AIState
{
    Idle,
    Seek,
    Attack,
    Die
}

public partial class MyPlaceable : MyEntity
{
    /// <summary>
    /// 获取敌方阵营
    /// </summary>
    public Faction OppnentFaction
    {
        get
        {
            if (faction==Faction.Red)
            {
                return Faction.Blue;
            }
            else
            {
                return Faction.Red;
            }
        }
    }
    public MyPlaceableView view
    {
        get { return viewBase.As<MyPlaceableView>(); }
    }
    private LVector3 _localPos = LVector3.zero;

    public LVector3 localPos
    {
        get { return _localPos; }
        set
        {
            _localPos = value;

            worldPos = parentPos + _localPos;//每更新局部坐标，都要算世界坐标
        }
    }
    private LVector3 _parentPos = LVector3.zero;
    public LVector3 parentPos
    {
        get
        {
            return _parentPos;
        }
        set
        {
            _parentPos = value;

            worldPos = parentPos + _localPos;//每更新父节点坐标，都要算世界坐标
        }
    }

    public Faction faction = Faction.None;//阵营
    public bool isPreview = false; //该兵种是否预览状态
    public MyPlaceable target = null;//攻击目标

    private AIState _state = AIState.Idle;//AI状态

    public AIState state
    {
        get
        {
            return _state;
        }
        set
        {
            if (value == _state)
                return;

            Debug.Log($"#eid={eid}# <b><color=blue>[STATE]:{_state} -> {value}</color></b>");
            //离开这个状态
            switch (_state)
            {
                case AIState.Idle:
                    {
                        this.OnLeaveIdle();
                        view.OnLeaveIdle();
                    }
                    break;
                case AIState.Seek:
                    {
                        this.OnLeaveSeek();
                        view.OnLeaveSeek();
                    }
                    break;
                case AIState.Attack:
                    {
                        this.OnLeaveAttack();
                        view.OnLeaveAttack();
                    }
                    break;
                case AIState.Die:
                    {
                        this.OnLeaveDie();
                        view.OnLeaveDie();
                    }
                    break;
            }
            //进入新状态,做实体层的新状态进入的逻辑处理，并通知视图层新状态的画面表现
            switch (value)
            {
                case AIState.Idle:
                    {
                        this.OnEnterIdle();
                        view.OnEnterIdle();
                    }
                    break;
                case AIState.Seek:
                    {
                        this.OnEnterSeek();
                        view.OnEnterSeek();
                    }
                    break;
                case AIState.Attack:
                    {
                        this.OnEnterAttack();
                        view.OnEnterAttack();
                    }
                    break;
                case AIState.Die:
                    {
                        this.OnEnterDie();
                        view.OnEnterDie();
                    }
                    break;
            }
            _state = value;
        }
    }

    #region 记录攻击时间间隔（按逻辑真时间），用于动画表现和伤害计算(MyPlaceableMgr.Update中)
    public LFloat lastBlowTime = 0;//上次攻击时间
    public LFloat accDeltaTime = 0;//从上次攻击到现在的累积时间
    #endregion

    public  List<LVector3> path;

    #region 状态管理
    public virtual void OnEnterIdle()
    {
        //
        var e = rot.eulerAngles;
        e.x = e.z = 0;
        rot = LQuaternion.Euler(e);
        UpdateRotation();
    }
    public virtual void OnLeaveIdle()
    {

    }
    public virtual void OnEnterSeek()
    {
        
    }
    public virtual void OnLeaveSeek()
    {
        Debug.Log($"#eid={eid} OnLeaveSeek()");
    }
    public virtual void OnEnterAttack()
    {
        accDeltaTime = LFloat.zero;
    }
    public virtual void OnLeaveAttack()
    {

    }

    /// <summary>
    /// 判断是不是国王塔死亡了
    /// </summary>
    /// <returns></returns>
    private bool IsMeKingTower()
    {
        if (this == MyClient.placeableMgr.myTower || this == MyClient.placeableMgr.hisTower)
        {
            return true;
        }
        return false;
    }
    public virtual async void OnEnterDie()
    {
        //防止重复进入死亡状态
        if (state==AIState.Die)
        {
            return;
        }
        //停止移动
        this.path = null;
        //设置死亡
        this.hitPoints = 0;
        //如果进入死亡状态为国王塔
        if (IsMeKingTower())
        {
            viewBase.SendMessage("OnGameOver");
            OnGameOWin();
        }

        //延时销毁
        await MyEntity.Destroy(this, dieDuration);
    }

    private void OnGameOWin()
    {
        // 客户端离开房间
        Avatar.Player.LeaveRoom();

        //判定获胜方：自己死就是敌方赢
        var faction = this.faction == Faction.Red ? Faction.Blue : Faction.Red;

        //处理游戏结束
        Avatar.Player.DoGameOver(faction);
    }

    public virtual void OnLeaveDie()
    {
        
    }
    #endregion

    #region 重写的虚方法
    public override void OnDestroy()
    {
        MyClient.placeableMgr.mine.Remove(this);
        MyClient.placeableMgr.his.Remove(this);
    }
    #endregion

    #region 游戏单位的普通法发（造成伤害）
    /// <summary>
    /// 伤害计算
    /// </summary>
    public void OnDealDamage()
    {
        if (this .target==null )
        {
            return;
        }
        this.target.hitPoints -= this.damagePerAttack;
        if (this.target.hitPoints<0)
        {
            this.target.hitPoints = 0;
            this.target.state = AIState.Die;
        }
    }
    #endregion

    #region 发射子弹
    /// <summary>
    /// 发射子弹
    /// </summary>
    public async void OnFireProjectile()
    {
        if (this .target==null )
        {
            return;
        }
        //实例化一个火球
        var prefabName = this.faction == Faction.Red ? this.redProjPrefab : this.blueProjPrefab;

        var ent = await MyEntity.Instanciate<MyProjectile, MyView>(null, prefabName, MyClient.projMgr.viewBase.transform, xdata =>
           {
            //子弹统一放在角色左手边，表现层需要运用IK调整手部位置，匹配此子弹发射位置
            xdata.worldPos = this.worldPos + LVector3.up + this.rot * (LVector3.left / 2);
               xdata.rot = LQuaternion.LookRotation((this.target.worldPos - this.worldPos).normalized);

            //设置投掷物的释放者和被攻击者
            xdata.caster = this;
               xdata.target = this.target;
           });

        MyClient.projMgr.MineProjList.Add(ent);
    }
    #endregion

   
}

public partial class MyPlaceableModel
{
    /// <summary>
    /// 兵种ID，兵种数据的字典
    /// </summary>
    private System.Collections.Generic.Dictionary<int, MyPlaceable> byId;

    /// <summary>
    /// 按照ID取兵种数据（带缓存）
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public MyPlaceable this[int id]
    {
        get
        {
            if (byId == null)
            {
                byId = list.ToDictionary(x=>(int)x.id);//存进字典k为id
            }
            return byId[id];
        }
    }
}