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
        private static StageManager _instance;

        public static StageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<StageManager>();
                }

                return _instance;
            }
        }
        
        public int StageLevel => _stageLevel;
        
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _containerPrefab;
        
        private GameObject _boardContainer;
        private Stage _currentStage;
        private StageBuilder _stageBuilder;
        private int _stageLevel;

        private void Start()
        {
            int stageLevel = PlayerPrefs.GetInt(PlayerPrefsKeys.lastStageLevel);
            LoadStage(stageLevel).Forget();
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

        public void ReloadStage(int stageLevel)
        {
            _currentStage?.Dispose();
            LoadStage(stageLevel);
        }

        public async UniTaskVoid LoadStage(int stageLevel)
        {
            GameManager.onGameReadyAction?.Invoke();
            _stageBuilder = new StageBuilder();
            _stageLevel = stageLevel;
            Stage stage = _stageBuilder.LoadStage(stageLevel);
            _currentStage = stage;
            Vector2 centerPosition =
                Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f));

            if (_boardContainer != null)
            {
                DestroyImmediate(_boardContainer.gameObject);
            }
            
            _boardContainer = Instantiate(_containerPrefab);
            await stage.BuildAsync(centerPosition, _blockPrefab, _cellPrefab, _boardContainer.transform);

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
    }
}