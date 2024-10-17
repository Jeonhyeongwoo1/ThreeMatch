using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Entity;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using UnityEngine;

namespace ThreeMatch.InGame.Manager
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private int _stageLevel;
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _containerPrefab;
        private GameObject _boardContainer;
        private Stage _currentStage;
        
        private void Start()
        {
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

        private UniTaskCompletionSource _gameReadyCompletionSource;

        private async UniTaskVoid LoadStage()
        {
            GameManager.onGameReadyAction?.Invoke();
            StageBuilder builder = new StageBuilder();
            Stage stage = builder.LoadStage();
            _currentStage = stage;
            Vector2 centerPosition =
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));

            _boardContainer = Instantiate(_containerPrefab);
            await stage.BuildAsync(centerPosition, _blockPrefab, _cellPrefab, _boardContainer.transform);

            _gameReadyCompletionSource = new();
            var gameReadyView = UIManager.Instance.CreateOrGetView<GameReadyView>();
            var missionModel = ModelFactory.CreateOrGet<MissionModel>();
            var missionViewDataList = missionModel.missionDataList.Value.Select(missionData => new MissionView.Data
                { missionType = missionData.missionType, count = missionData.removeCount }).ToList();
            gameReadyView.Show(missionViewDataList, _gameReadyCompletionSource);

            await _gameReadyCompletionSource.Task;
            await UniTask.WaitForSeconds(0.5f);
            await stage.BuildAfterProcessAsync();
            
            GameManager.onGameStartAction?.Invoke();
        }
    }
}