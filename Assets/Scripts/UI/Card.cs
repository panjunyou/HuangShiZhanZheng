using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityRoyale
{
    public class Card : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        public UnityAction<int, Vector2> OnDragAction;
        public UnityAction<int> OnTapDownAction, OnTapReleaseAction;

        [HideInInspector] public int cardId;
        [HideInInspector] public CardData cardData;

        public Image portraitImage; //Inspector-set reference
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        //called by CardManager, it feeds CardData so this card can display the placeable's portrait
        public void InitialiseWithData(CardData cData)
        {
            cardData = cData;
            //portraitImage.sprite = cardData.cardImage;
        }

		/// <summary>
		/// 响应鼠标按下
		/// </summary>
		/// <param name="pointerEvent"></param>
        public void OnPointerDown(PointerEventData pointerEvent)
        {
			print("鼠标按下");

			// 实现鼠标按下动作
			OnTapDownAction?.Invoke(cardId);
			//if (OnTapDownAction != null)
			//{
			//	OnTapDownAction(cardId);
			//}
		}

        public void OnDrag(PointerEventData pointerEvent)
        {
			print("鼠标拖拽中");

			// 实现具体鼠标拖拽动作
			OnDragAction?.Invoke(cardId, pointerEvent.delta);
		}

        public void OnPointerUp(PointerEventData pointerEvent)
        {
			print("鼠标弹起");
			// 实现鼠标弹起动作
			OnTapReleaseAction?.Invoke(cardId);
		}

        public void ChangeActiveState(bool isActive)
        {
            canvasGroup.alpha = (isActive) ? .05f : 1f;
        }
    }
}