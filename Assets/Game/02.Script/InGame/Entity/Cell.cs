using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThreeMatch.InGame.Entity
{
    public class Cell
    {
        public Vector2 Size => _cellBehaviour.Size;
        public Vector2 Position => _cellBehaviour.transform.position;
        public CellBehaviour CellBehaviour => _cellBehaviour;
        public CellImageType CellImageType => _cellImageType;
        public CellType CellType => _cellType;
        public CellMatchedType CellMatchedType => _cellMatchedType;
        public int Row => _row;
        public int Column => _column;
        
        private int _column;
        private int _row;
        private CellBehaviour _cellBehaviour;
        private CellImageType _cellImageType;
        private CellMatchedType _cellMatchedType;
        private CellType _cellType;

        public Cell(int row, int column, CellType cellType, CellImageType cellImageType = CellImageType.None)
        {
            _column = column;
            _row = row;
            _cellType = cellType;
            _cellImageType = cellImageType;
            _cellMatchedType = CellMatchedType.None;
        }

        public void CreateCellBehaviour(GameObject prefab)
        {
            var obj = Object.Instantiate(prefab);
            obj.name = $"Cell {_row} / {_column}";
            _cellBehaviour = obj.GetOrAddComponent<CellBehaviour>();
            _cellBehaviour.Initialize(_cellType, _cellImageType);
        }

        public void Swap(Vector3 position, int row, int column)
        {
            // Debug.Log($"Swap {_cellBehaviour.name} . {position} / {row} / {column}");
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

        public void SetCellTypeFromMatch(CellMatchedType cellMatchedType)
        {
            // _cellImageType = CellImageType.None;
            _cellType = GetCellTypeByMatchedType(cellMatchedType);
            _cellMatchedType = cellMatchedType;
            _cellBehaviour.ChangeCellSprite(_cellType, _cellMatchedType);
        }

        private CellType GetCellTypeByMatchedType(CellMatchedType cellMatchedType)
        {
            return cellMatchedType switch
            {
                CellMatchedType.Five_OneLine => CellType.Wand,
                CellMatchedType.Five_Shape => CellType.Bomb,
                CellMatchedType.None => CellType.None,
                CellMatchedType.Horizontal_Four => CellType.Rocket,
                CellMatchedType.Vertical_Four => CellType.Rocket,
                CellMatchedType.Three => CellType.None,
                _ => CellType.None
            };
        }

        public void ActivateRocket()
        {
            
        }

        public void ChangeCellType(CellType cellType)
        {
            _cellType = cellType;
        }

        public bool TryRemoveCell()
        {
            switch (_cellType)
            {
                case CellType.Normal:
                case CellType.Obstacle_Box:
                case CellType.Rocket:
                case CellType.Wand:
                case CellType.Bomb:
                    _cellBehaviour.Activate(false);
                    return true;
                case CellType.Obstacle_IceBox:
                    bool isCracked = _cellBehaviour.TakeDamage(_cellType);
                    return isCracked;
                case CellType.Obstacle_Cage:
                    isCracked = _cellBehaviour.TakeDamage(_cellType);
                    return isCracked;
                case CellType.None:
                    break;
            }
            
            Debug.Log($"failed remove cell {_cellType}");
            return false;
        }
    }
}