using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityRoyale
{
    public class DeckLoader : MonoBehaviour
    {
        private DeckData targetDeck;
        public UnityAction OnDeckLoaded;

        public void LoadDeck(DeckData deckToLoad)
        {
            targetDeck = deckToLoad;
			// 加载指定标签的资源（标签类似于AB的Label Variant）
			// 可以用Label加载比如说：只发给AI的牌，只发给玩家的牌，等等
			Addressables.LoadAssetsAsync<CardData>(targetDeck.labelsToInclude[0].labelString, null).Completed += OnResourcesRetrieved;
        }

        //...

		private void OnResourcesRetrieved(AsyncOperationHandle<IList<CardData>> obj)
		{
			targetDeck.CardsRetrieved((List<CardData>)obj.Result);

            if(OnDeckLoaded != null)
                OnDeckLoaded();

            Destroy(this);
		}
	}
}