using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityRoyale;

/// <summary>
/// 游戏单位管理器
/// </summary>

public partial class MyPlaceable
{
    public Placeable.Faction faction=Placeable.Faction.None;
    public  float  lastBlowTime=0;
    public bool isUse=false;//是否已经可以使用的

    public MyPlaceable Clone()
    {
        return this.MemberwiseClone() as MyPlaceable;//浅拷贝
    }
}
public class MyPlaceableMgr : MonoBehaviour
{
    public static MyPlaceableMgr Instance;

    public List<MyPlaceableView> mine;

    public List<MyPlaceableView> his;

    public Transform trhistower,trMyTower;

    private void Awake()
    {
        Instance = this;

        mine = new List<MyPlaceableView>();

        his = new List<MyPlaceableView>();
       
    }

    private void Start() 
    {
        his.Add(trhistower.GetComponent<MyPlaceableView>());
        mine.Add(trMyTower.GetComponent<MyPlaceableView>());
    }
    private void Update()
    {
        //驱动游戏AI的更新
        UpdatePlaceable(mine);
        UpdatePlaceable(his);

       
    }

    private bool IsInAttackRange(Vector3 position1, Vector3 position2, float attackRange)
    {
        return Vector3.Distance(position1, position2) < attackRange;
    }

    private MyAIBaes FindNearestEnemy(Vector3 myPos, Placeable.Faction faction)
    {
        List<MyPlaceableView> units = (faction == Placeable.Faction.Player) ? his : mine;
        float x = float.MaxValue;
        MyAIBaes n = null;
        foreach (var unit in units)
        {
            var s= unit.GetComponent<MyAIBaes>();

            if (s.state!=AIState.Die)
            {
                var d = Vector3.Distance(unit.transform.position, myPos);
                if (d < x)
                {
                    x = d;
                    n = unit.GetComponent<MyAIBaes>();
                }
            }           
        }

        return n;
    }

