using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "CellConfigData", menuName = "ThreeMatch/CellConfigData", order = 1)]
    public class CellConfigData : ScriptableObject
    {
        public Sprite[] SpriteArray => _spriteArray;
        public Sprite BombSprite => _bombSprite;
        public Sprite VerticalRocketSprite => _verticalRocketSprite;
        public Sprite HorizontalRocketSprite => _horizontalRocketSprite;
        public Sprite WandSprite => _wandSprite;
        
        [SerializeField] private Sprite[] _spriteArray;
        [SerializeField] private Sprite _bombSprite;
        [SerializeField] private Sprite _verticalRocketSprite;
        [SerializeField] private Sprite _horizontalRocketSprite;
        [SerializeField] private Sprite _wandSprite;
    }
}