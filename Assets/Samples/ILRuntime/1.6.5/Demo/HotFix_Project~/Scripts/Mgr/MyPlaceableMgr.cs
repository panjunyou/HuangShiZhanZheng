using Lockstep.Math;
using Lockstep.PathFinding;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityRoyale;
using static UnityRoyale.Placeable;

/// <summary>
/// 游戏单位管理器
/// </summary>


public class MyPlaceableMgr : MyEntity
{
   // public static MyPlaceableMgr Instance;

    private List<MyPlaceable> red = new List<MyPlaceable>();
    private List<MyPlaceable> blue = new List<MyPlaceable>();

    public  List<MyPlaceable> this[Faction f]
    {
        get
        {
            if (f==Faction.Red)
            {
                return red;
            }
            else
            {
                return blue;
            }
        }
    }
    public List<MyPlaceable> mine
    {
        get
        {
            if (Avatar.Player.MyFaction==Placeable.Faction.Red)
            {
                return red;
            }
            else
            {
                return blue;
            }
        }
    }

    public List<MyPlaceable> his
    {
        get
        {
            if (Avatar.Player.MyFaction == Placeable.Faction.Red)
            {
                return blue;
            }
            else
            {
                return red;
            }
        }
    }
     
    public MyPlaceable hisTower,myTower;//敌方、本方国王塔
    public MyPlaceable myTowerGuard, hisTowerGuard;//守卫

    private TriangleNavMesh navMesh;

    public async void OnGameReady(uint sees,byte frameRate,int idx)
    {
        Debug.Log("#Sequence# MyPlaceableMgr.OnGameReady()");

        OnRestart();//每一局游戏开始前都要清空场景非静态物体，否则会有上一局的物体存在

        var tmp = new List<MyPlaceable>();

        //红防御塔守卫
        tmp.Clear();
        await MyCardMgr.CreatePlacable(
            false,
            MyCardModel.instance.FindById(20003),
            this.viewBase.transform,
            MyClient.sceneInfo.GetObjPos("Battle", "MyPlaceablePlaceholder/redTower/myTowerGuard"),
            Faction.Red,
            tmp
            );

        if (Avatar.Player.MyFaction == Faction.Red)
        {
            myTowerGuard = tmp[0];
            mine.Add(myTowerGuard);
        }
        else
        {
            myTowerGuard = tmp[0];
            his.Add(myTowerGuard);
        }
        //红防御塔
        tmp.Clear();
        await MyCardMgr.CreatePlacable(
            false,
            MyCardModel.instance.FindById(20004),
            this.viewBase.transform,
            MyClient.sceneInfo.GetObjPos("Battle", "MyPlaceablePlaceholder/redTower"),
            Faction.Red,
            tmp
            );

        if (Avatar.Player.MyFaction==Faction.Red)
        {
            myTower = tmp[0];
            mine.Add(myTower);
        }
        else
        {
            hisTower = tmp[0];
            his.Add(hisTower);
        }

        //蓝防御塔守卫
        tmp.Clear();
        await MyCardMgr.CreatePlacable(
            false,
            MyCardModel.instance.FindById(20003),
            this.viewBase.transform,
            MyClient.sceneInfo.GetObjPos("Battle", "MyPlaceablePlaceholder/blueTower/hitTowerGuard"),
            Faction.Blue,
            tmp
            );

        if (Avatar.Player.MyFaction == Faction.Blue)
        {
            hisTowerGuard = tmp[0];
            mine.Add(hisTowerGuard);
        }
        else
        {
            hisTowerGuard = tmp[0];
            his.Add(hisTowerGuard);
        }
        //蓝防御塔
        tmp.Clear();
        await MyCardMgr.CreatePlacable(
            false,
            MyCardModel.instance.FindById(20004),
            this.viewBase.transform,
            MyClient.sceneInfo.GetObjPos("Battle", "MyPlaceablePlaceholder/blueTower"),
            Faction.Blue,
            tmp
            );

        if (Avatar.Player.MyFaction == Faction.Blue)
        {
            myTower = tmp[0];
            mine.Add(myTower);
        }
        else
        {
            hisTower = tmp[0];
            his.Add(hisTower);
        }
    }

    public override async Task OnAwake()
    {
        KBEngine.Event.registerOut("OnGameReady" ,this, "OnGameReady");
        var navData = await Addressables.LoadAssetAsync<TextAsset>("0.navmesh").Task;
        navMesh = new TriangleNavMesh(navData.text);
    }

