using System;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Interface;
using ThreeMatch.InGame.Manager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThreeMatch.InGame.Entity
{
    public class Cell : IDisposable
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
        
        public void CreateCellBehaviour(Vector3 position, Transform parent = null)
        {
            IPoolable pool = null;
            PoolKeyType poolKeyType = PoolKeyType.None;
            switch (_cellType)
            {
                case CellType.Normal:
                    poolKeyType = PoolKeyType.Cell;
                    break;
                case CellType.Obstacle_Box:
                    poolKeyType = PoolKeyType.Obstacle_Box;
                    break;
                case CellType.Obstacle_IceBox:
                    poolKeyType = PoolKeyType.Obstacle_IceBox;
                    break;
                case CellType.Obstacle_Cage:
                    poolKeyType = PoolKeyType.Obstacle_Cage;
                    break;
                case CellType.Generator:
                    poolKeyType = PoolKeyType.Generator;
                    break;
            }
            
#if UNITY_EDITOR
            var data =
                ObjectPoolConfigData.Instance.ObjectPoolDataList.Find(v => v.poolKeyType == poolKeyType);
            var obj = Object.Instantiate(data.prefab);
            pool = obj.GetComponent<IPoolable>();
#else
            pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.Cell);
#endif
            _cellBehaviour = pool.Get<CellBehaviour>();
            _cellBehaviour.name = $"{poolKeyType} {_row} / {_column}";
            _cellBehaviour.Initialize(_cellType, parent, position, _cellImageType);
        }

        public void Swap(Vector3 position, int row, int column)
        {
            _row = row;
            _column = column;
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

        public void SetCellTypeFromMatch(CellMatchedType cellMatchedType, Transform parent = null)
        {
            _cellType = GetCellTypeByMatchedType(cellMatchedType);
            _cellMatchedType = cellMatchedType;
            Vector3 position;
            
            switch (_cellType)
            {
                case CellType.Rocket:
                    _cellBehaviour.Activate(false);
                    position = Position;
                    var rocket = ObjectPoolManager.Instance.GetPool(PoolKeyType.Rocket);
                    _cellBehaviour = rocket.Get<CellBehaviour>();
                    _cellBehaviour.Initialize(_cellType, parent, position, _cellImageType, _cellMatchedType);
                    break;
                case CellType.Wand:
                    _cellBehaviour.Activate(false);
                    position = Position;
                    var wand = ObjectPoolManager.Instance.GetPool(PoolKeyType.Wand);
                    _cellBehaviour = wand.Get<CellBehaviour>();
                    _cellBehaviour.Initialize(_cellType, parent, position, _cellImageType);
                    break;
                case CellType.Bomb:
                    _cellBehaviour.Activate(false);
                    position = Position;
                    var bomb = ObjectPoolManager.Instance.GetPool(PoolKeyType.Bomb);
                    _cellBehaviour = bomb.Get<CellBehaviour>();
                    _cellBehaviour.Initialize(_cellType, parent, position, _cellImageType);
                    break;
                case CellType.Normal:
                case CellType.Obstacle_Box:
                case CellType.Obstacle_IceBox:
                case CellType.Obstacle_Cage:
                case CellType.Generator:
                    break;
            }
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
                case CellType.Wand:
                    return true;
                case CellType.Rocket:
                    var rocket = _cellBehaviour as RocketBehaviour;
                    rocket.ShowRocketEffect(_cellMatchedType);
                    rocket.Disappear(_cellImageType);
                    return true;
                case CellType.Normal:
                case CellType.Obstacle_Box:
                    _cellBehaviour.Disappear(_cellImageType);
                    return true;
                case CellType.Bomb:
                    var bomb = _cellBehaviour as BombBehaviour;
                    bomb.ShowBombEffect();
                    bomb.Disappear(_cellImageType);
                    return true;
                case CellType.Obstacle_Cage:
                case CellType.Obstacle_IceBox:
                    var obstacle = _cellBehaviour as Obstacle;
                    return obstacle.Hit(_cellType);
                case CellType.Generator:
                    var generator = _cellBehaviour as Generator;
                    generator.CreateStarObject();
                    return false;
                default:
                case CellType.None:
                    Debug.Log($"failed remove cell {_cellType}");
                    break;
            }
            
            return false;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}