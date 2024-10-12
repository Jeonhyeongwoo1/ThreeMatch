using System;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ThreeMatch.InGame.Presenter
{
    public class InGameItemPresenter : BasePresenter
    {
        private InGameItemView _inGameItemView;
        private InGameItemModel _inGameItemModel;
        private Action<InGameItemType> _onItemUsagePendingAction;
        
        public void Initialize(InGameItemView itemView)
        {
            _inGameItemModel = ModelFactory.CreateOrGet<InGameItemModel>();
            _inGameItemView = itemView;

            _inGameItemView.onPendingUseInGameItem += OnUseInGameItem;
            GameManager.onUsedInGameItemAction += OnUsedInGameItem;
        }

        private void OnUsedInGameItem(InGameItemType inGameItemType)
        {
            AddInGameItemData(inGameItemType, -1);
        }

        private bool IsPossibleUseItem(InGameItemType inGameItemType)
        {
            int itemCount = _inGameItemModel.GetInGameItemValue(inGameItemType);
            return itemCount > 0;
        }

        private void OnUseInGameItem(InGameItemType inGameItemType)
        {
            bool isPossibleUseItem = IsPossibleUseItem(inGameItemType);
            if (!isPossibleUseItem)
            {
                return;
            }
            
            GameManager.onInGameItemUsagePendingAction?.Invoke(inGameItemType);
        }

        public void AddInGameItemData(InGameItemType inGameItemType, int value)
        {
            int result = _inGameItemModel.AddOrRemoveInGameItemValue(inGameItemType, value);
            _inGameItemView.UpdateInGameItemAmount(inGameItemType, result.ToString());
        }
    }
}