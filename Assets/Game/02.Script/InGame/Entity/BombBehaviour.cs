using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Effect;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class BombBehaviour : CellBehaviour
    {
        public override void Disappear(CellImageType cellImageType = CellImageType.None)
        {
            base.Disappear(cellImageType);
        }
        
        public void ShowBombEffect()
        {
            var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.BombEffect);
            var obj = pool.Get<BombEffect>();
            obj.Spawn(transform);
        }
    }
}