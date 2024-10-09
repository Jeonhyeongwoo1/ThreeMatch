using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThreeMatch.InGame
{
    public class Board
    {
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
            if (_selectedCell != null)
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
                await PostSwapProcess(); 
            }
            
            _isSwapping = false;
            _selectedCell = null;
            _neighborCellList.Clear();
        }
        
        private void OnPointerUp(Vector2 endPosition)
        {
            _isSwapping = false;
            _selectedCell = null;
            _neighborCellList.Clear();
        }

        private async UniTask<bool> TrySwap(Cell cellA, Cell cellB)
        {
            Swap(cellA, cellB);

            await UniTask.WaitForSeconds(Const.SwapAnimationDuration);
            
            List<Cell> cellANeighborSameImageCellList = GetNeighborSameImageCellList(cellA);
            List<Cell> cellBNeighborSameImageCellList = GetNeighborSameImageCellList(cellB);
            if (cellANeighborSameImageCellList.Count >= Const.RemovableMatchedCellCount)
            {
                foreach (Cell cell in cellANeighborSameImageCellList)
                {
                    _cellArray[cell.Row, cell.Column] = null;
                    cell.RemoveCell();
                }
            }
            
            if (cellBNeighborSameImageCellList.Count >= Const.RemovableMatchedCellCount)
            {
                foreach (Cell cell in cellBNeighborSameImageCellList)
                {
                    _cellArray[cell.Row, cell.Column] = null;
                    cell.RemoveCell();
                }
            }

            //모두 없는 경우
            if (cellBNeighborSameImageCellList.Count < Const.RemovableMatchedCellCount &&
                cellANeighborSameImageCellList.Count < Const.RemovableMatchedCellCount)
            {
                Swap(cellA, cellB);
                await UniTask.WaitForSeconds(Const.SwapAnimationDuration);
                return false;
            }
            
            return true;
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
        
        private List<Cell> GetNeighborSameImageCellList(Cell cell)
        {
            var verticalSameImageCellList = new List<Cell>(Const.RemovableMatchedCellCount);
            var horizontalSameImageCellList = new List<Cell>(Const.RemovableMatchedCellCount);
            
            int row = cell.Row;
            int column = cell.Column;
            AddSameImageCell(row, column, cell.CellImageType, ref verticalSameImageCellList, ref horizontalSameImageCellList);

            var resultCellList = new List<Cell>();
            if (verticalSameImageCellList.Count >= Const.RemovableMatchedCellCount)
            {
                resultCellList.AddRange(verticalSameImageCellList);
            }

            if (horizontalSameImageCellList.Count >= Const.RemovableMatchedCellCount)
            {
                resultCellList.AddRange(horizontalSameImageCellList);
            }

            return resultCellList;
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

            if (cell == null)
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

        [Serializable]
        public struct CellMovementInfo
        {
            public Cell cell;
            public List<Vector3> movePositionList;
        }

        private void AddCellMovementInfo(Cell cell, List<Vector3> movePositionList, ref List<CellMovementInfo> cellMovementInfoList)
        {
            CellMovementInfo info = new CellMovementInfo();
            info.cell = cell;
            info.movePositionList = movePositionList;
            cellMovementInfoList.Add(info);
        }
        
        private bool TrySpawnAndMoveCell(int row, int column, int columnDir, ref List<Vector3> cellMovePositionList)
        {
            int targetColumn = column + columnDir;

            // 보드 범위를 벗어났는지 확인
            if (targetColumn < 0 || targetColumn >= _column)
            {
                cellMovePositionList.Clear();
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
                    newCell.PostSwapProcess(cellMovePositionList);
                    _cellArray[row, column] = newCell;
                    return true;
                }

                Block block = _blockArray[k, targetColumn];
        
                // 비어있는 블록이면 실패 처리
                if (block.BlockType == BlockType.None)
                {
                    cellMovePositionList.Clear();
                    Debug.Log($"{row} / {column} / {columnDir}");
                    return false;
                }

                Cell targetCell = _cellArray[k, targetColumn];
        
                // 타겟 셀을 찾았을 경우
                if (targetCell != null)
                {
                    _cellArray[row, column] = targetCell;
                    _cellArray[k, targetColumn] = null;
                    targetCell.UpdateRowAndColumn(row, column);
                    targetCell.PostSwapProcess(cellMovePositionList);
                    return true;
                }

                // 이동 경로 추가
                cellMovePositionList.Add(block.Position);
            }

            cellMovePositionList.Clear();
            return false;
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
                    List<CellMovementInfo> cellMovementInfoList = new();
                    //비어있는 블록을 기준으로 찾기
                    if (block.BlockType != BlockType.None && cell == null)
                    {
                        Debug.Log($"{i} / {j} / {block.BlockBehaviour.name}");
                        int[] direction = { 0, 1, -1 };
                        foreach (int columnDir in direction)
                        {
                            cellMovePositionList.Add(block.Position);
                            bool isSuccess = TrySpawnAndMoveCell(i, j, columnDir, ref cellMovePositionList);
                            if (isSuccess)
                            {
                                break;
                            }
                        }
                        
                        // for (int k = i + 1; k <= _row; k++)
                        // {
                        //     if (k == _row)
                        //     {
                        //         Cell newCell = CreateCell(i, j);
                        //         newCell.CreateCellBehaviour(_cellPrefab);
                        //         Vector3 spawnPosition = _blockArray[k - 1, j].Position +
                        //                                 new Vector3(0, 2.5f, 0);
                        //         newCell.CellBehaviour.UpdatePosition(spawnPosition);
                        //         newCell.PostSwapProcess(cellMovePositionList);
                        //         _cellArray[i, j] = newCell;
                        //         // AddCellMovementInfo(newCell, cellMovePositionList, ref cellMovementInfoList);
                        //         // cellMovementInfoDict.Add(j, cellMovementInfoList);
                        //         break;
                        //     }
                        //  
                        //     Block b = _blockArray[k, j];   
                        //     if (b.BlockType == BlockType.None)
                        //     {
                        //         break;
                        //     }
                        //
                        //     Cell c = _cellArray[k, j];
                        //     if (c != null)
                        //     {
                        //         _cellArray[i, j] = c;
                        //         _cellArray[k, j] = null;
                        //         c.UpdateRowAndColumn(i, j);
                        //         c.PostSwapProcess(cellMovePositionList);
                        //         // AddCellMovementInfo(c, cellMovePositionList, ref cellMovementInfoList);
                        //         // cellMovementInfoDict.Add(j, cellMovementInfoList);
                        //         break;
                        //     }
                        //     
                        //     cellMovePositionList.Add(b.Position);
                        // }
                        
                        cellMovePositionList.Clear();
                    }
                }
            }
            
            // foreach (var cellMovementInfo in cellMovementInfoDict)
            // {
            //     int key = cellMovementInfo.Key;
            //     List<CellMovementInfo> cellMovementInfoList = cellMovementInfo.Value;
            //     //낮은 것부터 차례대로 이동 시킨다.
            //     cellMovementInfoList.Sort((a, b)=> a.cell.Column.CompareTo(b.cell.Column));
            //     
            //     foreach (CellMovementInfo info in cellMovementInfoList)
            //     {
            //         info.cell.PostSwapProcess(info.movePositionList);
            //     }
            // }
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
            Cell cell = new Cell(row, column, (CellImageType)random);

            return cell;
        }

        public void Build(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab)
        {
            _blockPrefab = blockPrefab;
            _cellPrefab = cellPrefab;
            
            CreateBlockBehaviour(centerPosition, blockPrefab);
            CreateCellBehaviour(cellPrefab);
        }

        private void CreateBlockBehaviour(Vector2 centerPosition, GameObject blockPrefab)
        {
            int index = 0;
            for (int i = 0; i < _row; i++)
            {
                for (int j = 0; j < _column; j++)
                {
                    Block block = _blockArray[i, j];
                    block.CreateBlockBehaviour(blockPrefab, centerPosition, _row, _column, index % 2 == 0);
                    index++;
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




























