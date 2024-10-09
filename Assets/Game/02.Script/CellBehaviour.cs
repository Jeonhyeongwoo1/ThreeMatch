using System.Collections;
using DG.Tweening;
using ThreeMatch.InGame.Data;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class CellBehaviour : MonoBehaviour
    {
        public Vector2 Size => _sprite.bounds.size;
        public Vector3 OriginPosition => _originPosition;
        
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private CellConfigData _data;
        [SerializeField] private CellImageType _cellImageType;
        private Vector2 _originPosition;
        
        public void OnPointerDown(Vector3 position)
        {
            // transform.position = position;
        }

        public void Initialize(Vector3 initPosition, int spriteImageIndex)
        {
            _originPosition = initPosition;
            transform.position = _originPosition;
            _sprite.sprite = _data.SpriteArray[spriteImageIndex];
            _cellImageType = (CellImageType) spriteImageIndex;
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

        public void UndoSwap()
        {
            transform.position = _originPosition;
        }

        public void UpdateUI(int spriteIndex)
        {
            _sprite.sprite = _data.SpriteArray[spriteIndex];
        }
    }
}