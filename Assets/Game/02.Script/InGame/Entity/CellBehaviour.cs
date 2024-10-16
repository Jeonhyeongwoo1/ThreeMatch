using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Effect;
using ThreeMatch.InGame.Manager;
using UnityEngine;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

namespace ThreeMatch.InGame.Entity
{
    public class CellBehaviour : MonoBehaviour
    {
        public Vector2 Size => _backgroundSprite.bounds.size;

        [SerializeField] private SpriteRenderer _backgroundSprite;
        [SerializeField] private SpriteRenderer _frontSprite;
        [SerializeField] private CellConfigData _data;

        private GameObject _wandIdleParticlePrefab;
        
        public void Initialize(CellType cellType, CellImageType cellImageType = CellImageType.None)
        {
            switch (cellType)
            {
                case CellType.None:
                    break;
                case CellType.Normal:
                    _backgroundSprite.sprite = _data.GetCellImageTypeSpriteData(cellImageType).normalSprite;
                    break;
               case CellType.Obstacle_Box:
                   _backgroundSprite.sprite = _data.BoxSprite;
                   break;
               case CellType.Obstacle_Cage:
                   _backgroundSprite.sprite = _data.GetCellImageTypeSpriteData(cellImageType).normalSprite;
                   _frontSprite.sprite = _data.CageSprite;
                   transform.GetOrAddComponent<Health>().Initialize(Const.CageHP);
                   break;
               case CellType.Obstacle_IceBox:
                   int hp = Const.IceBoxHP;
                   _backgroundSprite.sprite = _data.IceBoxSpriteArray[hp - 1];
                   transform.GetOrAddComponent<Health>().Initialize(hp);
                   break;
               case CellType.Generator:
                   _backgroundSprite.sprite = _data.GeneratorSprite;
                   break;
            }
            
            _frontSprite.gameObject.SetActive(CellType.Obstacle_Cage == cellType);
        }

        public async UniTask MoveAsync(List<Vector3> movePositionList, bool isSpawn)
        {
            if (isSpawn)
            {
                Activate(true);
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

        public void ChangeCellSprite(CellType cellType, CellMatchedType cellMatchedType, CellImageType cellImageType)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    break;
                case CellType.Rocket:
                    bool isVertical = cellMatchedType == CellMatchedType.Vertical_Four;
                    _backgroundSprite.sprite = isVertical
                        ? _data.GetCellImageTypeSpriteData(cellImageType).verticalSprite
                        : _data.GetCellImageTypeSpriteData(cellImageType).horizontalSprite;
                    break;
                case CellType.Wand:
                    _backgroundSprite.sprite = _data.WandSprite;
                    _wandIdleParticlePrefab = Instantiate(_data.WandIdleParticlePrefab, transform);
                    break;
                case CellType.Bomb:
                    _frontSprite.sprite = _data.BombSprite;
                    _frontSprite.gameObject.SetActive(true);
                    break;
            }
        }
        
        public void ShowLighting(CellType cellType, Cell cell)
        {
            if (cellType == CellType.Wand)
            {
                var pool = ObjectPoolManager.Instance.DequeuePool(PoolKeyType.WandLightEffect);
                var lighting = pool.Get<Lighting>();
                lighting.SetPosition(transform.position, cell.Position);
                lighting.Spawn(transform);

                var lightPool = ObjectPoolManager.Instance.DequeuePool(PoolKeyType.CellDisappearLightEffect);
                var pooledObject = lightPool.Get<PooledObject>();
                pooledObject.Spawn(cell.CellBehaviour.transform);
                cell.CellBehaviour.Activate(false);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(transform.DOScale(Vector3.one * 1.5f, 0.3f));
                sequence.AppendInterval(0.5f);
                sequence.OnComplete(() => Activate(false));
            }
        }

        public void ShowBombEffect()
        {
            var pool = ObjectPoolManager.Instance.DequeuePool(PoolKeyType.BombEffect);
            var obj = pool.Get<BombEffect>();
            obj.Spawn(transform);
        }

        public void ShowRocketEffect(CellMatchedType cellMatchedType)
        {
            var pool = ObjectPoolManager.Instance.DequeuePool(PoolKeyType.RocketEffect);
            var effect = pool.Get<PooledObject>();
            effect.Spawn(transform);
            effect.transform.eulerAngles = new Vector3(0, 0, cellMatchedType == CellMatchedType.Vertical_Four ? 90 : 0);
        }

        public void Disappear(CellImageType cellImageType = CellImageType.None)
        {
            if (cellImageType != CellImageType.None)
            {
                var pool = ObjectPoolManager.Instance.DequeuePool(PoolKeyType.CellDisappearParticle);
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
            gameObject.SetActive(isActivate);

            if (_wandIdleParticlePrefab)
            {
                Destroy(_wandIdleParticlePrefab);
            }
        }

        public void HitGenerator()
        {
            GameObject prefab = _data.StarPrefab;
            GameObject obj = Instantiate(prefab);
            obj.transform.position = transform.position;
            obj.transform.DOMove(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f)), 1f)
                .OnComplete(() => obj.SetActive(false));
        }
        
        public bool Hit(CellType cellType)
        {
            if (!TryGetComponent(out Health health))
            {
                Debug.LogError($"failed get health component {transform.name}");
                return false; 
            }

            int hp = health.TakeDamage(1);
            if (hp > 0)
            {
                switch (cellType)
                {
                    case CellType.Obstacle_IceBox:
                        _backgroundSprite.sprite = _data.IceBoxSpriteArray[hp - 1];
                        break;
                    case CellType.Obstacle_Cage:
                        break;
                }
            }
            else
            {
                switch (cellType)
                {
                    case CellType.Obstacle_IceBox:
                        Activate(false);
                        break;
                    case CellType.Obstacle_Cage:
                        _frontSprite.gameObject.SetActive(false);
                        break;
                }
            }
            
            return health.IsDead();
        }
    }
}