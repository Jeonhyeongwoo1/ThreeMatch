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

        public void UpdateUI(bool isOdd, bool isActive)
        {
            _sprite.sprite = _data.SpriteArray[isOdd ? 0 : 1];
            gameObject.SetActive(isActive);
        }
    }
}