    private void UpdatePlaceable(List<MyPlaceableView> pviews)
    {
        List<MyPlaceableView> DesPlaceableView = new List<MyPlaceableView>();

        for (int i = 0; i < pviews.Count; i++)
        {
            var p = pviews[i];
            var data = p.data;
            var ai = p.GetComponent<MyAIBaes>();
            var nav = ai.GetComponent<NavMeshAgent>();
            var Anim = ai.GetComponent<Animator>();

            if (data.isUse)
            {
                switch (ai.state)
                {
                    case AIState.Idle:
                        {
                            //往目标塔行进，检测是否有敌人在范围内，若是则转到Seek                      
                            ai.target = FindNearestEnemy(ai.transform.position, data.faction);//返回没有Die状态的敌人，找不到返回Null

                            if (ai.target == null) break;

                            //TODO 国王塔处理 
                            if (data.pType == Placeable.PlaceableType.Building)
                            {

                                if (IsInAttackRange(ai.transform.position, ai.target.transform.position, data.attackRange))
                                {
                                    ai.state = AIState.Attack;
                                }

                                break;
                            }
                            else if (data.pType == Placeable.PlaceableType.Unit)
                            {
                                //  print($"找到最近的角色{ai.target.gameObject.name}");
                                nav.enabled = true;
                                Anim.SetBool("IsMoving", true);
                                ai.state = AIState.Seek;
                            }

                        }
                        break;
                    case AIState.Seek:
                        {
                            //目标死亡就重新找目标     否则移动到目标点                           
                            if (ai.target.state == AIState.Die)
                            {
                                nav.enabled = false;
                                Anim.SetBool("IsMoving", false);
                                ai.state = AIState.Idle;
                                ai.target = null;
                                break;
                            }
                            else
                            {
                                nav.SetDestination(ai.target.transform.position);
                            }
                            //判断是否进入攻击范围                   
                            if (IsInAttackRange(ai.transform.position, ai.target.transform.position, p.data.attackRange))
                            {
                                nav.enabled = false;

                                Anim.SetBool("IsMoving", false);

                                //面向目标
                                var targetPos = ai.target.transform.position;
                                targetPos.y = ai.transform.position.y;
                                ai.transform.LookAt(targetPos);

                                ai.state = AIState.Attack;
                            }
                        }
                        break;
                    case AIState.Attack:
                        {
                            //被攻击者离开攻击范围
                            if (false == IsInAttackRange(ai.transform.position, ai.target.transform.position, data.attackRange))
                            {
                                if (data.pType == Placeable.PlaceableType.Unit)
                                {
                                    Anim.SetBool("IsMoving", true);
                                }
                                ai.target = null;
                                ai.state = AIState.Idle;
                                break;
                            }

                            //被攻击者死亡
                            if (ai.target.state == AIState.Die)
                            {
                                ai.target = null;

                                ai.state = AIState.Idle;

                                break;
                            }
                            //被攻击者hp值为0
                            if (ai.target.GetComponent<MyPlaceableView>().data.hitPoints <= 0)
                            {
                                OnEnterDie(ai.target);

                                ai.target = null;

                                ai.state = AIState.Idle;

                                break;
                            }

                            //攻击间隔
                            if (Time.time > data.lastBlowTime + data.attackRatio)
                            {
                                if (Anim != null && data.pType == Placeable.PlaceableType.Unit)
                                {
                                    Anim.SetTrigger("Attack");
                                }
                                else if (Anim != null && data.pType == Placeable.PlaceableType.Building)
                                {
                                    //国王塔攻击处理
                                    ((MyBuildingAI)ai).OnFireProjectile();
                                }
                                data.lastBlowTime = Time.time;
                            }
                        }
                        break;
                    case AIState.Die:
                        {
                            //溶解效果
                            var rds = p.GetComponentsInChildren<Renderer>();

                            p.DieProgress += Time.deltaTime * (1 / p.DieDuration);

                            foreach (var rd in rds)
                            {
                                rd.material.SetFloat("_DissolveFactor", p.DieProgress);
                            }

                            if (p.DieProgress >= 1f)
                            {
                                //加入死亡列表
                                if (!DesPlaceableView.Contains(p))
                                {
                                    DesPlaceableView.Add(p);
                                }
                            }

                        }
                        break;
                }
            }           
        }

        // 死亡列表处理
        while (DesPlaceableView.Count > 0)
        {
            var desplaceableView = DesPlaceableView[0];
            DesPlaceableView.Remove(desplaceableView);
            
            //TODO
            //不是通过Addressables.InstantiateAsync（）创建的不能使用Addressables.ReleaseInstance ，现在塔不是通过Addressables.InstantiateAsync（）创建
            if (desplaceableView.data.pType==Placeable.PlaceableType.Building)
            {
                pviews.Remove(desplaceableView);
                Destroy(desplaceableView.gameObject);
            }
            else
            {
                if (Addressables.ReleaseInstance(desplaceableView.gameObject))
                {
                    pviews.Remove(desplaceableView);
                }
                else
                {
                    Debug.LogError("Addressables.ReleaseInstance失败！");
                }             
            }
        }
    }

   private  void OnEnterDie(MyAIBaes target)
    {
        //被攻击者死亡
        if (target.state == AIState.Die)
        {
            return;
        }

        var target_View = target.GetComponent<MyPlaceableView>();

        target_View.data.hitPoints = 0;

        if (target_View.data.pType == Placeable.PlaceableType.Unit)//移动单位
        {
            var nav1 = target.GetComponent<NavMeshAgent>();

            nav1.enabled = false;

            var targetAnim = target.GetComponent<Animator>();

            targetAnim.SetTrigger("IsDead");
        }
        else if (target_View.data.pType == Placeable.PlaceableType.Building)//防御塔
        {
            var targetAnim = target.GetComponent<Animator>();

            targetAnim.SetTrigger("IsDead");
        }

       // print($"{target.gameObject.name} is dead!");

        //设置溶解边颜色
        var rds=target.GetComponentsInChildren<Renderer>();
        var color = target_View.data.faction == Placeable.Faction.Player ? Color.red : Color.green;
        target_View.DieProgress = 0;
        foreach (var rd in rds)
        {
            rd.material.SetColor("_EdgeColor", color*8);
            rd.material.SetFloat("_EdgeWidth",0.1f);
        }      

        target.state = AIState.Die;

        //显示GameOver画面
        if (trhistower==null|| trMyTower==null)
        {
            return;
        }
        if (target.gameObject.name.Equals(trhistower.name)|| target.gameObject.name.Equals(trMyTower.name))
        {

            var fac = target_View.data.faction == Placeable.Faction.Player ? Placeable.Faction.Opponent : Placeable.Faction.Player;
            //注册消息
            KBEngine.Event.fireOut("OnGameOver", fac);//正营为参数
            UIPage.ShowPageAsync<GameOverPage>(fac);//可以给新显示的页面穿参数
        }

    }

    
}
