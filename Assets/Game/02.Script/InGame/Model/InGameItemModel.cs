using System.Collections.Generic;
using System.Linq;
using ThreeMatch.Firebase.Data;
using ThreeMatch.InGame.Interface;
using UniRx;
using UnityEngine;

namespace ThreeMatch.InGame.Model
{
    public class InGameItemModel : IModel
    {
        public readonly Dictionary<InGameItemType, ReactiveProperty<int>> inGameItemDict = new();

        public int GetInGameItemValue(InGameItemType inGameItemType)
        {
            if (inGameItemDict.TryGetValue(inGameItemType, out ReactiveProperty<int> value))
            {
                return value.Value;
            }

            Debug.LogError($"failed get item amount type : {inGameItemType}");
            return 0;
        }

        public int AddOrRemoveInGameItemValue(InGameItemType inGameItemType, int amount)
        {
            if (inGameItemDict.TryGetValue(inGameItemType, out ReactiveProperty<int> value))
            {
                value.Value += amount;
                return value.Value;
            }

            inGameItemDict[inGameItemType] = new ReactiveProperty<int>(amount);
            // Debug.LogError($"failed add or remove item amount type : {inGameItemType}  amount {amount}");
            return amount;
        }

        public void SetInGameItemValue(InGameItemType inGameItemType, int amount)
        {
            if (!inGameItemDict.TryGetValue(inGameItemType, out ReactiveProperty<int> value))
            {
                value = new ReactiveProperty<int>();
                value.Value = amount;
                inGameItemDict.Add(inGameItemType, value);
                return;
            }

            inGameItemDict[inGameItemType].Value = amount;
        }

        public List<InGameItemData> ConvertToInGameItemDataList()
        {
            var list = new List<InGameItemData>(inGameItemDict.Count);
            list.AddRange(inGameItemDict.Select(itemData => new InGameItemData()
                { ItemId = (int)itemData.Key, ItemCount = itemData.Value.Value }));

            return list;
        }
    }
}