using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Manager;
using ThreeMatch.OutGame.Manager;
using UnityEngine;

namespace ThreeMatch.InGame.UI
{
    public class TouchBlockPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _blockObj;
        
        private void OnEnable()
        {
            GameManager.onGameStartAction += OnGameStart;
            GameManager.onGameReadyAction += OnGameReady;
            StageLevelManager.onSelectedStageAction += OnSelectedStage;
        }

        private void OnDisable()
        {
            GameManager.onGameStartAction -= OnGameStart;
            GameManager.onGameReadyAction -= OnGameReady;
            StageLevelManager.onSelectedStageAction -= OnSelectedStage;
        }

        private void OnSelectedStage()
        {
            ActivateBlock(true);
        }

        private void OnGameReady()
        {
            ActivateBlock(true);
        }

        private void OnGameStart()
        {
            ActivateBlock(false);
        }

        private void ActivateBlock(bool active)
        {
            _blockObj.SetActive(active);
        }
    }
}