using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Stage : IDisposable
    {
        private Board _board;
        private Mission _mission;
        private int _remainingMoveCount;

        private event Action<CellType, int, ObstacleCellType, CellImageType> OnCheckMissionAction;
        private event Action OnEndDragAction;

        public Stage(BoardInfoData[,] boardInfoArray, List<MissionInfoData> missionDataList, int remainingMoveCount)
        {
            AddEvents();
            
            Board board = new Board(boardInfoArray, OnCheckMissionAction, OnEndDragAction);
            _board = board;

            Mission mission = new Mission(missionDataList);
            _mission = mission;

            _remainingMoveCount = remainingMoveCount;
            GameManager.onChangeRemainingMoveCountAction?.Invoke(_remainingMoveCount);
        }
        
        public void Dispose()
        {
            OnEndDragAction -= OnEndDrag;
            OnCheckMissionAction -= OnCheckMission;
            GameManager.onInGameItemUsagePendingAction -= OnItemUsagePendingAction;
            _board?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void AddEvents()
        {
            OnEndDragAction += OnEndDrag;
            OnCheckMissionAction += OnCheckMission;
            GameManager.onInGameItemUsagePendingAction += OnItemUsagePendingAction;
        }

        public async UniTask BuildAsync(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab, Transform container = null)
        {
            await _board.BuildAsync(centerPosition, blockPrefab, container);
        }
        
        public void CustomBuild(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab, Transform parent = null)
        {
            _board.Build(centerPosition, blockPrefab, parent);
        }

        public async UniTask BuildAfterProcessAsync()
        {
            await _board.BuildAfterProcess();
        }

        private void OnEndDrag()
        {
            _remainingMoveCount--;
            GameManager.onChangeRemainingMoveCountAction.Invoke(_remainingMoveCount);
            
            if (_remainingMoveCount == 0)
            {
                bool isAllSuccessMission = _mission.IsAllSuccessMission();
                if (isAllSuccessMission)
                {
                    Debug.Log("Game Clear");
                    //Game Clear
                    GameManager.onGameClearAction?.Invoke();
                }
                else
                {
                    Debug.Log("Game over");
                    //Game over
                    GameManager.onGameOverAction?.Invoke();
                }
            }
        }

        private void OnCheckMission(CellType cellType, int removeCount, ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None)
        {
            MissionType missionType = DetermineMissionTypeByCellTypeAndCellImageType(cellType, obstacleCellType, cellImageType);
            if (missionType == MissionType.None)
            {
                return;
            }
            
            bool isClearMission = _mission.TryClearMission(missionType, removeCount);
            if (!isClearMission)
            {
                return;
            }
            
            bool isAllSuccessMission = _mission.IsAllSuccessMission();
            if (isAllSuccessMission)
            {
                if (_remainingMoveCount > 0)
                {
                    Debug.Log("AllClear");
                    GameManager.onGameClearAction?.Invoke();
                    // GameManager.onAllSuccessMissionAction?.Invoke();
                }
                else
                {
                    GameManager.onGameOverAction?.Invoke();
                }
            }
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
                        case ObstacleCellType.Box:
                            return MissionType.RemoveObstacleOneHitBoxCell;
                        case ObstacleCellType.IceBox:
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