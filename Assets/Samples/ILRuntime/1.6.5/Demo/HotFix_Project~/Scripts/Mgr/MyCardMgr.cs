using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityRoyale;
using KBEngine;
using Lockstep.Math;
using System.Threading;

public class MyCardMgr : MyEntity
{
   // public static MyCardMgr Instance;
   [HideInInspector]
    public Transform[] cards; // 活动牌位置

    // public GameObject[] cardPrefabs; // 卡牌预制体（弓箭手/战士/法师/。。。）
    [HideInInspector]
    public Transform myCardMgr; // 创建出来的卡牌必须放在Canvas下，否则显示不出来
    [HideInInspector]
    public Transform startPos, endPos; // 发牌动画的起始位置和终止位置

    [HideInInspector]
    public  Transform previewCard; // 预览卡牌 
   
    public MeshRenderer blueFieldRenderer, redFieldRenderer;

    private string RedLayer = "RedField";
    private string BlueLayer = "BlueField";

    ////禁止区域
    private string BlueField = "BlueField";
    private string RedField = "RedField";
   

    public MeshRenderer OpponentFieldRenderer {//禁止区域显示网格
        get
        {
            return Avatar.Player.HisFaction == Placeable.Faction.Red ? redFieldRenderer : blueFieldRenderer;
        }
    }

    public string MyLayer
    {
        get
        {
            return Avatar.Player.MyFaction == Placeable.Faction.Red ? RedLayer : BlueLayer;
        }
    }

    private Lockstep.Math.Random rnd;//生成卡牌的随机数生成器

    public override  async Task OnAwake()
    {
       // Instance = this;

//#if UNITY_EDITOR
//        if (Avatar.Player ==null )
//        {
//            SceneManager.LoadScene("Start");
//            return;
//        }
//#endif
        Debug.Log("#Sequence# registerOut(OnGameReady)");
        KBEngine.Event.registerOut("OnGameReady",this,"OnGameReady");
              
        //资源预加载，防止卡顿
        IList<GameObject> goList = await Addressables.LoadAssetsAsync<GameObject>(new string[] { "BattleUnit" }, null, Addressables.MergeMode.None).Task;
        Debug.Log($"#Sequence# 预加载兵种数量：{goList.Count}");

        

    }

    public override  void OnDestroy()
    {
        //卸载
        Resources.UnloadUnusedAssets();
    }

    public async void OnGameReady(uint seed,byte frameRate,int idx)
    {

        //短线重连自动补帧进入游戏极快，执行到本方法时，客户端场景可能为加载完成，需要等待
        do
        {
            var scene = SceneManager.GetSceneByName("Battle");
            Debug.Log($"#Sequence# MyCardmMgr.OnGameReady() scene={scene}");
            if (scene!=null&&scene.isLoaded)
            {
                break;
            }
            await new WaitForEndOfFrame();
        } while (true);

        //旋转摄像机
        if (Avatar.Player.MyFaction==Placeable.Faction.Blue)
        {
            Camera.main.transform.RotateAround(Vector3.zero,Vector3.up,180);
            var newPos = Camera.main.transform.position;
            newPos.z = Mathf.Abs(newPos.z);

        }

        ////禁止区域赋值
        MyClient.cardMgr.blueFieldRenderer = GameObject.Find(BlueField).GetComponent<MeshRenderer>();
        MyClient.cardMgr.redFieldRenderer = GameObject.Find(RedField).GetComponent<MeshRenderer>();

        //初始化随机数（根据服务器）
        Debug.Log($"#Sequence# MyCardMgr:创建随机数，Seed={Avatar.Player.seed}");
        rnd = new Lockstep.Math.Random(Avatar.Player.seed);

        //一个逻辑真模拟的剩余时间(假设逻辑帧200ms/帧，当前渲染帧如果超过200ms，则触发一逻辑帧的逻辑运算)（会随渲染帧不停的减，比如渲染帧用了6ms-》 200-6=194 当为0就触发一次逻辑帧）
        remainingTime = Avatar.Player.fixedDeltaTime;

        Debug.Log("#Sequence# MyClinet.Init() finished");

        Debug.Log("#Sequence# UIPage.ShowPageAsync<DeckPage>()");
        //加载出牌UI
        UIPage.ShowPageAsync<DeckPage>(async () => {

           

            await 创建卡牌到预览区(0.5f);
            await 预览区到出牌区(0, 0.5f);

            await 创建卡牌到预览区(0.5f);
            await 预览区到出牌区(1, 0.5f);

            await 创建卡牌到预览区(0.5f);
            await 预览区到出牌区(2, 0.5f);

            await 创建卡牌到预览区(0.5f);
            Debug.Log("#Sequence# 发牌完毕！");            
        });
    }

