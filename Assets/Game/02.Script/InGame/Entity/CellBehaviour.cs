using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThreeMatch.InGame.Data;
using UnityEngine;
using Unity.VisualScripting;

namespace ThreeMatch.InGame.Entity
{
    public class CellBehaviour : MonoBehaviour
    {
        public Vector2 Size => _backgroundSprite.bounds.size;

        [SerializeField] private SpriteRenderer _backgroundSprite;
        [SerializeField] private SpriteRenderer _frontSprite;
        [SerializeField] private CellConfigData _data;

        public void Initialize(CellType cellType, CellImageType cellImageType = CellImageType.None)
        {
            switch (cellType)
            {
                case CellType.None:
                    break;
                case CellType.Normal:
                    _backgroundSprite.sprite = _data.SpriteArray[(int)cellImageType];       
                    break;
               case CellType.Obstacle_Box:
                   _backgroundSprite.sprite = _data.BoxSprite;
                   break;
               case CellType.Obstacle_Cage:
                   _backgroundSprite.sprite = _data.SpriteArray[(int)cellImageType];
                   _frontSprite.sprite = _data.CageSprite;
                   transform.GetOrAddComponent<Health>().Initialize(Const.CageHP);
                   break;
               case CellType.Obstacle_IceBox:
                   int hp = Const.IceBoxHP;
                   _backgroundSprite.sprite = _data.IceBoxSpriteArray[hp - 1];
                   transform.GetOrAddComponent<Health>().Initialize(hp);
                   break;
            }
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
        
        public void ChangeCellSprite(CellType cellType, CellMatchedType cellMatchedType)
        {
            switch (cellType)
            {
                case CellType.Normal:
                    break;
                case CellType.Rocket:
                    if (cellMatchedType == CellMatchedType.Horizontal_Four)
                    {
                        _backgroundSprite.sprite = _data.HorizontalRocketSprite;
                    }
                    else if (cellMatchedType == CellMatchedType.Vertical_Four)
                    {
                        _backgroundSprite.sprite = _data.VerticalRocketSprite;
                    }
                    break;
                case CellType.Wand:
                    _backgroundSprite.sprite = _data.WandSprite;
                    break;
                case CellType.Bomb:
                    _backgroundSprite.sprite = _data.BombSprite;
                    break;
            }
        }

        public void Activate(bool isActivate)
        {
            gameObject.SetActive(isActivate);
        }
        
        public bool TakeDamage(CellType cellType)
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