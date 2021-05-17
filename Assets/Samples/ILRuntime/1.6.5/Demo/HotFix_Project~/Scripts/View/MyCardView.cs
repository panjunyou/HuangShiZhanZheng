using DG.Tweening;
using KBEngine;
using Lockstep.Math;
using System;
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

    private bool isDragging = false;//卡牌变小兵   

   // private Transform previewHolder;

    private Camera MainCam;

    private CanvasGroup CanvasGroupInst;

   // private string MyLayer = "RedField";

   // private List<MyPlaceableView> MyPviews;
    private List<MyPlaceable> previewList = new List<MyPlaceable>();

    private void Start()
    {
        MainCam = Camera.main;

       // previewHolder = GameObject.Find("PreviewHolder").transform;

        CanvasGroupInst = GetComponent<CanvasGroup>();

      //  MyPviews = new List<MyPlaceableView>();
    }

    

   
 

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Avatar.Player.gameState!=GameState.GAME_START)
        {
            return;
        }

        transform.SetAsLastSibling();//设为父亲的最后一个孩子

        // MyCardMgr.Instance.forbiddenAreaRenderer.enabled = true; 
        //将敌方区域渲染为禁止放区
        MyClient.cardMgr.OpponentFieldRenderer.enabled = true;
    }

    public async void OnDrag(PointerEventData eventData)
    {

        if (Avatar.Player.gameState != GameState.GAME_START)
        {
            return;
        }

        //屏幕坐标转世界坐标
        RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent as RectTransform,
            eventData.position, null, out Vector3 posWorld);

        transform.position = posWorld;

        //位置发射射线
        Ray ray = MainCam.ScreenPointToRay(eventData.position );

        //判断射线碰到是否场景
        bool hitGround = Physics.Raycast(ray,out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer(MyClient.cardMgr.MyLayer));

        //如果碰到场景
        if (hitGround)
        {
            // previewHolder.position = hit.point;
            LVector3 hitPosint = hit.point.ToLVevtor3_My();

            UpdatePreviewList(hitPosint);//更新预览小兵位置

            if (isDragging==false)
            {

                isDragging = true;              

                print("命中地面，可以变兵");               

                CanvasGroupInst.alpha = 0;

                previewList.Clear();
                //在创建过程中会分一条线程，同帧内，如果有另一条进行销毁同一个物品就会销毁失败！！
                //  MyPviews = await CreatePlacable(true,data, previewHolder, previewHolder.position,Placeable.Faction.Red );

                await MyCardMgr.CreatePlacable(true,
                    data,
                    MyClient.placeableMgr.viewBase.transform,
                    hitPosint,
                    Avatar.Player.MyFaction,
                    previewList
                    );               
                                                             
            }
        }
        else
        {
            print("没有命中地面");

            if (isDragging)
            {
                isDragging = false;
                if (CanvasGroupInst.alpha != 1)
                {
                    CanvasGroupInst.alpha = 1f;
                }                  
            }

            DestroyPreviewList();
        }

    } 
    
    public async void OnPointerUp(PointerEventData eventData)
    {
        if (Avatar.Player.gameState != GameState.GAME_START)
        {
            return;
        }    

        //位置发射射线
        Ray ray = MainCam.ScreenPointToRay(eventData.position);

        //判断射线碰到是否场景
        bool hitGround = Physics.Raycast(ray,out RaycastHit hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer(MyClient.cardMgr.MyLayer));

        //如果碰到场景
        if (hitGround)
        {
            ////MyPviews.Count是否加载完成
            //if (isDragging==true&& MyPviews.Count == data.placeablesIndices.Length)
            //{
            //    //播放音响
            //    // previewHolder.GetChild(0).GetComponent<AudioSource>().Play();

            //    //把小兵放到MyPlaceableMgr.Instance.mine里，和加载下一张卡
            //    await SetPlayersToMgr();
            //}
            KBEngine.Event.fireIn("PlaceCard", new CMD()
            {
                cardId = (int)data.id,
                pos = new INT_VECTOR2()
                {
                    x = (int)(hit.point.x*LFloat.Precision),//发送位置乘1000，接收除1000
                    z = (int)(hit.point.z * LFloat.Precision)
                },

            });

            //销毁预览小兵
            DestroyPreviewList();
            //销毁打出去的牌
            Addressables.ReleaseInstance(gameObject);

            //把牌放到出牌区
            //  MyCardMgr.Instance. StartCoroutine(MyCardMgr.Instance.预览区到出牌区(index, 0.1f));
            await MyClient.cardMgr.预览区到出牌区(index, 0.1f);

            //在创建一张牌放到预览区                
            //   MyCardMgr.Instance.StartCoroutine(MyCardMgr.Instance.创建卡牌到预览区(0.2f));
            await MyClient.cardMgr.创建卡牌到预览区(0.2f);

        }
        else
        {
            //卡牌放回
            transform.DOMove(MyClient.cardMgr.cards[index].position, 0.3f);
        }

        MyClient.cardMgr.OpponentFieldRenderer.enabled = false;
    }

    public void UpdatePreviewList(LVector3 posCenter)
    {
        for (int i = 0; i < previewList .Count ; i++)
        {
            var p = previewList[i];
            p.parentPos = posCenter;
            p.UpdateWorldPos();
        }
    }

    private  void DestroyPreviewList()
    {
        //
        foreach (var p in previewList)
        {
             MyEntity.Destroy(p);

        }
        previewList.Clear();
    }

    
}