    //async Task Start()
    //{
    //    //资源预加载，防止卡顿
    //    IList<GameObject> goList = await Addressables.LoadAssetsAsync<GameObject>(new string[] { "BattleUnit" }, null, Addressables.MergeMode.None).Task;
    //    Debug.Log($"#Sequence# 预加载兵种数量：{goList.Count}");
       
    //}
    /// <summary>
    /// 创建小兵
    /// </summary>
    /// <param name="isPreview"></param>
    /// <param name="cardData"></param>
    /// <param name="parent"></param>
    /// <param name="posCenter"></param>
    /// <param name="faction"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static async Task CreatePlacable(bool isPreview, MyCard cardData, Transform parent, LVector3 posCenter, Placeable.Faction faction,List<MyPlaceable> list)
    {
      //  List<MyPlaceableView> views = new List<MyPlaceableView>();

        if (!isPreview)
        {
            Debug.Log($"#FDATA# CreatePlacable(cardData={cardData},posCenter={posCenter},parentName={parent.name},faction={faction})");
        }

        for (int i = 0; i < cardData.placeablesIndices.Length; i++)
        {
            //取小兵ID
            var unitId = cardData.placeablesIndices[i];//10000

            MyPlaceable p = MyPlaceableModel.instance[unitId];

            LVector3 offset = cardData.relativeOffsets[i];

            string prefabName = faction == Placeable.Faction.Red ? p.associatedPrefab : p.alternatePrefab;

            var ent = await MyEntity.Instanciate<MyPlaceable, MyPlaceableView>(p, prefabName, parent, xdata =>
              {
                  xdata.faction = faction;
                  xdata.isPreview = isPreview;
                  if (isPreview)
                  {
                    //预览
                    xdata.parentPos = posCenter;
                      xdata.localPos = offset;
                  }
                  else
                  {
                    //非预览
                    xdata.parentPos = LVector3.zero;
                      xdata.localPos = posCenter + offset;
                  }

                  if (xdata.faction == Placeable.Faction.Blue)
                  {
                      xdata.rot = LQuaternion.LookRotation(LVector3.back);
                  }
                  else
                  {
                      xdata.rot = LQuaternion.LookRotation(LVector3.forward);
                  }
              }, isPreview);
            list.Add(ent);            
        }
      //  return views;
    }

    public  async void Update()
    {
        //帧同步的更新开始
       
        if (Avatar.Player.gameState == GameState.GAME_START && MyClient.isInit)
        {
            await ConsumeFrame2();
        }
    }

    public LFloat remainingTime;//用来计算这一渲染帧是否达到执行逻辑帧的条件

