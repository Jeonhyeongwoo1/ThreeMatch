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

        public static Action<bool> onUseInGameItem;
        
        private Stage _currentStage;
        
        private void Start()
        {
            LoadStage();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _currentStage.UseInGameItem(InGameItemType.VerticalRocket);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _currentStage.UseInGameItem(InGameItemType.HorizontalRocket);
            }
        }

        private async void UseInGameItem()
        {
            // onUseInGameItem?.Invoke(true);
            await _currentStage.UseInGameItem(InGameItemType.Hammer);
            // onUseInGameItem?.Invoke(false);
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
