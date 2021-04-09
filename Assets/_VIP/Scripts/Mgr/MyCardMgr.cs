using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyCardMgr : MonoBehaviour
{
    public static MyCardMgr Instance;

    public Transform[] cards; // 活动牌位置

   // public GameObject[] cardPrefabs; // 卡牌预制体（弓箭手/战士/法师/。。。）

    public Transform canvas; // 创建出来的卡牌必须放在Canvas下，否则显示不出来

    public Transform startPos, endPos; // 发牌动画的起始位置和终止位置

    [HideInInspector]
    public  Transform previewCard; // 预览卡牌 

    public MeshRenderer forbiddenAreaRenderer;//禁止区域显示网格

    private void Awake()
    {
        Instance = this;
    }

     void  Start()
    {
        //StartCoroutine(创建卡牌到预览区(0.5f));
        //StartCoroutine(预览区到出牌区(0, 0.7f));

        //StartCoroutine(创建卡牌到预览区(1f));
        //StartCoroutine(预览区到出牌区(1, 1.2f));

        //StartCoroutine(创建卡牌到预览区(1.5f));
        //StartCoroutine(预览区到出牌区(2, 1.7f));

        //StartCoroutine(创建卡牌到预览区(2f));

        //await 创建卡牌到预览区(0.5f);
        //await 预览区到出牌区(0, 0.7f);

        //await 创建卡牌到预览区(1f);
        //await 预览区到出牌区(1, 1.2f);

        //await 创建卡牌到预览区(1.5f);
        //await 预览区到出牌区(2, 1.7f);

        //await 创建卡牌到预览区(2f);

        //加载出牌UI
        UIPage.ShowPageAsync<DeckPage>( async ()=> {           

            await 创建卡牌到预览区(0.5f);
            await 预览区到出牌区(0, 0.5f);

            await 创建卡牌到预览区(0.5f);
            await 预览区到出牌区(1, 0.5f);

            await 创建卡牌到预览区(0.5f);
            await 预览区到出牌区(2, 0.5f);

            await 创建卡牌到预览区(0.5f);
        });       
    }

    //用（async）声明为异步方法， 使用（await new）返回值必须为 Task（任务） 
    public async Task  创建卡牌到预览区(float 延迟值)
    {
        await new WaitForSeconds(延迟值);//这里会创建一个Task,在await时C#会返回Task对象（任务） 

        int iCard = Random.Range(0, MyCardModel.instance.list.Count);
        MyCard card = MyCardModel.instance.list[iCard];

        //GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);
        ////GameObject cardPrefab = cardPrefabs[Random.Range(0, cardPrefabs.Length)];
        //previewCard = Instantiate(cardPrefab).transform;

        //异步等待
        GameObject cardPrefab=await Addressables.InstantiateAsync(card.cardPrefab).Task;//异步等待实例化完成
        previewCard = cardPrefab.transform;

        previewCard.SetParent(canvas, false); // 位于父节点下的（0，0，0）偏移处
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