    /// <summary>
    /// 所以单位的渲染层更新和逻辑层的更新
    /// </summary>
    /// <returns></returns>
    public async Task ConsumeFrame2()
    {
        //取得服务器生成的没有被消费的帧列表
        var fs = Avatar.Player.frames;

        //追帧
        int fastForwardFrames = 5;//最大（客户端有超过5帧服务器发来的帧每没有被消费，超过这个值就会开始追帧）
        int stopForwardFrames = 1;//停止追帧

        //判断是否为追帧状态（默认为否）
        if (Avatar.Player .isFastForward)
        {
            //如果追到fs的帧小于stopForwardFrames帧，就是没有帧眼追了
            if (fs.Count<stopForwardFrames)
            {
                Avatar.Player.isFastForward = false;
            }
        }
        else
        {
            //如果延迟大于fastForwardFrames就开始追帧
            if (fs.Count>fastForwardFrames)
            {
                Avatar.Player.isFastForward = true;
            }
        }

        #region Mian Loop
        /*
         * 必须要取走数据包，在执行OnFrame帧逻辑
         * 因为FixedUpadte会重入，不取走会造成一个数据包被反复执行
         * 安全起见，取走数据包之前必须先克隆一份帧数据
         * 用克隆的数据来做帧处理
         */
        LFloat deltaTime = 0;//渲染帧时间（上一渲染帧到当前帧的时间，如每秒60帧，就是16ms）
        if (fs.Count>0)//是否有逻辑帧要处理
        {
            #region DeltaTime相关
            //如果是追帧状态：deltaTime直接设置为Avatar.Player.fixedDeltaTime;相当于每一个渲染帧都执行逻辑帧，这样就能快速追帧
            if (Avatar .Player .isFastForward)
            {
                //追帧状态                
                deltaTime = Avatar.Player.fixedDeltaTime;
                Debug.Log($"#Sequence# <b><color=yellow>FastForward,FrameCount={fs.Count}</color></b>");
            }
            else
            {
                //不追帧状态
                //deltaTime直接设置为渲染帧的模拟时间（16ms）
                //一帧模拟的时间不要超过fixedDeltaTime（保证每一逻辑帧都处理）
                deltaTime = LMath.Min(Time.deltaTime.ToLFloat(),Avatar.Player .fixedDeltaTime);
            }
            #endregion
            //remainingTime初始值为Avatar.Player .fixedDeltaTime
            //一个逻辑真模拟的剩余时间(假设逻辑帧200ms/帧，当前渲染帧如果超过200ms，则触发一逻辑帧的逻辑运算)（会随渲染帧不停的减，比如渲染帧用了6ms-》 200-6=194 当为0就触发一次逻辑帧）
            if (remainingTime-deltaTime<=0)
            {
                //开始执行逻辑帧

                FRAME_SYNC frame = null;
                {
                    frame = fs[0].DeepClone();
                    fs.RemoveAt(0);
                }
                //处理单帧模拟（固定帧间隔）
                await OnFrame2(Avatar.Player.consumedFrameCount, frame);
                //逻辑的更新（状态，位置等等）
                MyClient.placeableMgr.OnLogicUpdate(Avatar.Player.fixedDeltaTime);
                MyClient.projMgr.OnLogicUpdate(Avatar.Player.fixedDeltaTime);

                Avatar.Player.consumedFrameCount++;
                //这时remainingTime必定小于或等于零，比如remainingTime= -100  那么就逻辑帧时间（200ms）当前渲染帧时间（50ms）（-100-50+200=150ms），
                //下一次逻辑帧就会从remainingTime=150ms算，而不是Avatar.Player.fixedDeltaTime的值（200ms）
                remainingTime = remainingTime - deltaTime + Avatar.Player.fixedDeltaTime;

                Debug.Log($"#Sequence# remains={remainingTime}");
            }
            else
            {
                //不满足逻辑帧处理，（比如逻辑帧时间为200ms，渲染帧为50ms，就200-50=150ms，下一渲染帧为90ms 150-90=60ms，直到小于零才执行逻辑帧）
                remainingTime -= deltaTime;
            }
        }
        #endregion

        //表现层的渲染
        Avatar.Player.clntAccFrameTime += Time.deltaTime.ToLFloat();//累加客户端从开始同步时的总共时间（秒）
        LFloat lerpT = LFloat.one - remainingTime / Avatar.Player.fixedDeltaTime;
        //视图的更新（插值等等）
        MyClient.placeableMgr.OnViewUpdate(lerpT);
        MyClient.projMgr.OnVeiwUpdate(lerpT);
    }

