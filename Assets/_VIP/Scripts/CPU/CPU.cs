using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityRoyale;

public class CPU : MonoBehaviour
{
    public float interval = 5;//出牌间隔

    public Transform PosW;

    private bool isGameOver = false;

    async void Start()
    {
        KBEngine.Event.registerOut("OnGameOver", this, "OnGameOver");

        await CardOut();
    }

    //必须为public方法
    public void OnGameOver(object s)
    {
        Debug.Log(s);
        isGameOver = true;
    }


    async Task CardOut()
    {
        //
        while (true)
        {
            if (isGameOver) break;
            await new WaitForSeconds(interval);
            var list = MyCardModel.instance.list;
            var card = list[Random.Range(0, list.Count)];
            var pos = PosW.position  + new Vector3(Random.Range(PosW.localScale.x * 0.5f*(-1),
                PosW.localScale.x * 0.5f),0f, Random.Range(PosW.localScale.z * 0.5f * (-1), PosW.localScale.z * 0.5f ));

            if (isGameOver) break;
            var MyPviews =await MyCardView.CreatePlacable(card,MyPlaceableMgr.Instance.transform, pos,Placeable.Faction.Opponent);

            //开启使用权
            foreach (var p in MyPviews)
            {
                p.data.isUse = true;
            }

            for (int i = 0; i < MyPviews.Count; i++)
            {
                MyPlaceableMgr.Instance.his.Add(MyPviews[i]);
            }
        }       
    }


    private void OnDrawGizmos()
    {
        Color color = Color.red;
        Gizmos.color = color;
        Gizmos.DrawWireCube(PosW.position, PosW.localScale);
    }
}
