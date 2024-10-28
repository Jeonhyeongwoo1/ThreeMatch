using DG.Tweening;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class RocketBehaviour : CellBehaviour
    {
        [SerializeField] private Transform _scaler;
        
        private Animator _animator;
        
        protected override void Start()
        {
            base.Start();

            TryGetComponent(out _animator);
        }

        public override void ShowHintAnimation()
        {
            DoPunchScaleAnimation(_scaler, 1, true, 1f);
        }

        public override void Initialize(CellType cellType, Transform parent, Vector3 position,
            ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None,
            CellMatchedType cellMatchedType = CellMatchedType.None)
        {
            base.Initialize(cellType, parent, position, obstacleCellType, cellImageType, cellMatchedType);
            
            bool isVertical = cellMatchedType == CellMatchedType.Vertical_Four;
            transform.localEulerAngles = new Vector3(0, 0, isVertical ? 90 : 0);
        }

        public override void Disappear(CellImageType cellImageType = CellImageType.None)
        {
            if (_animator)
            {
                _animator.enabled = false;
            }
            
            base.Disappear(cellImageType);
        }
        
        public void ShowRocketEffect(CellMatchedType cellMatchedType)
        {
            var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.RocketEffect);
            var effect = pool.Get<PooledObject>();
            effect.Spawn(transform);
            effect.transform.eulerAngles = new Vector3(0, 0, cellMatchedType == CellMatchedType.Vertical_Four ? 90 : 0);
        }
    }
}