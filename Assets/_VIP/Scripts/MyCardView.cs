using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityRoyale;

public class MyCardView : MonoBehaviour,IDragHandler,IPointerUpHandler,IPointerDownHandler
{
    public MyCard data;

    public int index; //出牌区序号

    private bool isDragging = false;//卡牌变小兵中。。。

    private bool isDrag = false;//鼠标是否在拖动

    private Transform previewHolder;

    private Camera MainCam;

    private CanvasGroup CanvasGroupInst;

    private List<MyPlaceableView> MyPviews;

    private void Start()
    {
        MainCam = Camera.main;

        previewHolder = GameObject.Find("PreviewHolder").transform;

        CanvasGroupInst = GetComponent<CanvasGroup>();

        MyPviews = new List<MyPlaceableView>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.SetAsLastSibling();//设为父亲的最后一个孩子

        MyCardMgr.Instance.forbiddenAreaRenderer.enabled = true; 
    }

    public async void OnDrag(PointerEventData eventData)
    {
        
        //屏幕坐标转世界坐标
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform,
            eventData.position, null, out Vector3 posWorld);

        transform.position = posWorld;

        //位置发射射线
        Ray ray = MainCam.ScreenPointToRay(eventData.position );

        //判断射线碰到是否场景
        bool hitGround = Physics.Raycast(ray,out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

        //如果碰到场景
        if (hitGround)
        {            
            previewHolder.position = hit.point;

            if (isDragging==false)
            {

                isDragging = true;              

                print("命中地面，可以变兵");               

                isDrag = true;

                CanvasGroupInst.alpha = 0;

                //在创建过程中会分一条线程，同帧内，如果有另一条进行销毁同一个物品就会销毁失败！！
                MyPviews = await CreatePlacable(data, previewHolder, previewHolder.position,Placeable.Faction.Player );

                //开启使用权
                foreach (var p in MyPviews)
                {
                    p.data.isUse = true;
                }
                                                             
                //因为等带过程中鼠标改变， 所以从新设置小兵位置               
                for (int i = 0; i < data.placeablesIndices.Length; i++)
                {
                    MyPviews[i].transform.localPosition = data.relativeOffsets[i];
                }

                //如果在等待加载过程中鼠标放开
                if (isDrag == false)
                {
                    await SetPlayersToMgr();
                }
            }
        }
        else
        {
            print("没有命中地面");

            if (isDragging)
            {
                if (CanvasGroupInst.alpha != 1)
                {
                    CanvasGroupInst.alpha = 1;
                }                  
            }

            if (MyPviews.Count== data.placeablesIndices.Length)
            {
                StartCoroutine("下一帧执行");
            }
        }

    } 
    
    IEnumerator 下一帧执行()
    {
        yield return null;
        while (MyPviews.Count > 0)
        {
            var p = MyPviews[0];
            MyPviews.Remove(p);

            if (!Addressables.ReleaseInstance(p.gameObject))
            {
                Debug.LogError("d");
            }
        }
           isDragging = false;
    }

    public async void OnPointerUp(PointerEventData eventData)
    {
        isDrag = false ;

        MyCardMgr.Instance.forbiddenAreaRenderer.enabled = false;

        //位置发射射线
        Ray ray = MainCam.ScreenPointToRay(eventData.position);

        //判断射线碰到是否场景
        bool hitGround = Physics.Raycast(ray, float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

        //如果碰到场景
        if (hitGround)
        {
            //MyPviews.Count是否加载完成
            if (isDragging==true&& MyPviews.Count == data.placeablesIndices.Length)
            {
                //播放音响
                // previewHolder.GetChild(0).GetComponent<AudioSource>().Play();

                //把小兵放到MyPlaceableMgr.Instance.mine里，和加载下一张卡
                await SetPlayersToMgr();
            }
        }
        else
        {
            //卡牌放回
            transform.DOMove(MyCardMgr.Instance.cards[index].position, 0.3f);
        }
    }
   
    //
    private async Task SetPlayersToMgr()
    {
        //把小兵放到MyPlaceableMgr.Instance.mine里
        OnCardUsed(MyPviews);

        //把牌放到出牌区
        //  MyCardMgr.Instance. StartCoroutine(MyCardMgr.Instance.预览区到出牌区(index, 0.1f));
        await MyCardMgr.Instance.预览区到出牌区(index, 0.1f);

        //在创建一张牌放到预览区                
        //   MyCardMgr.Instance.StartCoroutine(MyCardMgr.Instance.创建卡牌到预览区(0.2f));
        await MyCardMgr.Instance.创建卡牌到预览区(0.2f);

        // Destroy(gameObject);
        Addressables.ReleaseInstance(gameObject);
    }

    private void OnCardUsed(List<MyPlaceableView> views)
    {
        while (views.Count > 0)
        {
            var vv = views[0];
            views.Remove(vv);
            vv.gameObject.transform.SetParent(MyPlaceableMgr.Instance.transform, true);
            MyPlaceableMgr.Instance.mine.Add(vv);
        }  
    }

    public  static async Task< List<MyPlaceableView>>  CreatePlacable(MyCard data1,Transform parent, Vector3 Pos, Placeable.Faction faction)
    {
        List<MyPlaceableView> views = new List<MyPlaceableView>();
        for (int i = 0; i < data1.placeablesIndices.Length; i++)
        {
            //取小兵ID
            var cardID = data1.placeablesIndices[i];

            MyPlaceable placeable = null;
            for (int j = 0; j < MyPlaceableModel.instance.list.Count; j++)
            {
                //根据小兵ID找小兵
                if (MyPlaceableModel.instance.list[j].id == cardID)
                {                   
                    placeable = MyPlaceableModel.instance.list[j];
                    break;
                }
            }
            //取偏移
            Vector3 offset = data1.relativeOffsets[i];

            //实列小兵
            //GameObject unitPrefad= Resources.Load<GameObject>((faction == Placeable.Faction.Player)? placeable.associatedPrefab: placeable.alternatePrefab);          

            //var placeableInst = Instantiate(unitPrefad, parent, false);

            var ps = (faction == Placeable.Faction.Player) ? placeable.associatedPrefab : placeable.alternatePrefab;
            //异步实列小兵
            GameObject placeableInst = await Addressables.InstantiateAsync(ps, parent,false).Task;

            //设置小兵偏移
            placeableInst.transform.localPosition = offset;
            placeableInst.transform.position = Pos + offset;
            //赋值小兵数据
            var p2 = placeable.Clone();
            p2.faction = faction;
            placeableInst.GetComponent<MyPlaceableView>().data = p2;

            if (faction==Placeable.Faction.Opponent)
            {
                placeableInst.transform.Rotate(new Vector3(0,180,0));
            }

            views.Add(placeableInst.GetComponent<MyPlaceableView>());
        }

        return views;
    }
}