    public void OnLogicUpdate(LFloat dt)//每一逻辑帧调用
    {
        //游戏逻辑AI更新(注意必须保证每一个客户端更新的先后都一样，比如每一个客户端先更新红方小兵)
        UpdatePlaceable(red,dt);
        UpdatePlaceable(blue, dt);
    }
    public void OnViewUpdate(LFloat lerpT)//每一渲染辑帧调用
    {
        UpdatePlaceableView(red, lerpT);
        UpdatePlaceableView(blue, lerpT);
    }
    private void UpdatePlaceableView(List<MyPlaceable> ents,LFloat lerpT)
    {
        for (int i = 0; i < ents.Count; i++)
        {
            MyPlaceable data = ents[i];
            MyView view = data.viewBase;

            view.OnViewUpdate(lerpT.ToFloat());
        }
    }
    

    //private void Awake()
    //{
    //    Instance = this;      
       
    //}

    

    private bool IsInAttackRange(LVector3 myPos, LVector3 targetPos, LFloat attackRange)
    {
        return (myPos-targetPos).magnitude<attackRange;
    }

    private MyPlaceable FindNearestEnemy(MyPlaceable unit)
    {
        LFloat x = LFloat.MaxValue;
        MyPlaceable nearest = null;
        foreach (var enemy in this[unit.OppnentFaction])
        {
            if (enemy.isAttackable==false)
            {
                continue;
            }
            LFloat d = (enemy.worldPos - unit.worldPos).magnitude;
            if (d<x&&enemy.hitPoints>0)
            {
                x = d;
                nearest = enemy;
            }
        }
        return nearest;
    }

