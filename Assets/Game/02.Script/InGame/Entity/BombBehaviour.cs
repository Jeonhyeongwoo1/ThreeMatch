using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThreeMatch.InGame.Effect;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class BombBehaviour : CellBehaviour
    {
        [SerializeField] private Transform _scaler;
        
        public override void Disappear(CellImageType cellImageType = CellImageType.None)
        {
            base.Disappear(cellImageType);
        }

        public override void ShowHintAnimation()
        {
            DoPunchScaleAnimation(_scaler, 1, true, 1f);
        }

        public void ShowBombEffect()
        {
            var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.BombEffect);
            var obj = pool.Get<BombEffect>();
            obj.Spawn(transform);
        }
    }
}