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
        public void ShowLighting(Cell cell, float duration = 0.8f)
        {
            var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.WandLightEffect);
            var lighting = pool.Get<Lighting>();
            lighting.SetPosition(transform.position, cell.Position);
            lighting.SetDuration(duration);
            lighting.Spawn(transform);
        }

        public void StartLightingAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(Vector3.one * 1.5f, 0.3f));
        }
    }
}