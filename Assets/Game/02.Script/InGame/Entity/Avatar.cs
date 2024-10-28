using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class Avatar : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            TryGetComponent(out _animator);
        }

        private void OnEnable()
        {
            GameManager.onCellComboAction += ComboAnimation;
            _animator.SetTrigger("Game");
        }

        private void OnDisable()
        {
            GameManager.onCellComboAction -= ComboAnimation;
        }

        private void ComboAnimation(int score, int comboCount)
        {
            if (comboCount > 0)
            {
                _animator.SetTrigger("Cool");
            }
        }
    }
}