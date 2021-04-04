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

    private bool isDragging = false;//卡牌是否变小兵

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

                CanvasGroupInst.alpha = 0;

                MyPviews =await CreatePlacable(data, previewHolder, previewHolder.position,Placeable.Faction.Player );

                //因为等带过程中鼠标改变， 所以从新设置小兵位置               
                for (int i = 0; i < data.placeablesIndices.Length; i++)
                {
                    MyPviews[i].transform.localPosition = data.relativeOffsets[i];
                }

            }
        }
        else
        {
            print("没有命中地面");
            if (isDragging == true)
            {
                isDragging = false;
                CanvasGroupInst.alpha = 1;

                foreach (Transform placeable in previewHolder)
                {
                    Destroy(placeable.gameObject );
                }
            }
        }

    } 
    
    public async void OnPointerUp(PointerEventData eventData)
    {
        MyCardMgr.Instance.forbiddenAreaRenderer.enabled = false;

        //位置发射射线
        Ray ray = MainCam.ScreenPointToRay(eventData.position);

        //判断射线碰到是否场景
        bool hitGround = Physics.Raycast(ray, float.PositiveInfinity, 1 << LayerMask.NameToLayer("PlayingField"));

        //如果碰到场景
        if (hitGround)
        {
            if (isDragging==true)
            {
                //播放音响
               // previewHolder.GetChild(0).GetComponent<AudioSource>().Play();

                OnCardUsed(MyPviews);

                //把牌放到出牌区
                //  MyCardMgr.Instance. StartCoroutine(MyCardMgr.Instance.预览区到出牌区(index, 0.1f));
                await MyCardMgr.Instance.预览区到出牌区(index, 0.1f);

                //在创建一张牌放到预览区                
                //   MyCardMgr.Instance.StartCoroutine(MyCardMgr.Instance.创建卡牌到预览区(0.2f));
                await MyCardMgr.Instance.创建卡牌到预览区(0.2f);

                Destroy(gameObject);
            }
        }
        else
        {
            //卡牌放回
            transform.DOMove(MyCardMgr.Instance.cards[index].position, 0.3f);
        }
    }

    private void OnCardUsed(List<MyPlaceableView> views)
    {
        for (int i = 0; i < views.Count; i++)
        {
            views[i].gameObject.transform.SetParent(MyPlaceableMgr.Instance.transform, true);
            MyPlaceableMgr.Instance.mine.Add(views[i]);
        }

        //for (int i = previewHolder.childCount-1; i >=0 ; i--)
        //{
        //    Transform placeable = previewHolder.GetChild(i);
        //    placeable.SetParent(MyPlaceableMgr.Instance.transform, true);
        //    MyPlaceableMgr.Instance.mine.Add(placeable.GetComponent<MyPlaceableView>());
        //}
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

            views.Add(placeableInst.GetComponent<MyPlaceableView>());
        }

        return views;
    }
}
