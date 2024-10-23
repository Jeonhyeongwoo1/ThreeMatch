using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Manager;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Entity;
using ThreeMatch.OutGame.Presenter;
using ThreeMatch.OutGame.View;
using UniRx;
using UnityEngine;

namespace ThreeMatch.OutGame.Manager
{
    public class StageLevelManager : MonoBehaviour
    {
        [SerializeField] private StageLevelListView _stageLevelListView;
        [SerializeField] private WaypointsMover _waypointsMover;
        [SerializeField] private GameObject _idleParticleObj;
        
        [SerializeField] private List<StageLevelModel> _dummyStageLevelModelList;
        [SerializeField] private int _dummyCurrentIndex;
        [SerializeField] private int _dummyNextIndex;

        private StageLevelListPresenter _stageLevelListPresenter;

        public static Action<int, Vector3> onArrivedWayPointAction;
        public static Action onStartMoveWayPointAction;
        
        private void Start()
        {
            LoadStageLevel();
            Initialize();
        }

        [Button]
        public void Move()
        {
            onStartMoveWayPointAction.Invoke();
            _waypointsMover.Move(_dummyCurrentIndex, _dummyNextIndex, OnFinishedDestinationWayPoint);
        }

        private void OnFinishedDestinationWayPoint(Vector3 position)
        {
            int level = _dummyNextIndex;
            _idleParticleObj.SetActive(true);
            onArrivedWayPointAction?.Invoke(level, position);
            _stageLevelListPresenter.UnLockStageLevel(level);
        }

        private void Initialize()
        {
            UIManager uiManager = UIManager.Instance;
            var userHeartPresenter = PresenterFactory.CreateOrGet<UserHeartPresenter>();
            userHeartPresenter.Initialize(uiManager.GetView<UserHeartView>(), ModelFactory.CreateOrGet<UserModel>());
            userHeartPresenter.UpdateUserHeart();
        }
        
        private void LoadStageLevel()
        {
            var listModel = ModelFactory.CreateOrGet<StageLevelListModel>();
            listModel.stageLevelModelList =
                new ReactiveProperty<List<StageLevelModel>>();
            var modelList = listModel.stageLevelModelList.Value;
            modelList = new List<StageLevelModel>(Const.MaxStageLevelCount);
            for (var i = 0; i < Const.MaxStageLevelCount; i++)
            {
                if (i < _dummyStageLevelModelList.Count)
                {
                    var model = _dummyStageLevelModelList[i];
                    model.level = i;
                    modelList.Add(model);
                }
                else
                {
                    StageLevelModel model = new StageLevelModel();
                    model.level = i;
                    modelList.Add(model);
                }
            }

            listModel.stageLevelModelList.Value = modelList;
            _stageLevelListPresenter = PresenterFactory.CreateOrGet<StageLevelListPresenter>();
            _stageLevelListPresenter.Initialize(_stageLevelListView, listModel);
        }
    }
}