using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.UI
{
    public class TouchBlockPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _blockObj;
        
        private void OnEnable()
        {
            GameManager.onGameStartAction += () => _blockObj.SetActive(false);
            GameManager.onGameReadyAction += () => _blockObj.SetActive(true);
        }

        private void OnDisable()
        {
            GameManager.onGameStartAction -= () => _blockObj.SetActive(false);
            GameManager.onGameReadyAction -= () => _blockObj.SetActive(true);
        }
    }
}