using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Health : MonoBehaviour
    {
        public int HP => _hp;
        
        [SerializeField] private int _hp;

        public void Initialize(int hp)
        {
            _hp = hp;
        }

        public int TakeDamage(int value)
        {
            _hp -= value;
            return _hp;
        }

        public bool IsDead() => _hp == 0;
    }
}
