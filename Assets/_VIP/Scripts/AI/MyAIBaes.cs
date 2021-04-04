using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRoyale;

public enum AIState
{
    Idle,
    Seek,
    Attack,
    Die,
}

public class MyAIBaes : MonoBehaviour
{
    public MyAIBaes target = null;//攻击目标

    public GameObject ProfInst;//投掷物
    public Transform FirePos;//投掷位置

    public AIState state = AIState.Idle;

    public virtual void OnIdle() { }
    public virtual void OnSeek() { }
    public virtual void OnAttack() { }
    public virtual void OnDie() { }

    //Animation->Evemts=战士
    public void OnDealDamage()
    {
        if (this.target == null) return;

        if (this.target.GetComponent<MyPlaceableView>().data.hitPoints > 0)
        {
            this.target.GetComponent<MyPlaceableView>().data.hitPoints -= GetComponent<MyPlaceableView>().data.damagePerAttack;
        }
    }

    //Animation->Evemts=法师,射手
    public void OnFireProjectile()
    {
        var go = Instantiate(ProfInst, FirePos.position, Quaternion.identity, MyProjectileMgr.Instance.transform);
        var MyProjectileInst = go.GetComponent<MyProjectile>();
        MyProjectileInst.caster = this;
        MyProjectileInst.target = this.target;

        if (target != null)
        {
            go.transform.forward = target.gameObject.transform.position + Vector3.up;

            var MyView= GetComponent<MyPlaceableView>().data;

            if (MyView.faction==Placeable.Faction.Player)
            {
                MyProjectileMgr.Instance.MineProjList.Add(MyProjectileInst);
            }
            else
            {
                MyProjectileMgr.Instance.HisProjList.Add(MyProjectileInst);
            }
        }
        else
        {
            Destroy(go);
        }
    }
}
