using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Generator : CellBehaviour
    {
        private Camera _camera;

        protected override void Start()
        {
            base.Start();

            _camera = Camera.main;
        }

        public void CreateStarObject()
        {
            var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.StarObject);
            var starObj = pool.Get<PooledObject>();
            starObj.transform.position = transform.position;
            starObj.gameObject.SetActive(true);

            Vector3 endPosition = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
            Sequence seq = DOTween.Sequence();
            seq.Append(starObj.transform.DOMove(endPosition, 1f));
            seq.OnComplete(() =>
            {
                starObj.ForceSleep();
            });
        }
    }
}