using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using UniRx;
using UnityEngine;

namespace ThreeMatch.InGame.Model
{
    public class InGameItemModel : IModel
    {
        private readonly Dictionary<InGameItemType, ReactiveProperty<int>> _inGameItemDict = new();

        public int GetInGameItemValue(InGameItemType inGameItemType)
        {
            if (_inGameItemDict.TryGetValue(inGameItemType, out ReactiveProperty<int> value))
            {
                return value.Value;
            }

            Debug.LogError($"failed get item amount type : {inGameItemType}");
            return 0;
        }

        public int AddOrRemoveInGameItemValue(InGameItemType inGameItemType, int amount)
        {
            if (_inGameItemDict.TryGetValue(inGameItemType, out ReactiveProperty<int> value))
            {
                value.Value += amount;
                return value.Value;
            }

            _inGameItemDict[inGameItemType] = new ReactiveProperty<int>(amount);
            // Debug.LogError($"failed add or remove item amount type : {inGameItemType}  amount {amount}");
            return amount;
        }
    }
}