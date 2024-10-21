using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThreeMatch.InGame.Effect;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class WandBehaviour : CellBehaviour
    {
        public void ShowLighting(CellType cellType, Cell cell)
        {
            if (cellType == CellType.Wand)
            {
                var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.WandLightEffect);
                var lighting = pool.Get<Lighting>();
                lighting.SetPosition(transform.position, cell.Position);
                lighting.Spawn(transform);

                var lightPool = ObjectPoolManager.Instance.GetPool(PoolKeyType.CellDisappearLightEffect);
                var pooledObject = lightPool.Get<PooledObject>();
                pooledObject.Spawn(cell.CellBehaviour.transform);
                cell.CellBehaviour.Activate(false);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(transform.DOScale(Vector3.one * 1.5f, 0.3f));
                sequence.AppendInterval(0.5f);
                sequence.OnComplete(() => Activate(false));
            }
        }
    }
}