using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

namespace UnityRoyale
{
    public class CardManager : MonoBehaviour
    {
        public Camera mainCamera; //public reference
        public LayerMask playingFieldMask;
        //public GameObject cardPrefab;
        public DeckData playersDeck;
		public MeshRenderer forbiddenAreaRenderer;
		
        public UnityAction<CardData, Vector3, Placeable.Faction> OnCardUsed;
        
        [Header("UI Elements")]
        public RectTransform backupCardTransform; //the smaller card that sits in the deck
        public RectTransform cardsDashboard; //the UI panel that contains the actual playable cards
        public RectTransform cardsPanel; //the UI panel that contains all cards, the deck, and the dashboard (center aligned)
        
        private Card[] cards;
        private bool cardIsActive = false; //when true, a card is being dragged over the play field
        private GameObject previewHolder;
        private Vector3 inputCreationOffset = new Vector3(0f, 0f, 1f); //offsets the creation of units so that they are not under the player's finger

        private void Awake()
        {
            previewHolder = new GameObject("PreviewHolder");
            cards = new Card[3]; //3 is the length of the dashboard
        }

        public void LoadDeck()
        {
            DeckLoader newDeckLoaderComp = gameObject.AddComponent<DeckLoader>();
            newDeckLoaderComp.OnDeckLoaded += DeckLoaded;
            newDeckLoaderComp.LoadDeck(playersDeck);
        }

        //...

		private void DeckLoaded()
		{
            Debug.Log("Player's deck loaded");

            //setup initial cards
            StartCoroutine(AddCardToDeck(.1f));
            for(int i=0; i<cards.Length; i++)
            {
                StartCoroutine(PromoteCardFromDeck(i, .4f + i));
                StartCoroutine(AddCardToDeck(.8f + i));
            }
		}

        //moves the preview card from the deck to the active card dashboard
        private IEnumerator PromoteCardFromDeck(int position, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);

            backupCardTransform.SetParent(cardsDashboard, true);
            //move and scale into position
            backupCardTransform.DOAnchorPos(new Vector2(210f * (position+1) + 20f, 0f),
                                            .2f + (.05f*position)).SetEase(Ease.OutQuad);
            backupCardTransform.localScale = Vector3.one;

            //store a reference to the Card component in the array
            Card cardScript = backupCardTransform.GetComponent<Card>();
            cardScript.cardId = position;
            cards[position] = cardScript;

            //setup listeners on Card events
            cardScript.OnTapDownAction += CardTapped;
            cardScript.OnDragAction += CardDragged;
            cardScript.OnTapReleaseAction += CardReleased;
        }

        //adds a new card to the deck on the left, ready to be used
        private IEnumerator AddCardToDeck(float delay = 0f) //TODO: pass in the CardData dynamically
        {
            yield return new WaitForSeconds(delay);

            //Get the next card data
            CardData data = playersDeck.GetNextCardFromDeck();

            //create new card
            backupCardTransform = Instantiate<GameObject>(data.cardPrefab, cardsPanel).GetComponent<RectTransform>();
            backupCardTransform.localScale = Vector3.one * 0.7f;
            
            //send it to the bottom left corner
            backupCardTransform.anchoredPosition = new Vector2(180f, -300f);
            backupCardTransform.DOAnchorPos(new Vector2(180f, 0f), .2f).SetEase(Ease.OutQuad);

            //populate CardData on the Card script
            Card cardScript = backupCardTransform.GetComponent<Card>();
            cardScript.InitialiseWithData(data);
        }

		/// <summary>
		/// 处理卡牌点击
		/// </summary>
		/// <param name="cardId"></param>
        private void CardTapped(int cardId)
        {
			// 按照cardId取到对应的卡牌数据
			// 把该张卡牌放到所有卡牌的所在的节点的最后一个
			// 使其在绘制时叠加在其他卡牌的上面
            cards[cardId].GetComponent<RectTransform>().SetAsLastSibling();
			forbiddenAreaRenderer.enabled = true;
        }

		/// <summary>
		/// 实现鼠标拖拽功能
		/// </summary>
		/// <param name="cardId">要拖拽的卡牌编号</param>
		/// <param name="dragAmount">拖拽的距离（卡组到鼠标当前位置的距离）</param>
		private void CardDragged(int cardId, Vector2 dragAmount)
        {
			// 移动卡牌到鼠标位置
            cards[cardId].transform.Translate(dragAmount);

            // 从鼠标位置发射一条射线
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
			// 判断该射线碰到场景什么位置
            bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask);

            if(planeHit) // 如果碰到场景物体
            {
                if(!cardIsActive) // 如果卡牌之前没有被拖拽出来（没有变成小兵）
                {
					print("hit plane & card is not active");
                    cardIsActive = true;
                    previewHolder.transform.position = hit.point;
                    cards[cardId].ChangeActiveState(true); // 隐藏该张卡牌

                    // 从卡牌数据数组找出该张卡牌的数据
                    PlaceableData[] dataToSpawn = cards[cardId].cardData.placeablesData; // 小兵数据
                    Vector3[] offsets = cards[cardId].cardData.relativeOffsets; // 小兵之间的相对偏移

					// 生成该卡牌对应的小兵数组，并且将其设置为预览用的卡牌（将其放置到一个统一的节点下（previewHolder）
					for (int i=0; i<dataToSpawn.Length; i++)
                    {
                        GameObject newPlaceable = GameObject.Instantiate<GameObject>(dataToSpawn[i].associatedPrefab,
                                                                                    hit.point + offsets[i] + inputCreationOffset,
                                                                                    Quaternion.identity,
                                                                                    previewHolder.transform);
                    }
                }
                else
                {
					print("hit plane & card is active");
					// 临时改变预览小兵的位置使其跟随鼠标移动
					previewHolder.transform.position = hit.point;
                }
            }
            else // 卡牌不在竞技区（在待选卡组区）
            {
                if(cardIsActive) // 如果卡牌曾经激活（曾经放到场景中了）
                {
					print("hit plane & card is deactive");
					cardIsActive = false; // 标记卡牌为未激活（未显示预览小兵）
                    cards[cardId].ChangeActiveState(false); //显示卡牌

                    ClearPreviewObjects(); // 销毁预览用的小兵
                }
            }
        }

        private void CardReleased(int cardId)
        {
            //raycasting to check if the card is on the play field
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, playingFieldMask))
            {
                if(OnCardUsed != null)
                    OnCardUsed(cards[cardId].cardData, hit.point + inputCreationOffset, Placeable.Faction.Player); // GameManager picks this up to spawn the actual Placeable

                ClearPreviewObjects();
                Destroy(cards[cardId].gameObject); // 删除打出去的卡牌

				StartCoroutine(PromoteCardFromDeck(cardId, .2f)); // 从手牌中取出一张
                StartCoroutine(AddCardToDeck(.6f)); 
            }
            else
            {
                cards[cardId].GetComponent<RectTransform>().DOAnchorPos(new Vector2(220f * (cardId+1), 0f),
                                                                        .2f).SetEase(Ease.OutQuad);
            }

			forbiddenAreaRenderer.enabled = false;
        }

        //happens when the card is put down on the playing field, and while dragging (when moving out of the play field)
        private void ClearPreviewObjects()
        {
            //destroy all the preview Placeables
            for(int i=0; i<previewHolder.transform.childCount; i++)
            {
                Destroy(previewHolder.transform.GetChild(i).gameObject);
            }
        }
    }

}
