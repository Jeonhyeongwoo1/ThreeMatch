using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class Cell
    {
        public Vector2 Size => _cellBehaviour.Size;
        public Vector2 Position => _cellBehaviour.transform.position;
        public CellBehaviour CellBehaviour => _cellBehaviour;
        public CellImageType CellImageType => _cellImageType;
        public string Name => _cellBehaviour.name;
        public int Row => _row;
        public int Column => _column;
        
        private int _column;
        private int _row;
        private CellBehaviour _cellBehaviour;
        private CellImageType _cellImageType;
        
        public Cell(int row, int column, CellImageType cellImageType)
        {
            _column = column;
            _row = row;
            _cellImageType = cellImageType;
        }

        public void CreateCellBehaviour(GameObject prefab)
        {
            var obj = Object.Instantiate(prefab);
            obj.name = $"Cell {_row} / {_column}";
            _cellBehaviour = obj.GetOrAddComponent<CellBehaviour>();
            _cellBehaviour.Initialize((int)_cellImageType);
        }

        public void Swap(Vector3 position, int row, int column)
        {
            Debug.Log($"Swap {_cellBehaviour.name} . {position} / {row} / {column}");
            _row = row;
            _column = column;
            CellBehaviour.name = $"Cell {_row} / {_column}";
            _cellBehaviour.Swap(position);
        }

        public void UpdateRowAndColumn(int row, int column)
        {
            _row = row;
            _column = column;
            CellBehaviour.name = $"Cell {_row} / {_column}";
        }

        public bool IsSameColorType(CellImageType cellImageType)
        {
            return cellImageType == _cellImageType;
        }
        
        public async UniTask PostSwapProcess(List<Vector3> movePositionList)
        {
            _cellBehaviour.Move(movePositionList).Forget();
            await UniTask.WaitForSeconds(Const.CellMoveAnimationDuration);
        }

        public void RemoveCell()
        {
            _cellBehaviour.gameObject.SetActive(false);
        }
    }
}