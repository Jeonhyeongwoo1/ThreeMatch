using System;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.InGame.UI;
using ThreeMatch.Manager;
using ThreeMatch.OutGame.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            InGameMenuButtonCallbackInitialize();
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
            inGameItemPresenter.Initialize(uiManager.GetView<InGameItemView>());
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
            var gameFailedView = PopupManager.Instance.GetPopup<GameFailedPopup>();
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
            var gameWinPopup = PopupManager.Instance.GetPopup<GameWinPopup>();
            var missionModel = ModelFactory.CreateOrGet<MissionModel>();
            gameWinPresenter.Initialize(gameWinPopup, missionModel);

            await gameWinPresenter.GameWinProcess();
        }

        private void InGameMenuButtonCallbackInitialize()
        {
            var ingameMenuButtonModel = ModelFactory.CreateOrGet<InGameMenuButtonModel>();
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.NextStage, OnChangeNextStage);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.Share, OnShare);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.MoveToStageLevelScene, OnMoveToStageLevelScene);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.RestartGame, OnRestartGame);
            ingameMenuButtonModel.AddCallback(InGameMenuPopupButtonType.ShowAd, OnShowAds);
        }

        private void OnShowAds()
        {
            Debug.Log("OnShowAds");
        }

        private void OnRestartGame()
        {
            Debug.Log("OnRestartGame");
            int stageLevel = PlayerPrefs.GetInt(PlayerPrefsKeys.lastStageLevel);
            StageManager.Instance.ReloadStage(stageLevel);
        }

        private void OnMoveToStageLevelScene()
        {
            Debug.Log("OnMoveToStageLevelScene");
            SceneManager.LoadSceneAsync(SceneType.StageLevel.ToString());
        }

        private void OnShare()
        {
            Debug.Log("OnShare");
        }

        private void OnChangeNextStage()
        {
            Debug.Log("OnChangeNextStage");
            int stageLevel = PlayerPrefs.GetInt("selectedStageLevel");
            PlayerPrefs.SetInt(PlayerPrefsKeys.unlockStageLevel, stageLevel + 1);
            SceneManager.LoadSceneAsync(SceneType.StageLevel.ToString());
        }

        private void UpdateState(GameState gameState) => _gameState = gameState;
    }
}