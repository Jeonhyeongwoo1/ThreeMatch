using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "BlockConfigData", menuName = "ThreeMatch/BlockConfigData", order = 1)]
    public class BlockConfigData : ScriptableObject
    {
        public Sprite[] SpriteArray => _spriteArray;
        
        [SerializeField] private Sprite[] _spriteArray;
    }
}