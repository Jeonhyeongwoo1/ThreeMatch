using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.Entity
{
    public class Background : MonoBehaviour
    {
        [SerializeField] private Image _bgImage;
        [SerializeField] private Sprite[] _bgSpriteArray;

        private void OnEnable()
        {
            _bgImage.sprite = _bgSpriteArray[0];
        }
    }
}
