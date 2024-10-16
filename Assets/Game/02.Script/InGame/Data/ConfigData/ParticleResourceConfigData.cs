using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThreeMatch.InGame.Data
{
    [CreateAssetMenu(fileName = "ParticleResourceConfigData", menuName = "ThreeMatch/ParticleResourceConfigData", order = 1)]
    public class ParticleResourceConfigData : ScriptableObject
    {
        public Sprite[] CellSpriteArray => _cellSpriteArray;
        
        [SerializeField] private Sprite[] _cellSpriteArray;
    }
}