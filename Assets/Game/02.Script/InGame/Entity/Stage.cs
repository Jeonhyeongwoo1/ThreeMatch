using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Stage
    {
        public Board Board => _board;
        private Board _board;
        private Mission _mission;
        private int _remainingMoveCount;

        private event Action<CellType, int, CellImageType> OnCheckMissionAction;
        private event Action OnEndDragAction;

        public Stage(BoardInfoData[,] boardInfoArray, List<MissionInfoData> missionDataList, int remainingMoveCount)
        {
            AddEvents();
            
            Board board = new Board(boardInfoArray, OnCheckMissionAction, OnEndDragAction);
            _board = board;

            Mission mission = new Mission(missionDataList);
            _mission = mission;

            _remainingMoveCount = remainingMoveCount;
        }

        private void AddEvents()
        {
            OnEndDragAction += OnEndDrag;
            OnCheckMissionAction += OnCheckMission;
            GameManager.onInGameItemUsagePendingAction += OnItemUsagePendingAction;
        }

        public async UniTask BuildAsync(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab, Transform container = null)
        {
            await _board.BuildAsync(centerPosition, blockPrefab, cellPrefab, container);
        }
        
        public void CustomBuild(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab, Transform parent = null)
        {
            _board.Build(centerPosition, blockPrefab, cellPrefab, parent);
        }

        private void OnEndDrag()
        {
            _remainingMoveCount--;
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

        private void OnCheckMission(CellType cellType, int removeCount, CellImageType cellImageType = CellImageType.None)
        {
            MissionType missionType = DetermineMissionTypeByCellTypeAndCellImageType(cellType, cellImageType);
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
                    GameManager.onAllSuccessMissionAction?.Invoke();
                }
                else
                {
                    GameManager.onGameOverAction?.Invoke();
                }
            }
        }

        private MissionType DetermineMissionTypeByCellTypeAndCellImageType(CellType cellType,
            CellImageType cellImageType = CellImageType.None)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    switch (cellImageType)
                    {
                        case CellImageType.Blue:
                            return MissionType.RemoveNormalBlueCell;
                        case CellImageType.Green:
                            return MissionType.RemoveNormalGreenCell;
                        case CellImageType.Pink:
                            return MissionType.RemoveNormalPinkCell;
                        case CellImageType.Purple:
                            return MissionType.RemoveNormalPurpleCell;
                        case CellImageType.Red:
                            return MissionType.RemoveNormalRedCell;
                        case CellImageType.None:
                        default:
                            return MissionType.None;
                    }
                case CellType.Obstacle_Box:
                    return MissionType.RemoveObstacleBoxCell;
                case CellType.Obstacle_IceBox:
                    return MissionType.RemoveObstacleIceBoxCell;
                case CellType.Obstacle_Cage:
                    return MissionType.RemoveObstacleCageCell;
                case CellType.Generator:
                    return MissionType.RemoveGeneratorCell;
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