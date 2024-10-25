using System;
using Sirenix.OdinInspector;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using ThreeMatch.InGame.UI;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    public class StageEditorManager : MonoBehaviour
    {
        public static StageLevel stageLevel;

        [ShowInInspector]
        public StageLevel StageLevel
        {
            get => stageLevel;
            set => stageLevel = value;
        }

        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _containerPrefab;
        
        private GameObject _boardContainer;
        private Stage _currentStage;

        private void Start()
        {
            LoadStage();
        }
        private void LoadStage()
        {
            RemoveDummyObj();

            var stageLevel = Resources.Load<StageLevelConfigDataForEditor>("StageLevelConfigDataForEditor");
            Debug.Log($"stageLevel {stageLevel}");
            StageBuilder builder = new StageBuilder();
            Stage stage = builder.LoadStage(stageLevel.GetBoardInfoDataArray(), stageLevel.missionInfoDataList, stageLevel.remainingMoveCount);
            _currentStage = stage;
            Vector2 centerPosition =
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));

            _boardContainer = Instantiate(_containerPrefab);
            stage.BuildAsync(centerPosition, _blockPrefab, _cellPrefab, _boardContainer.transform);
        }

        private void RemoveDummyObj()
        {
            var boardContainerArray = GameObject.FindGameObjectsWithTag("BoardContainer");
            foreach (GameObject go in boardContainerArray)
            {
                DestroyImmediate(go);
            }            
            
            var MissionPanelArray = GameObject.FindGameObjectsWithTag("MissionPanel");
            foreach (GameObject go in MissionPanelArray)
            {
                var missionElement = go.transform.GetComponentsInChildren<MissionElement>();
                foreach (MissionElement element in missionElement)
                {
                    DestroyImmediate(element.gameObject);
                }
            }
        }
    }
}