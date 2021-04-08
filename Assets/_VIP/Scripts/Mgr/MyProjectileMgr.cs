using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityRoyale;

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
        UpdateProjectiles();
  
    }

    private void UpdateProjectiles()
    {
        List<MyProjectile> DesProjectiles = new List<MyProjectile>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var projInst1 = transform.GetChild(i).gameObject;

            var projInst = projInst1.GetComponent<MyProjectile>();
          
            if (projInst.isUse)
            {
                projInst.progress += Time.deltaTime * projInst.Speed;

                if (projInst.caster != null && projInst.target != null && projInst.caster.state == AIState.Attack)
                {
                    projInst.transform.position = Vector3.Lerp(projInst.caster.FirePos.position, projInst.target.transform.position + Vector3.up, projInst.progress);
                    projInst.transform.forward = projInst.target.gameObject.transform.position + Vector3.up;
                }
                else
                {
                    if (!DesProjectiles.Contains(projInst))
                    {
                        DesProjectiles.Add(projInst);
 
                    }
                }

                if (projInst.progress >= 1f)
                {

                    projInst.caster.OnDealDamage();

                    if (!DesProjectiles.Contains(projInst))
                    {
                         DesProjectiles.Add(projInst);
                  
                    }

                }
            }            
        }
        //投掷物销毁处理
        while (DesProjectiles.Count > 0)
        {
            var p = DesProjectiles[0];
            DesProjectiles.Remove(p);
            MineProjList.Remove(p);
            HisProjList.Remove(p);
            Addressables.ReleaseInstance(p.gameObject);
        }
    }

     
}
