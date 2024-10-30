using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Manager;
using UnityEngine;
using IPoolable = ThreeMatch.InGame.Interface.IPoolable;
using Sequence = DG.Tweening.Sequence;

namespace ThreeMatch.InGame.Entity
{
    public class CellBehaviour : MonoBehaviour, IPoolable
    {
        public Vector2 Size => _collider2D.bounds.size;
        public PoolKeyType PoolKeyType { get; set; }
        
        [SerializeField] protected SpriteRenderer _backgroundSprite;
        [SerializeField] protected SpriteRenderer _frontSprite;
        [SerializeField] protected GameResourcesConfigData _data;

        private Sequence _doPunchScaleSequence = null;
        private Collider2D _collider2D;

        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }

        public void Spawn(Transform spawner = null, Action callback = null)
        {
            Activate(true);
        }

        protected virtual void Start()
        {
            _collider2D = GetComponentInChildren<Collider2D>();
        }

        public virtual void Initialize(CellType cellType, Transform parent, Vector3 position,
            ObstacleCellType obstacleCellType = ObstacleCellType.None, CellImageType cellImageType = CellImageType.None,
            CellMatchedType cellMatchedType = CellMatchedType.None)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    _backgroundSprite.sprite = _data.GetCellImageTypeSpriteData(cellImageType).normalSprite;
                    break;
            }

            if (!_collider2D)
            {
                _collider2D = GetComponentInChildren<Collider2D>();
            }

            transform.SetParent(parent);
            transform.position = position;
            Spawn();
        }

        public async UniTask MoveAsync(List<Vector3> movePositionList, bool isSpawn)
        {
            if (isSpawn)
            {
                Spawn();
                DoFade(true);
            }
            
            transform.DOKill();
            List<Vector3> list = new List<Vector3>();
            list.AddRange(movePositionList);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Vector3 position = list[i];
                transform.DOMove(position, Const.CellMoveAnimationDuration)
                    .SetEase(i == 0 ? Ease.OutBack : Ease.Linear);
                await UniTask.WaitForSeconds(Const.CellMoveAnimationDuration);
            }
        }

        private void DoFade(bool isFadeOut)
        {
            Color color = _backgroundSprite.color;
            _backgroundSprite.color = new Color(color.r, color.g, color.b, isFadeOut ? 0 : 1);
            _backgroundSprite.DOFade(isFadeOut ? 1 : 0, Const.CellAlphaAnimation);
        }

        public async UniTask MoveAsync(Vector3 movePosition)
        {
            var t = new UniTaskCompletionSource();
            transform.DOKill();
            transform.DOMove(movePosition, Const.CellMoveAnimationDuration).OnComplete(() => t.TrySetResult());

            await t.Task;
        }

        public void UpdatePosition(Vector3 position)
        {
            transform.position = position;
        }

        public void Swap(Vector3 position)
        {
            transform.DOKill();
            transform.DOMove(position, Const.SwapAnimationDuration);
        }

        public virtual void Disappear(CellImageType cellImageType = CellImageType.None)
        {
            if (cellImageType != CellImageType.None)
            {
                var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.CellDisappearParticle);
                SplashParticle particle = pool.Get<SplashParticle>();
                particle.SetParticle(cellImageType);
                particle.Spawn(transform);
            }
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(Vector3.one * 1.1f, .1f));
            sequence.Append(transform.DOScale(Vector3.zero, 0.3f));
            sequence.OnComplete(() =>
            {
                Activate(false);
            });
        }

        public void Activate(bool isActivate)
        {
            if (isActivate)
            {
                gameObject.SetActive(true);
            }
            else
            {
                transform.localScale = Vector3.one;
                ((IPoolable)this).Sleep();
            }
        }

        protected void DoPunchScaleAnimation(Transform target, float duration, bool isLoop, float delay)
        {
            _doPunchScaleSequence = DOTween.Sequence();
            _doPunchScaleSequence.Append(target.DOPunchScale(Vector3.one * 0.13f, duration, 5));
            _doPunchScaleSequence.AppendInterval(delay);
            _doPunchScaleSequence.OnComplete(() => target.localScale = Vector3.one);
            if (isLoop)
            {
                _doPunchScaleSequence.SetLoops(-1);
            }
        }

        public void StopPunchScale()
        {
            if (_doPunchScaleSequence != null)
            {
                _doPunchScaleSequence.Kill(true);
                _doPunchScaleSequence = null;
            }
        }

        public virtual void ShowHintAnimation()
        {
            DoPunchScaleAnimation(transform, 1, true, 1f);
        }
    }
}