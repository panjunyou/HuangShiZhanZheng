using Lockstep.Math;
using System.Threading.Tasks;
using UnityEngine;
using Random = Lockstep.Math.Random;

public class CPU : MyEntity
{
    public float interval = 5;//出牌间隔

    //public Transform PosW;
    private LVector3[] range = new LVector3[2]
    {
        new Vector3(-5,0,2).ToLVevtor3_My(),
        new Vector3(5,0,8).ToLVevtor3_My(),
    };

    private bool isGameOver = false;

    private Random rnd;

    public override Task OnAwake()
    {
//#if UNITY_EDITOR
//        if (Avatar.Player == null)
//        {
//            return Task.CompletedTask;
//        }
//#endif

        KBEngine.Event.registerOut("OnGameOver",this , "OnGameOver");

        KBEngine.Event.registerOut("OnGameReady", this, "OnGameReady");

        return Task.CompletedTask;
    }

    public void OnGameReady(uint seed,byte frameRate,int idx)
    {
        Debug.Log($"#Sequence# CPU:创建随机数，Seed={Avatar.Player.seed}");
        rnd = new Random(Avatar.Player.seed);
        isGameOver = false;
        CardOut();
    }
    //async void Start()
    //{
    //    KBEngine.Event.registerOut("OnGameOver", this, "OnGameOver");

    //    await CardOut();
    //}

    //必须为public方法
    public void OnGameOver(object s)
    {
        Debug.Log(s);
        isGameOver = true;
    }


    async void CardOut()
    {
        if (range[0].x>range[1].x||range[0].z>range[1].z)
        {
            Debug.LogError("range[0] must be less than range[1]");
            return;
        }
        //
        while (true)
        {
            await new WaitForSeconds(interval);

            var cardList = MyCardModel.instance.unitCards;
            var cardData = cardList[rnd.Next(cardList.Count)];

            if (isGameOver)
            {
                break;
            }

            await MyCardMgr.CreatePlacable(
                false,
                cardData,
                MyClient.placeableMgr.viewBase.transform,
                new LVector3(rnd.Next(range[1].x - range[0].x) + range[0].x, 0, rnd.Next(range[1].z - range[0].z) + range[0].z),                               
                Avatar.Player.HisFaction,
                MyClient.placeableMgr.his
                );

            if (isGameOver)
            {
                break;
            }

           // await new WaitForSeconds(interval);
        }       
    }


    //private void OnDrawGizmos()
    //{
    //    Color color = Color.red;
    //    Gizmos.color = color;
    //    Gizmos.DrawWireCube(PosW.position, PosW.localScale);
    //}

    public override void OnDestroy()
    {
        MyClient.cpu = null;
    }

    public override void OnRestart()
    {
        
    }
}
