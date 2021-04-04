using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRoyale;

public class 卡牌管理器 : MonoBehaviour
{
	public GameObject cardPrefab; // 卡牌预制体

	public Transform[] cards = new Transform[4]; // 四张活动卡牌

	public Transform startPos, endPos; // 预览卡牌的起始和终止位置

	private Transform previewCard; // 预览卡牌对象

	public Transform canvas; // 动态创建的卡牌要放在canvas下

    // Start is called before the first frame update
    void Start()
    {
		canvas.gameObject.SetActive(true);

		StartCoroutine(卡牌创建到预览区(.1f)); // 0.1

		for (int i = 0; i < cards.Length; i++)
		{
			StartCoroutine(从预览区发牌到活动区(i, .4f + i)); // 0.4, 1.4, 2.4, 3.4
			StartCoroutine(卡牌创建到预览区(.8f+i)); // 0.8, 1.8, 2.8, 3.8
		}
	}

    IEnumerator 卡牌创建到预览区(float delay)
	{
		// 延时创建卡牌
		yield return new WaitForSeconds(delay);
		print($"AddToDeck");

		// 随机创建一张牌
		int iCard = Random.Range(0, MyCardModel.instance.list.Count);
		MyCard card = MyCardModel.instance.list[iCard];
		GameObject cardPrefab = Resources.Load<GameObject>(card.cardPrefab);

		previewCard = Instantiate(cardPrefab, canvas).transform;

		// 为这张牌设置卡牌数据
		previewCard.GetComponent<MyCardComponent>().data = card;

		previewCard.position = startPos.position;
		previewCard.localScale = Vector3.one * 0.5f;
		previewCard.DOMove(endPos.position, .2f);
	}

	IEnumerator 从预览区发牌到活动区(int i, float delay)
	{
		// 延时发牌
		yield return new WaitForSeconds(delay);
		print($"PromoteFromDeck");

		previewCard.localScale = Vector3.one;
		previewCard.transform.DOMove(cards[i].position, .2f + 0.05f * i); // .2, .25, .3, .35
	}
}
