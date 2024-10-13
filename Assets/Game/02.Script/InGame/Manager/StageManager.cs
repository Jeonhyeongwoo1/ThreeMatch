using System;
using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Entity;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private int _stageLevel;
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _cellPrefab;
        
        private Stage _currentStage;
        
        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            LoadStage();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _currentStage.OnItemUsagePendingAction(InGameItemType.VerticalRocket);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _currentStage.OnItemUsagePendingAction(InGameItemType.HorizontalRocket);
            }
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

        public void ReadyStage()
        {
            // _currentStage.
        }
    }
}
