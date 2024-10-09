using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private int _stageLevel;
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _cellPrefab;
        
        private Stage _currentStage;
        
        private void Start()
        {
            LoadStage();
        }

        private void LoadStage()
        {
            StageBuilder builder = new StageBuilder();
            Stage stage = builder.LoadStage(_stageLevel);
            _currentStage = stage;

            Vector2 centerPosition =
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));
            stage.Build(centerPosition, _blockPrefab, _cellPrefab);
        }
    }
}
