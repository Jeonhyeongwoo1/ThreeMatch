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
        
        public Board(int[,] boardInfoArray)
        {
            _row = boardInfoArray.GetLength(0);
            _column = boardInfoArray.GetLength(1);
            CreateBlock(_row, _column, boardInfoArray);
            CreateCell(_row, _column);
            
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

        private void OnPointerUp(Vector2 endPosition)
        {
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
            var list = new List<Cell>(Const.RemovableMatchedCellCount);
            int row = cell.Row;
            int column = cell.Column;
            AddSameImageCell(row, column, cell.CellImageType, ref list);
            return list;
        }

        private void AddSameImageCell(int row, int column, CellImageType cellImageType, ref List<Cell> sameImageCellList)
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
            if (sameImageCellList.Contains(cell))
            {
                return;
            }
            
            if (cell.IsSameColorType(cellImageType))
            {
                Debug.Log(cell.Name);
                sameImageCellList.Add(cell);
                AddSameImageCellInDirection(row, column, 1, 0, cellImageType, ref sameImageCellList);
                AddSameImageCellInDirection(row, column, -1, 0, cellImageType, ref sameImageCellList);
                AddSameImageCellInDirection(row, column, 0, 1, cellImageType, ref sameImageCellList);
                AddSameImageCellInDirection(row, column, 0, -1, cellImageType, ref sameImageCellList);
            }
        }

        private void AddSameImageCellInDirection(int row, int column, int rowDir, int columnDir, CellImageType cellImageType, ref List<Cell> sameImageCellList)
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
            if (sameImageCellList.Contains(cell))
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
                Debug.Log(cell.Name);
                sameImageCellList.Add(cell);
                AddSameImageCellInDirection(currentRow, currentColumn, rowDir, columnDir, cellImageType, ref sameImageCellList);
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
            _isSwapping = false;
            _selectedCell = null;
            _neighborCellList.Clear();
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

        private void CreateBlock(int row, int column, int[,] boardInfoArray)
        {
            _blockArray = new Block[row, column];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    BlockType blockType = (BlockType) boardInfoArray[i, j];
                    Block block = new Block(i, j, blockType);
                    _blockArray[i, j] = block;
                    Debug.Log($"block row {i}, col {j}");
                }
            }
        }

        private void CreateCell(int row, int column)
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

                    int length = Enum.GetNames(typeof(CellImageType)).Length - 1;
                    int random = Random.Range(0, length);
                    Cell cell = new Cell(i, j, (CellImageType)random);
                    _cellArray[i, j] = cell;
                    Debug.Log($"cell row {i}, col {j}");
                }
            }
        }

        public void Build(Vector2 centerPosition, GameObject blockPrefab, GameObject cellPrefab)
        {
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
                    cell?.CreateCellBehaviour(cellPrefab, block.Position);
                }
            }
        }
    }
}




























