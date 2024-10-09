using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ThreeMatch.InGame.Data;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class CellBehaviour : MonoBehaviour
    {
        public Vector2 Size => _sprite.bounds.size;

        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private CellConfigData _data;
        [SerializeField] private CellImageType _cellImageType;
        
        public void OnPointerDown(Vector3 position)
        {
            // transform.position = position;
        }

        public void Initialize(int spriteImageIndex)
        {
            _sprite.sprite = _data.SpriteArray[spriteImageIndex];
            _cellImageType = (CellImageType) spriteImageIndex;
        }

        public async UniTask Move(List<Vector3> movePositionList)
        {
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

        public void UpdatePosition(Vector3 position)
        {
            transform.position = position;
        }

        public void Swap(Vector3 position)
        {
            transform.DOKill();
            transform.DOMove(position, Const.SwapAnimationDuration);
        }

        public void UpdateUI(int spriteIndex)
        {
            _sprite.sprite = _data.SpriteArray[spriteIndex];
        }
    }
}