    private void UpdatePlaceable(List<MyPlaceable> ps,LFloat dt)
    {
       // List<MyPlaceableView> DesPlaceableView = new List<MyPlaceableView>();


        for (int i = 0; i < ps.Count; i++)
        {
            //var p = pviews[i];
            //var data = p.data;
            //var ai = p.GetComponent<MyAIBaes>();
            //var nav = ai.GetComponent<NavMeshAgent>();
            //var Anim = ai.GetComponent<Animator>();
            MyPlaceable data = ps[i];
            MyPlaceableView view = data.view;

            if (data.isPreview)
            {
                return;
            }
            //状态机转换
            switch (data.state)
            {
                case AIState.Idle:
                    {
                        if (data.targetType==PlaceableTarget.None)
                        {
                            //国王塔
                            break;
                        }
                        //往目标塔行进，检测是否有敌人在范围内，若是则转到Seek                      
                        data.target = FindNearestEnemy(data);//返回没有Die状态的敌人，找不到返回Null

                        if (data.target!=null)
                        {
                            TrianglePointPath tmpPath = new TrianglePointPath();
                            //得到走向敌人的路径
                            data.path = navMesh.FindPath(data.worldPos, data.target.worldPos, tmpPath);

                            if (data .path!=null )
                            {
                                Debug.Log($"#eid={data.eid}# {data.ename} FindPath([{data.worldPos}] -> {data.path.ToStringEx()})");
                                for (int j = 0; j < data.path.Count; j++)
                                {
                                    Debug.Log($"#eid={data.eid}# {data.ename} data.path[{j}]-> {data.path[j] }");
                                }
                                
                                if (data .pType==PlaceableType.Unit)
                                {
                                    //可以移动攻击（小兵）
                                    data.state = AIState.Seek;
                                }
                                else
                                {
                                    //不可以移动（国王塔守卫）
                                    data.state = AIState.Attack;
                                }
                            }
                        }
                        

                    }
                    break;
                case AIState.Seek:
                    {
                        //追击过程中敌人死亡
                        if (data .target==null)
                        {
                            data.state = AIState.Idle;
                            break;
                        }
                        
                        LVector3 dir = LVector3.zero;
                        while (true)
                        {
                            if (data .path==null)
                            {
                                data.state = AIState.Idle;
                                break;
                            }

                            dir = data.path[0] - data.worldPos;//得到方向
                            var dist = dir.magnitude;////当前位置到data.path[0]的距离
                            dir.Normalize();//转为向量
                                                       
                            //var dist2 = ((data.worldPos + dir * data.speed * dt) - data.worldPos ).magnitude;//模拟当前一帧移动的距离

                            //TODO 有两种情况：一种是到点了，一种是一帧的移动超过点了,超过点就认为是到点了                            
                            if (dist>2f.ToLFloat())
                            {
                                //当前位置和目标位置大于0.5，说明这段路没走完所有 break;
                                break;
                            }

                           
                            //走完这段路了
                            data.path.RemoveAt(0);
                            if (data.path.Count==0)
                            {
                                data.path = null;
                            }
                            else
                            {
                                Debug.Log($"#{data.name}# [{data.path.Count}] Move to:({data.path[0]})");
                            }
                        }
                        #region 计算每一步的世界坐标
                        //可能角色移动速度太快，一个逻辑帧就超过了data.path[0]这个点，因为被超过了角色就回头走向这个点，就会死循环，防止角色回头就直接移除这个点
                        //当前坐标到data.path[0]点的距离是不是小于一帧行走的距离
                       // ExceedPath(data, dir, dt);

                        //设置worldPos的值，worldPos在渲染帧做插值
                        data.worldPos += dir * data.speed * dt;  //dt相当于Time.deltaTime  的作用（因为这是逻辑帧更新所以用逻辑帧模拟时间）
                        data.rot = LQuaternion.LookRotation(dir);                       
                        #endregion

                        //是否进入攻击范围
                        if (IsInAttackRange(data.worldPos,data.target.worldPos,data.attackRange))
                        {
                            Debug.Log($"#Sequence# IsInAttackRange({data.worldPos},{data.target.worldPos},{data.attackRange})");
                            //停止移动
                            data.path = null;

                            data.state = AIState.Attack;
                        }
                    }
                    break;
                case AIState.Attack:
                    {
                        if (data.target==null)
                        {
                            data.state = AIState.Idle;
                            break;
                        }

                        if (false==IsInAttackRange(data.worldPos, data.target.worldPos, data.attackRange))
                        {
                            data.state = AIState.Idle;
                            break;
                        }

                        //如果在攻击间隔内，则不攻击
                        data.accDeltaTime += dt;
                        if (data.accDeltaTime<data.attackRatio)
                        {
                            break;
                        }

                        //面向目标
                        LVector3 dir = (data.target.worldPos - data.worldPos).normalized;
                        data.rot = LQuaternion.LookRotation(dir);

                        //实行攻击
                        if (data .attackType==ThinkingPlaceable.AttackType.Melee)
                        {
                            data.OnDealDamage();
                        }
                        else
                        {
                            data.OnFireProjectile();
                        }
                        //执行攻击动作
                        view.SendMessage("OnPlayAttackAnim");

                        //攻击伤害结算
                        if (data .target.hitPoints<=0)
                        {
                             data.target.state = AIState.Die;
                            data.state = AIState.Idle;
                        }

                        //设置上一次攻击的时间为当前时间
                        data.lastBlowTime = Avatar.Player.clntFrameTime;
                        data.accDeltaTime = LFloat.zero;
                    }
                    break;
                case AIState.Die:
                    {
                        if (data.targetType==PlaceableTarget.None)
                        {
                            //
                            break;
                        }

                    }
                    break;
            }
        }
      
    }    
    /// <summary>
    /// 防止超过定点
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dir"></param>
    /// <param name="dt"></param>
    private  void ExceedPath(MyPlaceable data,LVector3 dir,LFloat dt)
    {       
        if ((data.worldPos - data.path[0]).magnitude< (data.worldPos - (data.worldPos + dir * data.speed * dt)).magnitude)
        {
            Debug.Log($"#{data.name}# {data.path[0]} 被超过了");
            data.path.RemoveAt(0);
            ExceedPath(data,dir,dt);
        }
    }
    public  void DebugOutputPlaceables()
    {
        Debug.Log("#FDATA# DebugOutputPlaceables--->>>");
        _DebugOutputPlaceables(red);
        _DebugOutputPlaceables(blue);
        Debug.Log("#FDATA# DebugOutputPlaceables<<<---");
    }
    public void _DebugOutputPlaceables(List<MyPlaceable> ps)
    {
        for (int i = 0; i < ps.Count; i++)
        {
            MyPlaceable data = ps[i];
            MyPlaceableView view = data.view;

            if (data.isPreview)
            {
                return;
            }
            Debug.Log($"#FDATA# eid={data.eid } cfid={Avatar.Player.consumedFrameCount} lerpT={data.lerpT} hp={view.dataBase.As<MyPlaceable>().hitPoints:F2} @[{data.name} @{data.worldPos}]");
        }
    }

    public override void OnDestroy()
    {
        MyClient.placeableMgr = null;
    }

    public override void OnRestart()
    {
        for (int i = this.red.Count - 1; i >= 0; i--)
        {
            var unit = this.red[i];
            MyEntity.Destroy(unit);
        }
        for (int i = this.blue.Count - 1; i >= 0; i--)
        {
            var unit = this.blue[i];
            MyEntity.Destroy(unit);
        }
    }
}
