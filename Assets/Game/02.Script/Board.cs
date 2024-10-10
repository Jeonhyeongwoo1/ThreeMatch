using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Manager;
using UnityEditor.VersionControl;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThreeMatch.InGame
{
    public class Board
    {
        [Serializable]
        public struct CellMovementInfo
        {
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
        private int _row;
        private int _column;

        private List<Cell> _neighborCellList = new ();
        private Cell _selectedCell;
        private bool _isSwapping;
        private GameObject _blockPrefab;
        private GameObject _cellPrefab;
        
        public Board(int[,] boardInfoArray)
        {
            _row = boardInfoArray.GetLength(0);
            _column = boardInfoArray.GetLength(1);
            CreateBlockArray(_row, _column, boardInfoArray);
            CreateCellArray(_row, _column);
            
            AddEvents();
        }

        private void AddEvents()
        {
            InputManager.OnPointerDownAction += OnPointerDown;
            // InputManager.OnBeginDragAction += OnBeginDrag;
            InputManager.OnDragAction += OnDrag;
            // InputManager.OnEndDragAction += OnEndDrag;
            InputManager.OnPointerUpAction += OnPointerUp;
        }

        private void OnPointerDown(Vector2 beginPosition)
        {
            if (_selectedCell != null || _isSwapping)
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
            cell.CellBehaviour.OnPointerDown(beginPosition);
        }

        private async void OnDrag(Vector2 dragPosition)
        {
            if (_selectedCell == null || _isSwapping)
            {
                return;
            }

            Cell cell = TryGetCell(_neighborCellList, dragPosition);
            if (cell == null || _selectedCell == cell)
            {
                return;
            }

            _isSwapping = true;
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
            ResetDrag();
        }

        private void OnPointerUp(Vector2 endPosition)
        {
            if (_selectedCell != null && !_isSwapping)
            {
                ResetDrag();
            }
        }

        private void ResetDrag()
        {
            _isSwapping = false;
            _selectedCell = null;
            _neighborCellList.Clear();
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
        
        private async UniTask PostSwapProcess()
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
                            bool isSuccess = TrySpawnOrMoveCell(i, j, columnDir, ref cellMovePositionList, ref spawnOrMoveCell);
                            if (isSuccess)
                            {
                                CellMovementInfo info = new CellMovementInfo();
                                info.movePositionList = cellMovePositionList;
                                info.cell = spawnOrMoveCell;
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

            int maxCount = 0;
            foreach (KeyValuePair<int, List<CellMovementInfo>> cellMovementInfo in cellMovementInfoDict)
            {
                int key = cellMovementInfo.Key;
                List<CellMovementInfo> cellMovementInfoList = cellMovementInfo.Value;

                if (cellMovementInfoList.Count > maxCount)
                {
                    maxCount = cellMovementInfoList.Count;
                }
                
                //낮은 것부터 차례대로 이동 시킨다.
                MoveCell(cellMovementInfoList, key).Forget();
            }

            float waitTime = maxCount * Const.CellMoveAnimationDuration;
            // Debug.Log($"waitTime {waitTime} / {maxCount} / {Const.CellMoveAnimationDuration}");
            await UniTask.WaitForSeconds(waitTime);
        }

        private async UniTask MoveCell(List<CellMovementInfo> cellMovementInfoList, int key)
        {
            // cellMovementInfoList.Sort((a, b)=> a.cell.Column.CompareTo(b.cell.Column));
            foreach (CellMovementInfo info in cellMovementInfoList)
            {
                // Debug.Log($"key {key} {info.cell.Name}  movelist count {info.movePositionList.Count}");
                await info.cell.PostSwapProcess(info.movePositionList);
            }
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
                        _cellArray[cell.Row, cell.Column] = null;
                        cell.RemoveCell();
                    }

                    await UniTask.WaitForSeconds(Const.CellRemoveAnimationDuration);
                    return true;
                case CellMatchedType.Vertical_Four:
                case CellMatchedType.Horizontal_Four:
                case CellMatchedType.Five:
                case CellMatchedType.Five_Shape:
                    Debug.LogWarning("Match :" + cellMatchedType);
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
                            cell.RemoveCell();
                        });
                        
                        moveTaskList.Add(moveTask);
                    }
                    await UniTask.WhenAll(moveTaskList);
                    
                    firstCell.SetCellTypeFromMatch(cellMatchedType);
                    return true;
                case CellMatchedType.None:
                default:
                    return false;
            }
        }
 
        private async UniTask ActivateRocketProcessAsync(int row, int column, int startIndex, int endIndex, bool isPositive,bool isUpDir, Cell activateCell)
        {
            int step = isPositive ? 1 : -1;
            UniTask cellTask = UniTask.CompletedTask;
            for (int i = startIndex; i != endIndex; i += step)
            {
                Block block = isUpDir ? _blockArray[i, column] : _blockArray[row, i];
                Cell cell = isUpDir ? _cellArray[i, column] : _cellArray[row, i];

                //연출이 들어가야함. 추후 수정
                // UniTask moveTask = activateCell.CellBehaviour.MoveAsync(block.Position);
                UniTask moveTask = UniTask.WaitForSeconds(0.2f);
                if (block.BlockType == BlockType.None || cell == null)
                {
                    await moveTask;
                    continue;
                }

                switch (cell.CellType)
                {
                    case CellType.Normal:
                        cell.RemoveCell();
                        break;
                    case CellType.Rocket:
                    case CellType.Wand:
                    case CellType.Bomb:
                        if (activateCell == cell)
                        {
                            cell.RemoveCell();
                        }
                        else
                        {
                            cellTask = TryActivateCellProperty(cell);
                        }                        
                        break;
                }
                
                if (isUpDir)
                {
                    _cellArray[i, column] = null;
                }
                else
                {
                    _cellArray[row, i] = null;
                }

                await moveTask;
            }

            await cellTask;
        }

        private async UniTask ActivateBombProcessAsync(int row, int column, Cell activateCell)
        {
            UniTask cellTask = UniTask.CompletedTask;
            for (int i = row - 1; i <= row + 1; i++)
            {
                for (int j = column - 1; j <= column + 1; j++)
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
                        case CellType.Normal:
                            cell.RemoveCell();
                            break;
                        case CellType.Rocket:
                        case CellType.Wand:
                        case CellType.Bomb:
                            if (activateCell == cell)
                            {
                                cell.RemoveCell();
                            }
                            else
                            {
                                cellTask = TryActivateCellProperty(cell);
                            }                        
                            break;
                    }
                    
                    _cellArray[i, j] = null;
                }
            }

            await UniTask.WaitForSeconds(0.5f);
            await cellTask;
        }

        private async UniTask ActivateWandProcessAsync(Cell activateCell)
        {
            CellImageType cellImageType = activateCell.CellImageType;
            Debug.Log($"ActivateWandProcessAsync {cellImageType}");
            List<Cell> sameImageTypeCellList = new List<Cell>();
            UniTask cellTask = UniTask.CompletedTask;
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Cell cell = _cellArray[i, j];
                    if (cell == null || cell.CellType != CellType.Normal || cell.CellImageType != cellImageType)
                    {
                        continue;
                    }
                    
                    sameImageTypeCellList.Add(cell);
                }
            }
            
            _cellArray[activateCell.Row, activateCell.Column] = null;
            activateCell.RemoveCell();
            foreach (Cell cell in sameImageTypeCellList)
            {
                _cellArray[cell.Row, cell.Column] = null;
                switch (cell.CellType)
                {
                    case CellType.Normal:
                        cell.RemoveCell();
                        break;
                    case CellType.Rocket:
                    case CellType.Wand:
                    case CellType.Bomb:
                        if (activateCell == cell)
                        {
                            cell.RemoveCell();
                        }
                        else
                        {
                            cellTask = TryActivateCellProperty(cell);
                        }
                        break;
                }
            }
            
            await UniTask.WaitForSeconds(0.5f);
            await cellTask;
        }

        private async UniTask<bool> TryActivateCellProperty(Cell activateCell)
        {
            int row = activateCell.Row;
            int column = activateCell.Column;
            switch (activateCell.CellType)
            {
                case CellType.Rocket:
                    activateCell.ActivateRocket();
                    switch (activateCell.CellMatchedType)
                    {
                        case CellMatchedType.Vertical_Four:
                            UniTask upTask = ActivateRocketProcessAsync(row, column, row, _row, true, true, activateCell);
                            UniTask downTask = ActivateRocketProcessAsync(row, column, row, -1, false, true, activateCell);
                            await UniTask.WhenAll(upTask, downTask);
                            break;
                        case CellMatchedType.Horizontal_Four:
                            UniTask rightTask = ActivateRocketProcessAsync(row, column, column, _column, true, false, activateCell);
                            UniTask leftTask = ActivateRocketProcessAsync(row, column, column, -1, false, false, activateCell);
                            await UniTask.WhenAll(rightTask, leftTask);
                            break;
                    }

                    return true;
                case CellType.Wand:
                    await ActivateWandProcessAsync(activateCell);
                    return true;
                case CellType.Bomb:
                    await ActivateBombProcessAsync(row, column, activateCell);
                    return true;
                case CellType.None:
                case CellType.Normal:
                default:
                    return false;
            }
        }
        
        private async UniTask<bool> TrySwap(Cell cellA, Cell cellB)
        {
            Swap(cellA, cellB);

            await UniTask.WaitForSeconds(Const.SwapAnimationDuration);

            Debug.Log($"cellA {cellA.CellType} / CellB {cellB.CellType}");
            //두 개가 일반형인 경우
            if (cellA.CellType == CellType.Normal && cellB.CellType == CellType.Normal)
            {
                MatchedSameImageCellInfo? cellAMatchedSameImageCellInfo = GetMatchedSameImageCellInfo(cellA);
                MatchedSameImageCellInfo? cellBMatchedSameImageCellInfo = GetMatchedSameImageCellInfo(cellB);

                await UniTask.WhenAll(HandleMatchedCell(cellAMatchedSameImageCellInfo),
                    HandleMatchedCell(cellBMatchedSameImageCellInfo));
            
                if (!cellAMatchedSameImageCellInfo.HasValue && !cellBMatchedSameImageCellInfo.HasValue)
                {
                    Swap(cellA, cellB);
                    await UniTask.WaitForSeconds(Const.SwapAnimationDuration);
                    return false; 
                }

                //모두 없는 경우
                // if (cellAMatchedSameImageCellInfo != null &&
                //     cellBMatchedSameImageCellInfo != null &&
                //     cellAMatchedSameImageCellInfo.Value.cellList.Count < Const.RemovableMatchedCellCount &&
                //     cellBMatchedSameImageCellInfo.Value.cellList.Count < Const.RemovableMatchedCellCount)
                // {
                //     Swap(cellA, cellB);
                //     await UniTask.WaitForSeconds(Const.SwapAnimationDuration);
                //     return false;
                // }
            
                return true;
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

            Debug.LogError("Swap error");
            Swap(cellA, cellB);
            await UniTask.WaitForSeconds(Const.SwapAnimationDuration);
            return false;
        }

        private void Swap(Cell cellA, Cell cellB)
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
            
            // Debug.Log($"vertical {verticalSameImageCellListCount} / horizontal {horizontalSameImageCellListCount}");
            //조합(T, L, 십자가.. 5)폭탄 형태인 경우
            if (verticalSameImageCellListCount >= removableMatchedCellCount &&
                horizontalSameImageCellListCount >= removableMatchedCellCount)
            {
                MatchedSameImageCellInfo matchedSameImageCellInfo = new MatchedSameImageCellInfo();
                matchedSameImageCellInfo.cellMatchedType = CellMatchedType.Five_Shape;
                matchedSameImageCellInfo.cellList = new List<Cell>(removableMatchedCellCount * 2);
                matchedSameImageCellInfo.cellList.AddRange(verticalSameImageCellList);
                matchedSameImageCellInfo.cellList.AddRange(horizontalSameImageCellList);
                return matchedSameImageCellInfo;
            }
            
            if (verticalSameImageCellListCount >= removableMatchedCellCount)
            {
                CellMatchedType cellMatchedType = CellMatchedType.None;
                if (verticalSameImageCellListCount == (int)CellMatchedType.Three)
                {
                    cellMatchedType = CellMatchedType.Three;
                }
                else if (verticalSameImageCellListCount == (int)CellMatchedType.Four)
                {
                    cellMatchedType = CellMatchedType.Vertical_Four;
                }
                else if(verticalSameImageCellListCount == (int)CellMatchedType.Five)
                {
                    cellMatchedType = CellMatchedType.Five_OneLine;
                }
                
                MatchedSameImageCellInfo matchedSameImageCellInfo = new MatchedSameImageCellInfo();
                matchedSameImageCellInfo.cellMatchedType = cellMatchedType;
                matchedSameImageCellInfo.cellList = verticalSameImageCellList;
                return matchedSameImageCellInfo;
            }

            if (horizontalSameImageCellListCount >= removableMatchedCellCount)
            {
                CellMatchedType cellMatchedType = CellMatchedType.None;
                if (horizontalSameImageCellListCount == (int)CellMatchedType.Three)
                {
                    cellMatchedType = CellMatchedType.Three;
                }
                else if (horizontalSameImageCellListCount == (int)CellMatchedType.Four)
                {
                    cellMatchedType = CellMatchedType.Horizontal_Four;
                }
                else if(horizontalSameImageCellListCount == (int)CellMatchedType.Five)
                {
                    cellMatchedType = CellMatchedType.Five_OneLine;
                }
                
                MatchedSameImageCellInfo matchedSameImageCellInfo = new MatchedSameImageCellInfo();
                matchedSameImageCellInfo.cellMatchedType = cellMatchedType;
                matchedSameImageCellInfo.cellList = horizontalSameImageCellList;
                return matchedSameImageCellInfo;
            }

            return null;
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
                Debug.LogWarning($"cell {currentRow} / {currentColumn}");
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

        private bool IsOutOfBoardRange(int row, int column)
        {
            if (row < 0 || row >= _row || column < 0 || column >= _column)
            {
                return true;
            }

            return false;
        }

        
        private bool TrySpawnOrMoveCell(int row, int column, int columnDir, ref List<Vector3> cellMovePositionList, ref Cell cell)
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
                    Cell newCell = CreateCell(row, column);
                    newCell.CreateCellBehaviour(_cellPrefab);
                    Vector3 spawnPosition = _blockArray[k - 1, targetColumn].Position + new Vector3(0, 2.5f, 0);
                    newCell.CellBehaviour.UpdatePosition(spawnPosition);
                    cell = newCell;
                    _cellArray[row, column] = newCell;
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

                Vector2 half = cell.Size * 0.5f;
                float x1 = cell.Position.x - half.x;
                float x2 = cell.Position.x + half.x;
                float y1 = cell.Position.y - half.y;
                float y2 = cell.Position.y + half.y;

                if ((x1 < position.x && x2 > position.x) &&
                    (y1 < position.y && y2 > position.y))
                {
                    return cell;
                }
            }

            return null;
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

                    Vector2 half = cell.Size * 0.5f;
                    float x1 = cell.Position.x - half.x;
                    float x2 = cell.Position.x + half.x;
                    float y1 = cell.Position.y - half.y;
                    float y2 = cell.Position.y + half.y;

                    if ((x1 < position.x && x2 > position.x) &&
                        (y1 < position.y && y2 > position.y))
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        private List<Cell> GetNeighborCellList(Cell cell)
        {
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

        private void CreateBlockArray(int row, int column, int[,] boardInfoArray)
        {
            _blockArray = new Block[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    BlockType blockType = (BlockType) boardInfoArray[i, j];
                    Block block = new Block(i, j, blockType);
                    _blockArray[i, j] = block;
                    // Debug.Log($"block row {i}, col {j}");
                }
            }
        }

        private void CreateCellArray(int row, int column)
        {
            _cellArray = new Cell[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Block block = _blockArray[i, j];
                    if (block.BlockType == BlockType.None)
                    {
                        continue;
                    }

                    _cellArray[i, j] = CreateCell(i, j);
                    // Debug.Log($"cell row {i}, col {j}");
                }
            }
        }

        private Cell CreateCell(int row, int column)
        {
            int length = Enum.GetNames(typeof(CellImageType)).Length - 1;
            int random = Random.Range(0, length);
            Cell cell = new Cell(row, column, (CellImageType)random, CellType.Normal);

            return cell;
        }

        public void Build(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab)
        {
            _blockPrefab = blockPrefab;
            _cellPrefab = cellPrefab;
            
            CreateBlockBehaviour(centerPosition, blockPrefab);
            CreateCellBehaviour(cellPrefab);
            BuildAfterProcess();
        }

        private async void BuildAfterProcess()
        {
            while (await CheckMatchingCell())
            {
                await PostSwapProcess();
            }
        }

        private void CreateBlockBehaviour(Vector2 centerPosition, GameObject blockPrefab)
        {
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    block.CreateBlockBehaviour(blockPrefab, centerPosition, _row, _column, (i * _column + j) % 2 == 0);
                }
            }
        }

        private void CreateCellBehaviour(GameObject cellPrefab)
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
                    cell?.CreateCellBehaviour(cellPrefab);
                    cell?.CellBehaviour.UpdatePosition(block.Position);
                }
            }
        }
    }
}



























