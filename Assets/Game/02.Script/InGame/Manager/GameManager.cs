using System;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.InGame.UI;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    Initialized();
                }

                return _instance;
            }
        }
        
        public static Action<InGameItemType> onInGameItemUsagePendingAction;
        public static Action<InGameItemType> onUsedInGameItemAction;
        
        private static GameManager _instance;

        private void Awake()
        {
            Initialized();
            UpdateUserInGameItem();
        }

        private static void Initialized()
        {
            if (_instance)
            {
                return;
            }
            
            UIManager uiManager = UIManager.Instance;

            var inGameItemPresenter = PresenterFactory.CreateOrGet<InGameItemPresenter>();
            inGameItemPresenter.Initialize(uiManager.CreateOrGet<InGameItemView>());
        }

        public void UpdateUserInGameItem()
        {
            var inGameItemPresenter = PresenterFactory.CreateOrGet<InGameItemPresenter>();
            inGameItemPresenter.AddInGameItemData(InGameItemType.Hammer, 10);
            inGameItemPresenter.AddInGameItemData(InGameItemType.Shuffle, 10);
            inGameItemPresenter.AddInGameItemData(InGameItemType.VerticalRocket, 10);
            inGameItemPresenter.AddInGameItemData(InGameItemType.HorizontalRocket, 10);
        }
    }
}