using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class RocketBehaviour : CellBehaviour
    {
        private Animator _animator;
        
        protected override void Start()
        {
            base.Start();

            TryGetComponent(out _animator);
        }

        public override void Initialize(CellType cellType, Transform parent, Vector3 position,
            CellImageType cellImageType = CellImageType.None, CellMatchedType cellMatchedType = CellMatchedType.None)
        {
            base.Initialize(cellType, parent, position, cellImageType);
            
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