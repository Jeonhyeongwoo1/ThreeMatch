using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Manager;
using ThreeMatch.Shared.Data;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Stage : IDisposable
    {
        private Board _board;
        private Mission _mission;
        private InGameScore _inGameScore;
        private int _remainingMoveCount;

        private event Action<CellType, Vector3, int, ObstacleCellType, CellImageType> OnCheckMissionAction;
        private event Action OnEndDragAction;

        public Stage(BoardInfoData[,] boardInfoArray, List<MissionInfoData> missionDataList, int remainingMoveCount, int aimScore)
        {
            AddEvents();
            
            _board = new Board(boardInfoArray, OnCheckMissionAction, OnEndDragAction, GameManager.onCellComboAction);
            _mission = new Mission(missionDataList);
            _inGameScore = new InGameScore(aimScore);
            _remainingMoveCount = remainingMoveCount;
            
            GameManager.onChangeRemainingMoveCountAction?.Invoke(_remainingMoveCount);
        }
        
        public void Dispose()
        {
            OnEndDragAction -= OnEndDrag;
            OnCheckMissionAction -= OnCheckMission;
            GameManager.onCellComboAction -= OnAddScore;
            GameManager.onInGameItemUsagePendingAction -= OnItemUsagePendingAction;
            _board?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void AddEvents()
        {
            OnEndDragAction += OnEndDrag;
            OnCheckMissionAction += OnCheckMission;
            GameManager.onCellComboAction += OnAddScore; 
            GameManager.onInGameItemUsagePendingAction += OnItemUsagePendingAction;
        }
        
        private void OnAddScore(int score, int comboCount)
        {
            _inGameScore.AddScore(score, comboCount);
        }

        private void OnEndDrag()
        {
            _remainingMoveCount--;
            GameManager.onChangeRemainingMoveCountAction.Invoke(_remainingMoveCount);
            
            if (_remainingMoveCount == 0)
            {
                bool isAllSuccessMission = _mission.IsAllSuccessMission();
                GameResultData gameResultData = MakeGameResultData();
                
                if (isAllSuccessMission)
                {
                    Debug.Log("Game Clear");
                    //Game Clear
                    GameManager.onGameClearAction?.Invoke(gameResultData);
                }
                else
                {
                    Debug.Log("Game over");
                    //Game over
                    GameManager.onGameOverAction?.Invoke(gameResultData);
                }
            }
        }

        private GameResultData MakeGameResultData()
        {
            return new GameResultData
            {
                starCount = _inGameScore.GetStarCount(),
                removeCellCount = _board.RemoveCellCount,
                usedItemCountArray = _board.UsedItemCountArray
            };
        }

        private void OnCheckMission(CellType cellType, Vector3 cellPosition, int removeCount, ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None)
        {
            MissionType missionType = DetermineMissionTypeByCellTypeAndCellImageType(cellType, obstacleCellType, cellImageType);
            if (missionType == MissionType.None)
            {
                return;
            }

            bool isClearMission = false;
            Vector3 missionElementPosition;
            (isClearMission, missionElementPosition) = _mission.TryClearMission(missionType, removeCount);
            if (!isClearMission)
            {
                return;
            }

            if (missionElementPosition != Vector3.zero)
            {
                TokenManager.Instance.GenerateCellToken(cellType, cellPosition, missionElementPosition,
                    obstacleCellType, cellImageType);
            }
            
            bool isAllSuccessMission = _mission.IsAllSuccessMission();
            if (isAllSuccessMission)
            {
                GameResultData gameResultData = MakeGameResultData();
                if (_remainingMoveCount > 0)
                {
                    Debug.Log("AllClear");
                    GameManager.onGameClearAction?.Invoke(gameResultData);
                    // GameManager.onAllSuccessMissionAction?.Invoke();
                }
                else
                {
                    GameManager.onGameOverAction?.Invoke(gameResultData);
                }
            }
        }
        
        public async UniTask BuildAsync(Vector2 centerPosition, GameObject blockPrefab, Transform container = null)
        {
            await _board.BuildAsync(centerPosition, blockPrefab, container);
        }
        
        public void CustomBuild(Vector2 centerPosition, GameObject blockPrefab, Transform parent = null)
        {
            _board.Build(centerPosition, blockPrefab, parent);
        }

        public async UniTask BuildAfterProcessAsync()
        {
            await _board.PostSwapProcess();
        }
        
        private MissionType DetermineMissionTypeByCellTypeAndCellImageType(CellType cellType, ObstacleCellType obstacleCellType = ObstacleCellType.None,
            CellImageType cellImageType = CellImageType.None)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    switch (cellImageType)
                    {
                        case CellImageType.Yellow:
                            return MissionType.RemoveNormalYellowCell;
                        case CellImageType.Green:
                            return MissionType.RemoveNormalGreenCell;
                        case CellImageType.Blue:
                            return MissionType.RemoveNormalBlueCell;
                        case CellImageType.Purple:
                            return MissionType.RemoveNormalPurpleCell;
                        case CellImageType.Red:
                            return MissionType.RemoveNormalRedCell;
                        case CellImageType.Orange:
                            return MissionType.RemoveNormalOrangeCell;
                        case CellImageType.None:
                        default:
                            Debug.LogError(
                                $"failed determine mission type : {cellType} / {obstacleCellType} / {cellImageType}");
                            return MissionType.None;
                    }
                case CellType.Obstacle:
                    switch (obstacleCellType)
                    {
                        case ObstacleCellType.OneHitBox:
                            return MissionType.RemoveObstacleOneHitBoxCell;
                        case ObstacleCellType.HitableBox:
                            return MissionType.RemoveObstacleHitableBoxCell;
                        case ObstacleCellType.Cage:
                            return MissionType.RemoveObstacleCageCell;
                        case ObstacleCellType.None:
                        default:
                            Debug.LogError(
                                $"failed determine mission type : {cellType} / {obstacleCellType} / {cellImageType}");
                            return MissionType.None;
                    }
                case CellType.Generator:
                    return MissionType.RemoveStarGeneratorCell;
                case CellType.Rocket:
                case CellType.Wand:
                case CellType.Bomb:
                case CellType.None:
                default:
                    return MissionType.None;
            }
        }

        public void OnItemUsagePendingAction(InGameItemType inGameItemType)
        {
            _board.SetPendingUseInGameItemType(inGameItemType);
        }
    }   
}