using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProjectileMgr : MonoBehaviour
{
    public static MyProjectileMgr Instance;

    public List<MyProjectile> MineProjList;//

    public List<MyProjectile> HisProjList;//
    private void Awake()
    {
        Instance = this;

        MineProjList = new List<MyProjectile>();
        HisProjList = new List<MyProjectile>();
    }

    private void Update()
    {
        //子弹AI
        UpdateProjectiles(MineProjList);
        UpdateProjectiles(HisProjList);
    }

    private void UpdateProjectiles(List<MyProjectile> myProjectiles)
    {
        List<MyProjectile> DesProjectiles = new List<MyProjectile>();

        for (int i = 0; i < myProjectiles.Count; i++)
        {
            var projInst = myProjectiles[i];

            projInst.progress += Time.deltaTime * projInst.Speed;

            if (projInst.caster != null && projInst.target != null && projInst.caster.state == AIState.Attack)
            {
                projInst.transform.position = Vector3.Lerp(projInst.caster.FirePos.position, projInst.target.transform.position + Vector3.up, projInst.progress);
            }
            else
            {
                DesProjectiles.Add(projInst);
            }

            if (projInst.progress >= 1f)
            {

                projInst.caster.OnDealDamage();

                DesProjectiles.Add(projInst);

            }
        }

        while (DesProjectiles.Count > 0)
        {
            var proj = DesProjectiles[0];
            DesProjectiles.Remove(proj);
            myProjectiles.Remove(proj);
            Destroy(proj.gameObject);
        }
    }
}
