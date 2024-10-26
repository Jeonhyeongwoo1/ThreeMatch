using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Manager;
using TMPro;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class RemainingMoveCountView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _countText;

        private void OnEnable()
        {
            GameManager.onChangeRemainingMoveCountAction += OnValueChanged;
        }

        private void OnDisable()
        {
            GameManager.onChangeRemainingMoveCountAction -= OnValueChanged;
        }

        private void OnValueChanged(int remainingMoveCount)
        {
            _countText.text = remainingMoveCount.ToString();
        }
    }
}