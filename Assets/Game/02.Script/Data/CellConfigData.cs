using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "CellConfigData", menuName = "ThreeMatch/CellConfigData", order = 1)]
    public class CellConfigData : ScriptableObject
    {
        public Sprite[] SpriteArray => _spriteArray;
        
        [SerializeField] private Sprite[] _spriteArray;
    }
}