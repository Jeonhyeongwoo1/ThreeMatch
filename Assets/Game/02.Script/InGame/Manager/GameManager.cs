using System;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Model;
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
        public static Action onAllSuccessMissionAction;
        public static Action onGameReadyAction;
        public static Action onGameStartAction;
        public static Action onGameClearAction;
        public static Action onGameOverAction;
        public static Action<int> onChangeRemainingMoveCountAction;
        
        private static GameManager _instance;

        private GameState _gameState;

        private void Awake()
        {
            Initialized();
            UpdateUserInGameItem();
        }

        private void OnEnable()
        {
            onGameReadyAction += OnGameReady;
            onGameStartAction += OnGameStart; 
            onGameOverAction += OnGameOver;
            onGameClearAction += OnGameClear;
        }

        private void OnDestroy()
        {
            onGameReadyAction -= OnGameReady;
            onGameStartAction -= OnGameStart;
            onGameOverAction -= OnGameOver;
            onGameClearAction -= OnGameClear;
        }

        private void OnGameStart()
        {
            UpdateState(GameState.Start);
        }

        private void OnGameReady()
        {
            UpdateState(GameState.Ready);
        }

        private static void Initialized()
        {
            if (_instance)
            {
                return;
            }
            
            UIManager uiManager = UIManager.Instance;

            var inGameItemPresenter = PresenterFactory.CreateOrGet<InGameItemPresenter>();
            inGameItemPresenter.Initialize(uiManager.CreateOrGetView<InGameItemView>());
        }

        public void UpdateUserInGameItem()
        {
            var inGameItemPresenter = PresenterFactory.CreateOrGet<InGameItemPresenter>();
            inGameItemPresenter.AddInGameItemData(InGameItemType.Hammer, 10);
            inGameItemPresenter.AddInGameItemData(InGameItemType.Shuffle, 10);
            inGameItemPresenter.AddInGameItemData(InGameItemType.VerticalRocket, 10);
            inGameItemPresenter.AddInGameItemData(InGameItemType.HorizontalRocket, 10);
        }

        private async void OnGameOver()
        {
            if (GameState.Start != _gameState)
            {
                return;
            }
            
            UpdateState(GameState.End);
            var gameFailPresenter = PresenterFactory.CreateOrGet<GameFailPresenter>();
            var gameFailedView = UIManager.Instance.CreateOrGetView<GameFailedView>();
            gameFailPresenter.Initialize(gameFailedView, ModelFactory.CreateOrGet<MissionModel>());
            await gameFailPresenter.GameFailProcess();
        }

        private async void OnGameClear()
        {
            if (GameState.Start != _gameState)
            {
                return;
            }
            
            UpdateState(GameState.End);
            var gameWinPresenter = PresenterFactory.CreateOrGet<GameWinPresenter>();
            var gameWinView = UIManager.Instance.CreateOrGetView<GameWinView>();
            var missionModel = ModelFactory.CreateOrGet<MissionModel>();
            gameWinPresenter.Initialize(gameWinView, missionModel);

            await gameWinPresenter.GameWinProcess();
        }

        private void UpdateState(GameState gameState) => _gameState = gameState;
    }
}