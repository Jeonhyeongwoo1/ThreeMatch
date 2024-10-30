using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using ThreeMatch.Core;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Entity;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using UnityEngine;

#if UNITY_EDITOR
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
        private async void LoadStage()
        {
            RemoveDummyObj();

            GameManager.onGameReadyAction?.Invoke();
            var stageLevel = Resources.Load<StageLevelConfigDataForEditor>("StageLevelConfigDataForEditor");
            Debug.Log($"stageLevel {stageLevel}");
            StageBuilder builder = new StageBuilder();
            Stage stage = builder.LoadStage(stageLevel.GetBoardInfoDataArray(), stageLevel.missionInfoDataList,
                stageLevel.remainingMoveCount, stageLevel.aimScore);
            _currentStage = stage;
            Vector2 centerPosition =
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));

            _boardContainer = Instantiate(_containerPrefab);
            await stage.BuildAsync(centerPosition, _blockPrefab, _boardContainer.transform);
            
            UniTaskCompletionSource gameReadyCompletionSource = new();
            var gameReadyView = UIManager.Instance.GetView<GameReadyView>();
            var missionModel = ModelFactory.CreateOrGet<MissionModel>();
            var missionViewDataList = missionModel.missionDataList.Value.Select(missionData => new MissionView.Data
                { missionType = missionData.missionType, count = missionData.removeCount }).ToList();
            gameReadyView.Show(missionViewDataList, gameReadyCompletionSource);

            await gameReadyCompletionSource.Task;
            await UniTask.WaitForSeconds(0.5f);
            await stage.BuildAfterProcessAsync();
            
            GameManager.onGameStartAction?.Invoke();
        }

        private void RemoveDummyObj()
        {
            var boardContainerArray = GameObject.FindGameObjectsWithTag("BoardContainer");
            foreach (GameObject go in boardContainerArray)
            {
                DestroyImmediate(go);
            }            
            
            var missionPanelArray = GameObject.FindGameObjectsWithTag("MissionPanel");
            foreach (GameObject go in missionPanelArray)
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
#endif