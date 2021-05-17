using Lockstep.Math;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityRoyale;

public class MyProjectileMgr : MyEntity
{
    // public static MyProjectileMgr Instance;
    public List<MyProjectile> red = new List<MyProjectile>();
    public List<MyProjectile> blue = new List<MyProjectile>();

    public List<MyProjectile> MineProjList
    {
        get
        {
            if (Avatar.Player .MyFaction==Placeable.Faction.Red)
            {
                return red;
            }
            else
            {
                return blue;
            }
        }
    }

    public List<MyProjectile> HisProjList
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

    public override Task OnAwake()
    {
        KBEngine.Event.registerOut("OnGameReady",this,"OnGameReady");

        return Task.CompletedTask;//Task.CompletedTask表示任务完成
    }

    public void OnLogicUpdate(LFloat dt)
    {
        UpdateProjectiles(red,dt);
        UpdateProjectiles(blue, dt);
    }   
    public void OnVeiwUpdate(LFloat lerpT)
    {
        UpdateProjectileView(red, lerpT);
        UpdateProjectileView(blue, lerpT);
    }
    private void UpdateProjectileView(List<MyProjectile> ps,LFloat lerpT)
    {
        for (int i = 0; i < ps.Count; i++)
        {
            MyProjectile data = ps[i];
            MyView view = data.viewBase;

            view.OnViewUpdate(lerpT.ToFloat());
        }
    }
    private void UpdateProjectiles(List<MyProjectile> projList,LFloat dt)
    {
        List<MyProjectile> DesProjectiles = new List<MyProjectile>();

        for (int i = 0; i < projList.Count; i++)
        {
            var proj = projList[i];
            MyPlaceable casterAI = proj.caster;
            MyPlaceable targetAI = proj.target;

            proj.progress += dt * proj.Speed;

            Debug.Assert(proj.caster != null);

            if (proj.target==null)
            {
                DesProjectiles.Add(proj);
                continue;
            }
            //计算子弹的飞行位置：技能释放则位置+当前飞行距离
            //总飞行距离=终点-起点+飞行高度
            //当前飞行距离=总飞行距离*飞行进度
            LVector3 deltaPos = targetAI.worldPos + LVector3.up - casterAI.worldPos;
            proj.worldPos = casterAI.worldPos + deltaPos * proj.progress;

            if (proj.progress>=1f)
            {
                casterAI.OnDealDamage();

                if (targetAI.hitPoints<=0)
                {
                    proj.target.state = AIState.Die;
                }

                DesProjectiles.Add(proj);
            }
        }
        //投掷物销毁处理
        while (DesProjectiles.Count > 0)
        {
            var p = DesProjectiles[0];
            DesProjectiles.Remove(p);
            MyEntity.Destroy(p);
        }
    }

    public  void OnGameReady(uint sees, byte frameRate, int idx)
    {
        OnRestart();
    }

    public override void OnDestroy()
    {
        MyClient.projMgr = null;
    }

    public override void OnRestart()
    {
        for (int i = this.red.Count-1; i >= 0; i--)
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
