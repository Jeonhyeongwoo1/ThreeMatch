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
        public ObstacleCellType ObstacleCellType => _obstacleCellType;
        public int Row => _row;
        public int Column => _column;
        
        private int _column;
        private int _row;
        private CellBehaviour _cellBehaviour;
        private CellImageType _cellImageType;
        private CellMatchedType _cellMatchedType;
        private CellType _cellType;
        private ObstacleCellType _obstacleCellType;

        public Cell(int row, int column, CellType cellType, ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None)
        {
            _column = column;
            _row = row;
            _cellType = cellType;
            _obstacleCellType = obstacleCellType;
            _cellMatchedType = CellMatchedType.None;
            
            if (obstacleCellType == ObstacleCellType.Cage)
            {
                int length = Enum.GetNames(typeof(CellImageType)).Length - 1;
                int select = UnityEngine.Random.Range(0, length);
                _cellImageType = (CellImageType)select;
            }
            else
            {
                _cellImageType = cellImageType;
            }
        }
        
        public void CreateCellBehaviour(Vector3 position, Transform parent = null)
        {
            IPoolable pool;
            PoolKeyType poolKeyType = PoolKeyType.None;
            poolKeyType = _cellType switch
            {
                CellType.Normal => PoolKeyType.Cell_Normal,
                CellType.Obstacle => _obstacleCellType switch
                {
                    ObstacleCellType.Box => PoolKeyType.Cell_Obstacle_OneHitBox,
                    ObstacleCellType.IceBox => PoolKeyType.Cell_Obstacle_HitableBox,
                    ObstacleCellType.Cage => PoolKeyType.Cell_Obstacle_Cage,
                    _ => poolKeyType
                },
                CellType.Generator => PoolKeyType.Cell_Generator,
                _ => poolKeyType
            };

#if UNITY_EDITOR
            // Debug.Log($"{poolKeyType} /{_obstacleCellType}");
            var data =
                ObjectPoolConfigData.Instance.ObjectPoolDataList.Find(v => v.poolKeyType == poolKeyType);
            var obj = Object.Instantiate(data.prefab);
            pool = obj.GetComponent<IPoolable>();
#else
            pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.Cell);
#endif
            _cellBehaviour = pool.Get<CellBehaviour>();
            _cellBehaviour.name = $"{poolKeyType} {_row} / {_column}";
            _cellBehaviour.Initialize(_cellType, parent, position, _obstacleCellType, _cellImageType);
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
            PoolKeyType poolKeyType = PoolKeyType.None;
            switch (_cellType)
            {
                case CellType.Rocket:
                    poolKeyType = PoolKeyType.Cell_Rocket;
                    break;
                case CellType.Wand:
                    poolKeyType = PoolKeyType.Cell_Wand;
                    break;
                case CellType.Bomb:
                    poolKeyType = PoolKeyType.Cell_Bomb;
                    break;
                case CellType.Normal:
                case CellType.Obstacle:
                case CellType.Generator:
                    Debug.LogError($"failed change cell type from match  {cellMatchedType} {_cellType}");
                    poolKeyType = PoolKeyType.Cell_Normal;
                    break;
            }
            
            _cellBehaviour.Activate(false);
            position = Position;
            var poolable = ObjectPoolManager.Instance.GetPool(poolKeyType);
            _cellBehaviour = poolable.Get<CellBehaviour>();
            _cellBehaviour.Initialize(_cellType, parent, position, _obstacleCellType, _cellImageType, _cellMatchedType);
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
                    _cellBehaviour.Disappear(_cellImageType);
                    return true;
                case CellType.Obstacle:
                    switch (_obstacleCellType)
                    {
                        case ObstacleCellType.Box:
                            _cellBehaviour.Disappear(_cellImageType);
                            return true;
                        case ObstacleCellType.IceBox:
                        case ObstacleCellType.Cage: 
                            var obstacle = _cellBehaviour as Obstacle;
                            return obstacle.Hit(_obstacleCellType);
                    }
                    break;
                case CellType.Bomb:
                    var bomb = _cellBehaviour as BombBehaviour;
                    bomb.ShowBombEffect();
                    bomb.Disappear(_cellImageType);
                    return true;
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