    private async Task OnFrame2(int frameId, FRAME_SYNC frame)
    {
        Debug.Log($"#FDATA# OnFrame({frameId},{frame.ToStringEx()})");

        foreach (var cmd in frame.cmds)//所有玩家的命令
        {
            if (cmd.cardId==-1)
            {
                continue;
            }
            //
            var avatar = KBEngineApp.app.findEntity(cmd.pid) as Avatar;
            //
            MyCard cardData = MyCardModel.instance.FindById(cmd.cardId);
            //
            await MyCardMgr.CreatePlacable(
                false,
                cardData,
                MyClient.placeableMgr.viewBase.transform,
                new LVector3(true, cmd.pos.x, 0, cmd.pos.z),                
                cmd.pid == KBEngineApp.app.player().id ? Avatar.Player.MyFaction : Avatar.Player.HisFaction,
                cmd.pid == KBEngineApp.app.player().id ? MyClient.placeableMgr.mine :
                MyClient.placeableMgr.his
                );
        }
        MyClient.placeableMgr.DebugOutputPlaceables();
        Debug.Log($"#FDATA# OnFrame END");
    }

    public  void OnGUI()
    {
        if (Avatar.Player ==null||Avatar.Player.gameState !=GameState.GAME_START)
        {
            return;
        }
        GUILayout.Label($"cftime:{Avatar.Player.clntAccFrameTime},SFID:{Avatar.Player.frameId},FW={Avatar.Player.isFastForward}");

        int sleepFrame = 0;
        if (GUILayout.Button("延时3帧"))
        {
            sleepFrame = 3;
        }
        if (GUILayout.Button("延时10帧"))
        {
            sleepFrame = 10;
        }
        if (GUILayout.Button("延时30帧"))
        {
            sleepFrame = 30;
        }

        if (sleepFrame>0)
        {
            Debug.Log($"#Sequence# <b>准备延时{sleepFrame}帧</b>" );
            Thread.Sleep((int)(Avatar.Player.fixedDeltaTime.ToFloat()*sleepFrame*1000));
            Debug.Log($"#Sequence# <b>结束延时{sleepFrame}帧</b>");
        }
    }

    //用（async）声明为异步方法， 使用（await new）返回值必须为 Task（任务） 
    public async Task  创建卡牌到预览区(float 延迟值)
    {
        await new WaitForSeconds(延迟值);//这里会创建一个Task,在await时C#会返回Task对象（任务） 

        int iCard = UnityEngine.Random.Range(0, MyCardModel.instance.unitCards.Count);
        MyCard card = MyCardModel.instance.unitCards[iCard];

        //GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);
        ////GameObject cardPrefab = cardPrefabs[Random.Range(0, cardPrefabs.Length)];
        //previewCard = Instantiate(cardPrefab).transform;

        //异步等待
        GameObject cardPrefab=await Addressables.InstantiateAsync(card.cardPrefab).Task;//异步等待实例化完成
        previewCard = cardPrefab.transform;

        previewCard.SetParent(myCardMgr, false); // 位于父节点下的（0，0，0）偏移处
        previewCard.localScale = Vector3.one * 0.7f;
        previewCard.position = startPos.position;
        previewCard.DOMove(endPos.position, 0.1f);

        previewCard.GetComponent<MyCardView>().data= card;
    }

    public  async Task 预览区到出牌区(int i, float 延迟值)
    {
        await new WaitForSeconds(延迟值);

        previewCard.localScale = Vector3.one;
        previewCard.DOMove(cards[i].position, 0.3f);

        previewCard.GetComponent<MyCardView>().index=i;
    }
}
