using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityRoyale
{
	// 游戏出牌数据
    [CreateAssetMenu(fileName = "NewDeck", menuName = "Unity Royale/Deck Data")]
    public class DeckData : ScriptableObject
    {
        public AssetLabelReference[] labelsToInclude; //set by designers

        private CardData[] cards; // 实际要打的牌，我们需要对这些数据进行洗牌
        private int currentCard = 0; // 当前打到第几张牌了

		/// <summary>
		/// 把卡牌拷贝（保存）到我们自己定义的卡牌数组里面
		/// </summary>
		/// <param name="cardDataDownloaded"></param>
        public void CardsRetrieved(List<CardData> cardDataDownloaded)
        {
            //load the actual cards data into an array, ready to use
            int totalCards = cardDataDownloaded.Count;
            cards = new CardData[totalCards];
            for(int c=0; c<totalCards; c++)
            {
                cards[c] = cardDataDownloaded[c];
            }
        }

		/// <summary>
		/// 洗牌，但是这里还没有洗牌，通过CardsRetrieved下载的牌是什么样，我们就用什么样的牌
		/// </summary>
		public void ShuffleCards()
        {
            //TODO: shuffle cards
        }

		//returns the next card in the deck. You probably want to shuffle cards first
		// 从未打的牌中取一张牌出来（最好首先洗一下牌）
		public CardData GetNextCardFromDeck()
        {
            //advance the index
            currentCard++;
            if(currentCard >= cards.Length)
                currentCard = 0;

            return cards[currentCard];
        }
    }
}
