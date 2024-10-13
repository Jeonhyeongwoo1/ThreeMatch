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
        public Sprite BoxSprite => _boxSprite;
        public Sprite[] IceBoxSpriteArray => _iceBoxSpriteArray;
        public Sprite CageSprite => _cageSprite;
        public Sprite GeneratorSprite => _generatorSprite;
        public GameObject StarPrefab => _starPrefab;
        
        [SerializeField] private Sprite[] _spriteArray;
        [SerializeField] private Sprite _bombSprite;
        [SerializeField] private Sprite _verticalRocketSprite;
        [SerializeField] private Sprite _horizontalRocketSprite;
        [SerializeField] private Sprite _wandSprite;

        [SerializeField] private Sprite _boxSprite;
        [SerializeField] private Sprite[] _iceBoxSpriteArray;
        [SerializeField] private Sprite _cageSprite;
        [SerializeField] private Sprite _generatorSprite;

        [SerializeField] private GameObject _starPrefab;
    }
}