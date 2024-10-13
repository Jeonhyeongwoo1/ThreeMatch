using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "BlockConfigData", menuName = "ThreeMatch/BlockConfigData", order = 1)]
    public class BlockConfigData : ScriptableObject
    {
        public Sprite[] NormalBlockTypeSpriteArray => _normalBlockTypeSpriteArray;
        public Sprite GeneratorSprite => _generatorSprite;
   
        [SerializeField] private Sprite[] _normalBlockTypeSpriteArray;
        [SerializeField] private Sprite _generatorSprite;
    }
}