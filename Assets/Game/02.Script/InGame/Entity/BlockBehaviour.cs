using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Data;
using UnityEngine;

namespace ThreeMatch.InGame
{
    public class BlockBehaviour : MonoBehaviour
    {
        public Vector2 Size => _sprite.bounds.size;
        
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private BlockConfigData _data;
        
        public void UpdatePosition(Vector2 position)
        {
            transform.position = position;
        }

        public void UpdateUI(bool isOdd, BlockType blockType)
        {
            switch (blockType)
            {
                case BlockType.Normal:
                    _sprite.sprite = _data.NormalBlockTypeSpriteArray[isOdd ? 0 : 1];
                    break;
                case BlockType.None:
                    gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }
}
