using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.Helper;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using ThreeMatch.InGame.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThreeMatch.InGame
{
    public class Board : IDisposable
    {
        [Serializable]
        public struct CellMovementInfo
        {
            public bool isSpawn;
            public Cell cell;
            public List<Vector3> movePositionList;
        }
        
        [Serializable]
        public struct MatchedSameImageCellInfo
        {
            public CellMatchedType cellMatchedType;
            public List<Cell> cellList;
        }
        
        private Cell[,] _cellArray;
        private Block[,] _blockArray;
        private readonly int _row;
        private readonly int _column;
        private BoardState _boardState = BoardState.None;

        private List<Cell> _neighborObstacleCellList = new(4);
        private List<Cell> _neighborCellList = new ();
        private Cell _selectedCell;
        
        private Transform _boardContainerTransform;
        private UniTaskCompletionSource _executeInGameItemTaskCompletionSource;
        private readonly Action<CellType, int, ObstacleCellType, CellImageType> _onCheckMissionAction;
        private readonly Action _onEndDragAction;

        public Board(BoardInfoData[,] boardInfoDataArray, Action<CellType, int, ObstacleCellType, CellImageType> onCheckMissionAction, Action onEndDragAction)
        {
            _row = boardInfoDataArray.GetLength(0);
            _column = boardInfoDataArray.GetLength(1);
            CreateBlockArray(_row, _column, boardInfoDataArray);
            CreateCellArray(_row, _column, boardInfoDataArray);
         
            InputPanel.OnPointerDownAction += OnPointerDown;
            InputPanel.OnDragAction += OnDrag;
            InputPanel.OnPointerUpAction += OnPointerUp;
            _onCheckMissionAction = onCheckMissionAction;
            _onEndDragAction = onEndDragAction;
        }
        
        public void Dispose()
        {
            _selectedCell?.Dispose();
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    _blockArray[i, j]?.Dispose();
                    _cellArray[i, j]?.Dispose();
                }
            }
            
            InputPanel.OnPointerDownAction -= OnPointerDown;
            InputPanel.OnDragAction -= OnDrag;
            InputPanel.OnPointerUpAction -= OnPointerUp;
        }
        
        private async void OnPointerDown(Vector2 beginPosition)
        {
            Debug.Log($"onPointerDown ---- {_selectedCell} / {GetBoardState()}");
            if (_selectedCell != null || IsBoardState(BoardState.Swapping) || !IsOutOfBoardRange(beginPosition))
            {
                return;
            }

            Cell cell = TryGetCell(beginPosition);
            if (cell == null)
            {
                return;
            }
            
            _selectedCell = cell;
            _neighborCellList = GetNeighborCellList(cell);
            
            if (IsBoardState(BoardState.PendingUseInGameItem))
            {
                await ExecuteInGameItem();
            }
        }

        private async void OnDrag(Vector2 dragPosition)
        {
            if (_selectedCell == null || IsBoardState(BoardState.Swapping))
            {
                return;
            }

            Cell cell = TryGetCell(_neighborCellList, dragPosition);
            if (cell == null || _selectedCell == cell || IsObstacleOrGeneratorCellType(cell.CellType) ||
                IsObstacleOrGeneratorCellType(_selectedCell.CellType))
            {
                return;
            }

            bool isSuccess = await TrySwap(cell, _selectedCell);
            if (isSuccess)
            {
                do
                {
                    await PostSwapProcess();
                } while (await CheckMatchingCell());
            }

            //살짝 딜레이를 줌
            await UniTask.WaitForSeconds(0.5f);
            
            UpdateBoardState(BoardState.Ready);
            ResetDrag();
            _onEndDragAction.Invoke();
        }

        private void OnPointerUp(Vector2 endPosition)
        {
            if (_selectedCell != null && IsBoardState(BoardState.Ready))
            {
                ResetDrag();
            }
        }

        private void ResetDrag()
        {
            _selectedCell = null;
        }

        private async UniTask<bool> CheckMatchingCell()
        {
            List<UniTask> taskList = new List<UniTask>();
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    Cell cell = _cellArray[i, j];

                    if (block.BlockType == BlockType.None || cell == null || cell.CellType != CellType.Normal)
                    {
                        continue;
                    }
                    
                    MatchedSameImageCellInfo? matchedSameImageCellInfo = GetMatchedSameImageCellInfo(cell);
                    if (!matchedSameImageCellInfo.HasValue)
                    {
                        continue;
                    }

                    taskList.Add(HandleMatchedCell(matchedSameImageCellInfo));
                }
            }

            if (taskList.Count <= 0)
            {
                return false;
            }
            
            await UniTask.WhenAll(taskList);
            return true;
        }

        private async UniTask FillCellOnEmptyBlock()
        {
             Dictionary<int, List<CellMovementInfo>>
                cellMovementInfoDict = new Dictionary<int, List<CellMovementInfo>>();
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    Cell cell = _cellArray[i, j];
                    var cellMovePositionList = new List<Vector3>();
                    //비어있는 블록을 기준으로 찾기
                    if (block.BlockType != BlockType.None && cell == null)
                    {
                        int[] direction = { 0, 1, -1 };
                        foreach (int columnDir in direction)
                        {
                            cellMovePositionList.Add(block.Position);
                            Cell spawnOrMoveCell = null;
                            bool isSpawn = false;
                            bool isSuccess = TryFindOrSpawnCellForEmptyBlock(i, j, columnDir, ref cellMovePositionList,
                                ref spawnOrMoveCell, ref isSpawn);
                            if (isSuccess)
                            {
                                CellMovementInfo info = new CellMovementInfo
                                {
                                    movePositionList = cellMovePositionList,
                                    cell = spawnOrMoveCell,
                                    isSpawn = isSpawn
                                };
                                
                                if (!cellMovementInfoDict.TryGetValue(spawnOrMoveCell.Column, out var cellMovementInfoList))
                                {
                                    cellMovementInfoList = new List<CellMovementInfo>();
                                }
                                
                                cellMovementInfoList.Add(info);
                                cellMovementInfoDict[spawnOrMoveCell.Column] = cellMovementInfoList;
                                break;
                            }
                        }
                    }
                }
            }

            List<UniTask> cellPostSwapTaskList = new();
            foreach (KeyValuePair<int, List<CellMovementInfo>> cellMovementInfo in cellMovementInfoDict)
            {
                List<CellMovementInfo> cellMovementInfoList = cellMovementInfo.Value;
                foreach (CellMovementInfo info in cellMovementInfoList)
                {
                    if (info.isSpawn)
                    {
                        info.cell.CellBehaviour.Activate(false);
                    }
                }
                
                cellPostSwapTaskList.Add(MoveCell(cellMovementInfoList));
            }

            await UniTask.WhenAll(cellPostSwapTaskList);
        }
        
        private async UniTask PostSwapProcess()
        {
            //부서지는 블록 주변에 
            await FillCellOnEmptyBlock();
        }

        private async UniTask MoveCell(List<CellMovementInfo> cellMovementInfoList)
        {
            List<UniTask> list = new();
            foreach (CellMovementInfo info in cellMovementInfoList)
            {
                list.Add(info.cell.CellBehaviour.MoveAsync(info.movePositionList, info.isSpawn));
                await UniTask.WaitForSeconds(Const.CellMoveAnimationDuration);
            }

            await UniTask.WhenAll(list);
        }
        
        private async UniTask<bool> HandleMatchedCell(MatchedSameImageCellInfo? matchedSameImageCellInfo)
        {
            if (!matchedSameImageCellInfo.HasValue)
            {
                return false;
            }

            List<Cell> cellList = matchedSameImageCellInfo.Value.cellList;
            CellMatchedType cellMatchedType = matchedSameImageCellInfo.Value.cellMatchedType;
            switch (cellMatchedType)
            {
                case CellMatchedType.Three:
                    foreach (Cell cell in cellList)
                    {
                        RemoveCellProcess(cell.Row, cell.Column);
                    }

                    await UniTask.WaitForSeconds(Const.CellRemoveAnimationDuration);
                    return true;
                case CellMatchedType.Vertical_Four:
                case CellMatchedType.Horizontal_Four:
                case CellMatchedType.Five:
                case CellMatchedType.Five_Shape:
                    //첫번쨰 셀로 모두 합치기
                    Cell firstCell = cellList[0];
                    List<UniTask> moveTaskList = new(cellList.Count - 2);
                    for (int i = 1; i < cellList.Count; i++)
                    {
                        Cell cell = cellList[i];
                        if (cell == firstCell)
                        {
                            continue;
                        }
                        
                        _cellArray[cell.Row, cell.Column] = null;
                        var moveTask = cell.CellBehaviour.MoveAsync(firstCell.Position).ContinueWith(() =>
                        {
                            cell.TryRemoveCell();
                        });
                        
                        moveTaskList.Add(moveTask);
                    }
                    
                    await UniTask.WhenAll(moveTaskList);
                    firstCell.SetCellTypeFromMatch(cellMatchedType, _boardContainerTransform);
                    return true;
                case CellMatchedType.None:
                default:
                    return false;
            }
        }

        private async UniTask ActivateRocketProcessAsync(int row, int column, int startIndex, int endIndex,
            bool isPositive, bool isUpDir)
        {
            int step = isPositive ? 1 : -1;
            List<UniTask> cellTask = new List<UniTask>();
            for (int i = startIndex; i != endIndex; i += step)
            {
                Block block = isUpDir ? _blockArray[i, column] : _blockArray[row, i];
                Cell cell = isUpDir ? _cellArray[i, column] : _cellArray[row, i];

                UniTask moveTask = UniTask.WaitForSeconds(0.05f);
                if (block.BlockType == BlockType.None || cell == null)
                {
                    await moveTask;
                    continue;
                }

                switch (cell.CellType)
                {
                    case CellType.Generator:
                    case CellType.Obstacle:
                    case CellType.Normal:
                        RemoveCellProcess(cell.Row, cell.Column);
                        break;
                    case CellType.Rocket:
                    case CellType.Wand:
                    case CellType.Bomb:
                        cellTask.Add(TryActivateCellProperty(cell));
                        break;
                }

                await moveTask;
            }

            await cellTask;
        }

        private async UniTask ActivateBombProcessAsync(int row, int column, int range)
        {
            UniTask cellTask = UniTask.CompletedTask;
            for (int i = row - range; i <= row + range; i++)
            {
                for (int j = column - range; j <= column + range; j++)
                {
                    if (IsOutOfBoardRange(i, j))
                    {
                        continue;
                    }

                    Block block = _blockArray[i, j];
                    Cell cell = _cellArray[i, j];
                    if (block.BlockType == BlockType.None || cell == null)
                    {
                        continue;
                    }
                    
                    switch (cell.CellType)
                    {
                        case CellType.Generator:
                        case CellType.Obstacle:
                        case CellType.Normal:
                            RemoveCellProcess(cell.Row, cell.Column);
                            break;
                        case CellType.Rocket:
                        case CellType.Wand:
                        case CellType.Bomb:
                            cellTask = TryActivateCellProperty(cell);
                            break;
                    }
                }
            }

            await UniTask.WaitForSeconds(0.5f);
            await cellTask;
        }

        private async UniTask ActivateWandProcessAsync(Cell activateCell)
        {
            List<Cell> sameImageTypeCellList = GetSameImageTypeCellList(activateCell.CellImageType);
            List<UniTask> taskList = new();
            foreach (Cell cell in sameImageTypeCellList)
            {
                if (cell.CellType == CellType.Normal)
                {
                    var wand = activateCell.CellBehaviour as WandBehaviour;
                    wand.ShowLighting(activateCell.CellType, cell);
                    await UniTask.WaitForSeconds(0.05f);
                }
                else
                {
                    taskList.Add(TryActivateCellProperty(cell));
                }
            }
            
            await UniTask.WaitForSeconds(1f, cancelImmediately: true);
            
            sameImageTypeCellList.ForEach(cell =>
            {
                if (cell.CellType == CellType.Normal)
                {
                    RemoveCellProcess(cell.Row, cell.Column);
                }
            });
            
            await UniTask.WhenAll(taskList);
        }

        private bool RemoveCell(Cell cell)
        {
            _onCheckMissionAction?.Invoke(cell.CellType, 1, cell.ObstacleCellType, cell.CellImageType);
            bool isSuccess = cell.TryRemoveCell();
            if (isSuccess)
            {
                if (cell.CellType == CellType.Obstacle)
                {
                    //케이지는 부서지면 일반 셀로 변경
                    cell.ChangeCellType(CellType.Normal);
                }
                else
                {
                    _cellArray[cell.Row, cell.Column] = null;
                }

                return true;
            }

            return false;
        }

        private void RemoveCellProcess(int row, int column)
        {
            Cell cell = _cellArray[row, column];
            if (cell == null)
            {
                return;
            }

            // 장애물 셀이 있는지 확인 후 부서질 수 있는지 체크
            List<Cell> neighborObstacleCellList = GetNeighborObstacleOrGeneratorCellList(cell);
            if (neighborObstacleCellList != null)
            {
                neighborObstacleCellList.ForEach(v => RemoveCell(v));
            }
            
            RemoveCell(cell);
        }

        private List<Cell> GetNeighborObstacleOrGeneratorCellList(Cell cell)
        {
            if (cell.CellType != CellType.Normal)
            {
                return null;
            }
            
            _neighborObstacleCellList.Clear();
            
            int[,] dirArray = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
            for (int i = 0; i < dirArray.GetLength(0); i++)
            {
                int x = dirArray[i, 0];
                int y = dirArray[i, 1];

                int row = cell.Row + x;
                int column = cell.Column + y;
                if (IsOutOfBoardRange(row, column))
                {
                    continue;
                }

                Cell neighborCell = _cellArray[row, column];
                if (neighborCell == null || !IsObstacleOrGeneratorCellType(neighborCell.CellType) || cell == neighborCell)
                {
                    continue;
                }
                
                _neighborObstacleCellList.Add(neighborCell);
            }

            return _neighborObstacleCellList;
        }

        private bool IsObstacleOrGeneratorCellType(CellType cellType)
        {
            return cellType == CellType.Obstacle || cellType == CellType.Generator;
        }

        private async UniTask<bool> TryActivateCellProperty(Cell activateCell)
        {
            int row = activateCell.Row;
            int column = activateCell.Column;
            //Task 리스트에 들어가는 것과 동시에 실행
            List<UniTask> taskList = new();
            switch (activateCell.CellType)
            {
                case CellType.Generator:
                case CellType.Obstacle:
                case CellType.None:
                case CellType.Normal:
                default:
                    return false;
                case CellType.Rocket:
                    RemoveCellProcess(activateCell.Row, activateCell.Column);
        
                    activateCell.ActivateRocket();
                    switch (activateCell.CellMatchedType)
                    {
                        case CellMatchedType.Vertical_Four:
                            taskList.Add(ActivateRocketProcessAsync(row, column, row, _row, true, true));
                            taskList.Add(ActivateRocketProcessAsync(row, column, row, -1, false, true));
                            break;
                        case CellMatchedType.Horizontal_Four:
                            taskList.Add(ActivateRocketProcessAsync(row, column, column, _column, true, false));
                            taskList.Add(ActivateRocketProcessAsync(row, column, column, -1, false, false));
                            break;
                    }
                    
                    break;
                case CellType.Wand:
                    RemoveCellProcess(activateCell.Row, activateCell.Column);
                    taskList.Add(ActivateWandProcessAsync(activateCell));
                    break;
                case CellType.Bomb:
                    RemoveCellProcess(activateCell.Row, activateCell.Column);
                    taskList.Add(ActivateBombProcessAsync(row, column, Const.BombRange));
                    break;
            }
            
            await UniTask.WhenAll(taskList);
            return true;
        }
        
        //CellB가 selected cell
        private async UniTask<bool> TrySwap(Cell cellA, Cell cellB)
        {
            UpdateBoardState(BoardState.Swapping);
            await Swap(cellA, cellB);
            
            //두 개가 일반형인 경우
            if (cellA.CellType == CellType.Normal && cellB.CellType == CellType.Normal)
            {
                MatchedSameImageCellInfo? cellAMatchedSameImageCellInfo = GetMatchedSameImageCellInfo(cellA);
                MatchedSameImageCellInfo? cellBMatchedSameImageCellInfo = GetMatchedSameImageCellInfo(cellB);

                await UniTask.WhenAll(HandleMatchedCell(cellAMatchedSameImageCellInfo),
                    HandleMatchedCell(cellBMatchedSameImageCellInfo));

                if (cellAMatchedSameImageCellInfo.HasValue || cellBMatchedSameImageCellInfo.HasValue)
                {
                    return true;
                }

                await Swap(cellA, cellB);
                return false;
            }

            //CellA는 특수 타입이고 CellB는 일반형
            if (cellA.CellType != CellType.Normal && cellB.CellType == CellType.Normal)
            {
                MatchedSameImageCellInfo? cellBMatchedSameImageCellInfo = GetMatchedSameImageCellInfo(cellB);
                await UniTask.WhenAll(TryActivateCellProperty(cellA), HandleMatchedCell(cellBMatchedSameImageCellInfo));
                return true;
            }

            //CellB는 특수 타입이고 CellA는 일반형
            if (cellA.CellType == CellType.Normal && cellB.CellType != CellType.Normal)
            {
                MatchedSameImageCellInfo? cellAMatchedSameImageCellInfo = GetMatchedSameImageCellInfo(cellA);
                await UniTask.WhenAll(TryActivateCellProperty(cellB), HandleMatchedCell(cellAMatchedSameImageCellInfo));
                return true;
            }

            if (await HandleCellCombination(cellA, cellB))
            {
                return true;
            }

            Debug.LogWarning($"Swap error cellA {cellA.CellType} / CellB {cellB.CellType}");
            // await Swap(cellA, cellB);
            return false;
        }

        private CellCombinationType EvaluateCellCombinationType(CellType cellTypeA, CellType cellTypeB)
        {
            switch ((cellTypeA, cellTypeB))
            {
                case (CellType.Rocket, CellType.Rocket):
                    return CellCombinationType.RocketAndRocket;
                case (CellType.Rocket, CellType.Bomb):
                case (CellType.Bomb, CellType.Rocket):
                    return CellCombinationType.RocketAndBomb;
                case (CellType.Rocket, CellType.Wand):
                    case (CellType.Wand, CellType.Rocket):
                    return CellCombinationType.RocketAndWand;
                case (CellType.Bomb, CellType.Bomb):
                    return CellCombinationType.BombAndBomb;
                case (CellType.Bomb, CellType.Wand):
                    case (CellType.Wand, CellType.Bomb):
                    return CellCombinationType.BombAndWand;
                case (CellType.Wand, CellType.Wand):
                    return CellCombinationType.WandAndWand;
                default:
                    return CellCombinationType.None;
            }
        }

        private async UniTask<bool> HandleCellCombination(Cell cellA, Cell cellB)
        {
            CellCombinationType cellCombinationType = EvaluateCellCombinationType(cellA.CellType, cellB.CellType);
            switch (cellCombinationType)
            {
                case CellCombinationType.RocketAndBomb:
                    /*
                     1. 3x3 폭탄 크기의 셀을 먼저 제거
                     2. 남은 셀을들 한번에 지움
                     */

                    //대각선
                    int row = cellB.Row;
                    int column = cellB.Column;
                    RemoveCellProcess(cellA.Row, cellA.Column);
                    RemoveCellProcess(cellB.Row, cellB.Column);
                    await ActivateBombProcessAsync(row, column, Const.BombRange);
                    await UniTask.WaitForSeconds(0.5f);
                    
                    ActivateRocketAndBombCombination(row, true);
                    ActivateRocketAndBombCombination(column, false);

                    await UniTask.WaitForSeconds(0.5f);
                    return true;
                case CellCombinationType.RocketAndWand:
                    List<Cell> sameImageTypeCellList = new List<Cell>();
                    if (cellA.CellType == CellType.Wand)
                    {
                        sameImageTypeCellList = GetSameImageTypeCellList(cellA.CellImageType);
                    }

                    if (cellB.CellType == CellType.Wand)
                    {
                        sameImageTypeCellList = GetSameImageTypeCellList(cellA.CellImageType);
                    }

                    sameImageTypeCellList.ForEach(cell =>
                    {
                        int random = Random.Range(0, 2);
                        cell.SetCellTypeFromMatch(
                            random == 0 ? CellMatchedType.Horizontal_Four : CellMatchedType.Vertical_Four, _boardContainerTransform);
                        cell.ActivateRocket();
                    });

                    RemoveCellProcess(cellA.Row, cellA.Column);
                    RemoveCellProcess(cellB.Row, cellB.Column);
                    
                    await UniTask.WaitForSeconds(0.5f);

                    List<UniTask> taskList = new();
                    sameImageTypeCellList.ForEach(cell =>
                    {
                        int row = cell.Row;
                        int column = cell.Column;
                        switch (cell.CellMatchedType)
                        {
                            case CellMatchedType.Vertical_Four:
                                taskList.Add(ActivateRocketProcessAsync(row, column, row, _row, true, true));
                                taskList.Add(ActivateRocketProcessAsync(row, column, row, -1, false, true));
                                break;
                            case CellMatchedType.Horizontal_Four:
                                taskList.Add(ActivateRocketProcessAsync(row, column, column, _column, true, false));
                                taskList.Add(ActivateRocketProcessAsync(row, column, column, -1, false, false));
                                break;
                        }
                    });

                    await UniTask.WhenAll(taskList);

                    return true;
                case CellCombinationType.RocketAndRocket:
                    //선택된 셀이 아닌 변경되는 셀을 기준으로 사방으로 로켓을 발사한다.
                    row = cellA.Row;
                    column = cellA.Column;

                    RemoveCellProcess(cellA.Row, cellA.Column);
                    RemoveCellProcess(cellB.Row, cellB.Column);
                    
                    //가로 세로 1줄 로켓 발사
                    UniTask upTask = ActivateRocketProcessAsync(row, column, row, _row, true, true);
                    UniTask downTask = ActivateRocketProcessAsync(row, column, row, -1, false, true);
                    UniTask rightTask = ActivateRocketProcessAsync(row, column, column, _column, true, false);
                    UniTask leftTask = ActivateRocketProcessAsync(row, column, column, -1, false, false);
                    await UniTask.WhenAll(rightTask, leftTask, upTask, downTask);
                    return true;
                case CellCombinationType.BombAndWand:
                    sameImageTypeCellList = new List<Cell>();
                    if (cellA.CellType == CellType.Wand)
                    {
                        sameImageTypeCellList = GetSameImageTypeCellList(cellA.CellImageType);
                    }

                    if (cellB.CellType == CellType.Wand)
                    {
                        sameImageTypeCellList = GetSameImageTypeCellList(cellA.CellImageType);
                    }

                    sameImageTypeCellList.ForEach(cell=>
                    {
                        cell.SetCellTypeFromMatch(CellMatchedType.Five_Shape, _boardContainerTransform);
                    });
                
                    RemoveCellProcess(cellA.Row, cellA.Column);
                    RemoveCellProcess(cellB.Row, cellB.Column);
                    
                    await UniTask.WaitForSeconds(0.5f);

                    taskList = new List<UniTask>();
                    sameImageTypeCellList.ForEach(cell =>
                        taskList.Add(ActivateBombProcessAsync(cell.Row, cell.Column, Const.BombRange)));
                
                    await UniTask.WhenAll(taskList);
                    return true;
                case CellCombinationType.BombAndBomb:
                    //선택된 셀이 아닌 변경되는 셀을 기준으로 사방으로 로켓을 발사한다.
                    row = cellA.Row;
                    column = cellA.Column;
                    RemoveCellProcess(cellA.Row, cellA.Column);
                    RemoveCellProcess(cellB.Row, cellB.Column);
                    
                    await ActivateBombProcessAsync(row, column, Const.BombAndBombCombinationRange);
                    return true;
                case CellCombinationType.WandAndWand:
                    for (int i = 0; i < _row; i++)
                    {
                        for (int j = 0; j < _column; j++)
                        {
                            Block block = _blockArray[i, j];
                            Cell cell = _cellArray[i, j];
                            if (block.BlockType == BlockType.None || cell == null)
                            {
                                continue;
                            }
                            RemoveCellProcess(i, j);
                        }
                    }
                
                    return true;
                case CellCombinationType.None:
                default:
                    return false;
            }
        }

        private List<Cell> GetSameImageTypeCellList(CellImageType cellImageType)
        {
            List<Cell> list = new();
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Cell cell = _cellArray[i, j];
                    if (cell == null || cell.CellType != CellType.Normal || cell.CellImageType != cellImageType)
                    {
                        continue;
                    }

                    list.Add(cell);
                }
            }

            return list;
        }

        private void ActivateRocketAndBombCombination(int centerIndex, bool isUp)
        {
            for (int i = centerIndex - 1; i <= centerIndex + 1; i++)
            {
                if (i < 0 || i >= (isUp ? _row : _column))
                {
                    continue;
                }

                int count = isUp ? _column : _row;
                for (int j = 0; j < count; j++)
                {
                    Block block = isUp ? _blockArray[i, j] : _blockArray[j, i];
                    Cell cell = isUp ? _cellArray[i, j] : _cellArray[j, i];
                    if (block.BlockType == BlockType.None || cell == null)
                    {
                        continue;
                    }
                    
                    switch(cell.CellType)
                    {
                        case CellType.Generator:
                        case CellType.Obstacle:
                        case CellType.Normal:
                            RemoveCellProcess(cell.Row, cell.Column);
                            break;
                        case CellType.Rocket:
                        case CellType.Wand:
                        case CellType.Bomb:
                            RemoveCellProcess(cell.Row, cell.Column);
                            // TryActivateCellProperty(cell);
                            break;
                    }
                }
            }
        }

        private async UniTask Swap(Cell cellA, Cell cellB)
        {
            int cellARow = cellA.Row;
            int cellAColumn = cellA.Column;
            _cellArray[cellARow, cellAColumn] = cellB;

            int cellBRow = cellB.Row;
            int cellBColumn = cellB.Column;
            _cellArray[cellBRow, cellBColumn] = cellA;

            Vector3 cellAPosition = cellA.Position;
            Vector3 cellBPosition = cellB.Position;
         
            cellA.Swap(cellBPosition, cellBRow, cellBColumn);
            cellB.Swap(cellAPosition, cellARow, cellAColumn);
            await UniTask.WaitForSeconds(Const.SwapAnimationDuration);
        }
        
        private MatchedSameImageCellInfo? GetMatchedSameImageCellInfo(Cell cell)
        {
            int removableMatchedCellCount = Const.RemovableMatchedCellCount;
            var verticalSameImageCellList = new List<Cell>(removableMatchedCellCount);
            var horizontalSameImageCellList = new List<Cell>(removableMatchedCellCount);
            
            int row = cell.Row;
            int column = cell.Column;
            AddSameImageCell(row, column, cell.CellImageType, ref verticalSameImageCellList, ref horizontalSameImageCellList);

            int verticalSameImageCellListCount = verticalSameImageCellList.Count;
            int horizontalSameImageCellListCount = horizontalSameImageCellList.Count;

            bool isRemovableVerticalLine = verticalSameImageCellListCount >= removableMatchedCellCount;
            bool isRemovableHorizontalLine = horizontalSameImageCellListCount >= removableMatchedCellCount;
            // Debug.Log($"vertical {verticalSameImageCellListCount} / horizontal {horizontalSameImageCellListCount}");
            //조합(T, L, 십자가.. 5)폭탄 형태인 경우
            if (isRemovableVerticalLine && isRemovableHorizontalLine)
            {
                MatchedSameImageCellInfo matchedSameImageCellInfo = new MatchedSameImageCellInfo();
                matchedSameImageCellInfo.cellMatchedType = CellMatchedType.Five_Shape;
                matchedSameImageCellInfo.cellList =
                    new List<Cell>(verticalSameImageCellListCount + horizontalSameImageCellListCount);
                matchedSameImageCellInfo.cellList.AddRange(verticalSameImageCellList);
                matchedSameImageCellInfo.cellList.AddRange(horizontalSameImageCellList);
                return matchedSameImageCellInfo;
            }
            
            if (isRemovableVerticalLine)
            {
                return CreateMatchedSameImageCellInfo(verticalSameImageCellListCount, true, verticalSameImageCellList);
            }

            if (isRemovableHorizontalLine)
            {
                return CreateMatchedSameImageCellInfo(horizontalSameImageCellListCount, false,
                    horizontalSameImageCellList);
            }

            return null;
        }

        private CellMatchedType GetCellMatchedType(int count, bool isVertical)
        {
            switch (count)
            {
                case (int) CellMatchedType.Three:
                    return CellMatchedType.Three;
                case (int) CellMatchedType.Four:
                    return isVertical ? CellMatchedType.Vertical_Four : CellMatchedType.Horizontal_Four;
                case (int) CellMatchedType.Five:
                    return CellMatchedType.Five;
                default:
                    return CellMatchedType.None;
            }
        }

        private MatchedSameImageCellInfo CreateMatchedSameImageCellInfo(int count, bool isVertical, List<Cell> cellList)
        {
            return new MatchedSameImageCellInfo
            {
                cellMatchedType = GetCellMatchedType(count, isVertical),
                cellList = cellList
            };
        }

        private void AddSameImageCell(int row, int column, CellImageType cellImageType,
            ref List<Cell> verticalSameImageCellList, ref List<Cell> horizontalSameImageCellList)
        {
            if (IsOutOfBoardRange(row, column))
            {
                return;
            }

            Block block = _blockArray[row, column];
            if (block.BlockType == BlockType.None)
            {
                return;
            }

            Cell cell = _cellArray[row, column];
            if (cell == null)
            {
                Debug.LogError($"cell is null {row} / {column}");
                return;
            }

            if (cell.IsSameColorType(cellImageType))
            {
                verticalSameImageCellList.Add(cell);
                horizontalSameImageCellList.Add(cell);
                AddSameImageCellInDirection(row, column, 1, 0, cellImageType, ref verticalSameImageCellList,
                    ref horizontalSameImageCellList);
                AddSameImageCellInDirection(row, column, -1, 0, cellImageType, ref verticalSameImageCellList,
                    ref horizontalSameImageCellList);
                AddSameImageCellInDirection(row, column, 0, 1, cellImageType, ref verticalSameImageCellList,
                    ref horizontalSameImageCellList);
                AddSameImageCellInDirection(row, column, 0, -1, cellImageType, ref verticalSameImageCellList,
                    ref horizontalSameImageCellList);
            }
        }

        private void AddSameImageCellInDirection(int row, int column, int rowDir, int columnDir,
            CellImageType cellImageType, ref List<Cell> verticalSameImageCellList,
            ref List<Cell> horizontalSameImageCellList)
        {
            int currentRow = row + rowDir;
            int currentColumn = column + columnDir;
            if (IsOutOfBoardRange(currentRow, currentColumn))
            {
                return;
            }

            Block block = _blockArray[currentRow, currentColumn];
            if (block.BlockType == BlockType.None)
            {
                return;
            }

            Cell cell = _cellArray[currentRow, currentColumn];
            if (verticalSameImageCellList.Contains(cell) || horizontalSameImageCellList.Contains(cell))
            {
                return;
            }

            if (cell == null || cell.CellType != CellType.Normal)
            {
                // Debug.LogWarning($"cell {currentRow} / {currentColumn}");
                return;
            }

            if (cell.IsSameColorType(cellImageType))
            {
                if (rowDir != 0)
                {
                    verticalSameImageCellList.Add(cell);
                }

                if (columnDir != 0)
                {
                    horizontalSameImageCellList.Add(cell);
                }

                AddSameImageCellInDirection(currentRow, currentColumn, rowDir, columnDir, cellImageType,
                    ref verticalSameImageCellList, ref horizontalSameImageCellList);
            }
        }

        private bool IsOutOfBoardRange(Vector2 inputPosition)
        {
            Vector2 size = _blockArray[0, 0].BlockSize;
            Vector2 leftTopPosition = _blockArray[0, 0].Position + new Vector3(-size.x * 0.5f, -size.y * 0.5f);
            Vector2 rightBottomPosition = _blockArray[_row - 1, _column - 1].Position +
                                          new Vector3(size.x * 0.5f, size.y * 0.5f);
            if ((leftTopPosition.x <= inputPosition.x) && (rightBottomPosition.x >= inputPosition.x) &&
                (leftTopPosition.y <= inputPosition.y) && (rightBottomPosition.y >= inputPosition.y))
            {
                return true;
            }

            return false;
        }

        private bool IsOutOfBoardRange(int row, int column)
        {
            if (row < 0 || row >= _row || column < 0 || column >= _column)
            {
                return true;
            }

            return false;
        }
        
        private bool TryFindOrSpawnCellForEmptyBlock(int row, int column, int columnDir, ref List<Vector3> cellMovePositionList, ref Cell cell, ref bool isSpawn)
        {
            int targetColumn = column + columnDir;
            if (targetColumn < 0 || targetColumn >= _column)
            {
                return false;
            }

            for (int k = row + 1; k <= _row; k++)
            {
                // 마지막 행에 도달했을 경우 새 셀 생성 및 스폰
                if (k == _row)
                {
                    int length = Enum.GetNames(typeof(CellImageType)).Length - 1;
                    int select = Random.Range(0, length);
                    Vector3 spawnPosition = _blockArray[k - 1, targetColumn].Position + new Vector3(0, 2.5f, 0);
                    Cell newCell = CreateCell(row, column, CellType.Normal, ObstacleCellType.None,
                        (CellImageType)select);
                    newCell.CreateCellBehaviour(spawnPosition, _boardContainerTransform);
                    newCell.CellBehaviour.UpdatePosition(spawnPosition);
                    cell = newCell;
                    _cellArray[row, column] = newCell;
                    isSpawn = true;
                    return true;
                }

                Block block = _blockArray[k, targetColumn];
        
                // 비어있는 블록이면 실패 처리
                if (block.BlockType == BlockType.None)
                {
                    return false;
                }

                Cell targetCell = _cellArray[k, targetColumn];
                // 타겟 셀을 찾았을 경우
                if (targetCell != null)
                {
                    _cellArray[row, column] = targetCell;
                    _cellArray[k, targetColumn] = null;
                    targetCell.UpdateRowAndColumn(row, column);
                    cell = targetCell;
                    return true;
                }

                // 이동 경로 추가
                cellMovePositionList.Add(block.Position);
            }

            return false;
        }

        private Block FindBlockWithoutCell(int row, int column)
        {
            if (IsOutOfBoardRange(row, column))
            {
                return null;
            }
            
            Block block = _blockArray[row, column];
            Cell cell = _cellArray[row, column];
            if (block.BlockType == BlockType.None || cell != null)
            {
                return null;
            }

            return block;
        }

        private Cell TryGetCell(List<Cell> cellList, Vector2 position)
        {
            int count = cellList.Count;
            for (int j = 0; j < count; j++)
            {
                Cell cell = cellList[j];
                if (cell == null)
                {
                    continue;
                }

                if (IsContainCellBoundary(cell, position))
                {
                    return cell;
                }
            }

            return null;
        }

        private bool IsContainCellBoundary(Cell cell, Vector3 position)
        {
            Vector2 half = cell.Size * 0.5f;
            float x1 = cell.Position.x - half.x;
            float x2 = cell.Position.x + half.x;
            float y1 = cell.Position.y - half.y;
            float y2 = cell.Position.y + half.y;

            if ((x1 < position.x && x2 > position.x) &&
                (y1 < position.y && y2 > position.y))
            {
                return true;
            }

            return false;
        }

        private Cell TryGetCell(Vector2 position)
        {
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Cell cell = _cellArray[i, j];
                    if (cell == null)
                    {
                        continue;
                    }

                    if (IsContainCellBoundary(cell, position))
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        private List<Cell> GetNeighborCellList(Cell cell)
        {
            _neighborCellList.Clear();
            int row = cell.Row;
            int column = cell.Column;

            _neighborCellList.Capacity = 4;
            if (row > 0)
            {
                var topCell = _cellArray[row - 1, column];
                _neighborCellList.Add(topCell);
            }

            if (row < _row - 1)
            {
                var bottomCell = _cellArray[row + 1, column];
                _neighborCellList.Add(bottomCell);
            }

            if (column > 0)
            {
                var leftCell = _cellArray[row, column - 1];
                _neighborCellList.Add(leftCell);
            }

            if (column < _column - 1)
            {
                var rightCell = _cellArray[row, column + 1];
                _neighborCellList.Add(rightCell);
            }

            return _neighborCellList;
        }

        private void CreateBlockArray(int row, int column, BoardInfoData[,] boardInfoDataArray)
        {
            _blockArray = new Block[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    BoardInfoData boardInfoData = boardInfoDataArray[i, j];
                    if (boardInfoData == null)
                    {
                        Debug.Log($"board {i} {j}");
                        continue;
                    }

                    BlockType blockType = boardInfoData.BlockType;
                    Block block = new Block(i, j, blockType);
                    _blockArray[i, j] = block;
                }
            }
        }

        private void CreateCellArray(int row, int column, BoardInfoData[,] boardInfoDataArray)
        {
            _cellArray = new Cell[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    BoardInfoData boardInfoData = boardInfoDataArray[i, j];
                    if (boardInfoData == null)
                    {
                        continue;
                    }

                    _cellArray[i, j] = CreateCell(i, j, boardInfoData.CellType, boardInfoData.ObstacleCellType,
                        boardInfoData.CellImageType);
                }
            }
        }

        private Cell CreateCell(int row, int column, CellType cellType, ObstacleCellType obstacleCellType, CellImageType cellImageType)
        {
            Cell cell = new Cell(row, column, cellType, obstacleCellType, cellImageType);
            return cell;
        }

        public async UniTask BuildAsync(Vector2 centerPosition, GameObject blockPrefab, Transform container = null)
        {
            UpdateBoardState(BoardState.Building);
            _boardContainerTransform = container;
            
            CreateBlockBehaviour(centerPosition, blockPrefab, container);
            CreateCellBehaviour(container);
            
            UpdateBoardState(BoardState.CompleteBuild);
        }
        
        public void Build(Vector2 centerPosition, GameObject blockPrefab, Transform parent = null)
        {
            CreateBlockBehaviour(centerPosition, blockPrefab, parent);
            CreateCellBehaviour(parent);
        }

        public async UniTask BuildAfterProcess()
        {
            while (await CheckMatchingCell())
            {
                await PostSwapProcess();
            }
        }

        private void CreateBlockBehaviour(Vector2 centerPosition, GameObject blockPrefab, Transform parent = null)
        {
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    block.CreateBlockBehaviour(blockPrefab, centerPosition, _row, _column, (i * _column + j) % 2 == 0, parent);
                }
            }
        }

        private void CreateCellBehaviour(Transform parent = null)
        {
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    if (block.BlockType == BlockType.None)
                    {
                        continue;
                    }
                    
                    Cell cell = _cellArray[i, j];
                    cell.CreateCellBehaviour(block.Position, parent);
                }
            }
        }

        private List<Cell> GetCellList()
        {
            List<Cell> list = new();
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Cell cell = _cellArray[i, j];
                    if (cell == null)
                    {
                        continue;
                    }
                    
                    list.Add(cell);
                }
            }

            return list;
        }

        #region Player_Item

        private InGameItemType _pendingInGameItemType;
        
        public void SetPendingUseInGameItemType(InGameItemType inGameItemType)
        {
            _pendingInGameItemType = inGameItemType;
            UpdateBoardState(BoardState.PendingUseInGameItem);
        }

        private async UniTask ExecuteInGameItem()
        {
            UpdateBoardState(BoardState.UseItem);
            GameManager.onUsedInGameItemAction?.Invoke(_pendingInGameItemType);

            Debug.Log("ExecuteInGameItem : " + _pendingInGameItemType);
            switch (_pendingInGameItemType)
            {
                case InGameItemType.Shuffle:
                    await ExecuteShuffleCell();
                    break;
                case InGameItemType.Hammer:
                    ExecuteHammer();
                    break;
                case InGameItemType.VerticalRocket:
                    int row = _selectedCell.Row;
                    int column = _selectedCell.Column;
                    await UniTask.WhenAll(ActivateRocketProcessAsync(row, column, row, _row, true, true),
                        ActivateRocketProcessAsync(row, column, row, -1, false, true));
                    break;
                case InGameItemType.HorizontalRocket:
                    row = _selectedCell.Row;
                    column = _selectedCell.Column;
                    await UniTask.WhenAll(ActivateRocketProcessAsync(row, column, column, _column, true, false),
                        ActivateRocketProcessAsync(row, column, column, -1, false, false));
                    break;
            }
            
            do
            {
                await PostSwapProcess();
            } while (await CheckMatchingCell());
            
            _pendingInGameItemType = InGameItemType.None;
            ResetDrag();
            UpdateBoardState(BoardState.Ready);
        }

        private void ExecuteHammer()
        {
            RemoveCellProcess(_selectedCell.Row, _selectedCell.Column);
        }

        private async UniTask ExecuteShuffleCell()
        {
            List<Cell> cellList = GetCellList();
            cellList = CollectionHelper.Shuffle(cellList);
            
            List<UniTask> moveTaskList = new List<UniTask>(cellList.Count);
            int index = 0;
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    if (_cellArray[i, j] == null)
                    {
                        continue;
                    }
                    _cellArray[i, j] = cellList[index];
                    cellList[index].UpdateRowAndColumn(i, j);
                    index++;
                }
            }
            
            cellList.ForEach(cell =>
            {
                int centerRow = Mathf.FloorToInt(_row * 0.5f);
                int centerColumn = Mathf.FloorToInt(_column * 0.5f);
                Vector3 centerPosition = _blockArray[centerRow, centerColumn].Position;
                moveTaskList.Add(cell.CellBehaviour.MoveAsync(centerPosition));
            });

            await UniTask.WhenAll(moveTaskList);
            await UniTask.WaitForSeconds(1f);
            
            moveTaskList.Clear();
            cellList.ForEach(cell =>
            {
                Vector3 originPosition = _blockArray[cell.Row, cell.Column].Position;
                moveTaskList.Add(cell.CellBehaviour.MoveAsync(originPosition));
            });

            await UniTask.WhenAll(moveTaskList);
        }

        #endregion

        public void UpdateBoardState(BoardState state) => _boardState = state;
        public BoardState GetBoardState() => _boardState;
        public bool IsBoardState(BoardState state) => _boardState == state;
    }
}



























