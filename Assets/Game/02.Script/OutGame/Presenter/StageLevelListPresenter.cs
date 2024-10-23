using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.View;
using UnityEngine;

namespace ThreeMatch.OutGame.Presenter
{
    public class StageLevelListPresenter : BasePresenter
    {
        private StageLevelListView _listView;
        private StageLevelListModel _listModel;
        private List<StageLevelPresenter> _stageLevelPresenterList;

        public void Initialize(StageLevelListView view, StageLevelListModel model)
        {
            _listView = view;
            _listModel = model;
            
            List<StageLevelView> list = _listView.StageLevelViewList;
            _stageLevelPresenterList = new List<StageLevelPresenter>(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                StageLevelView levelView = list[i];
                StageLevelModel levelModel = model.stageLevelModelList.Value[i];
                StageLevelPresenter presenter = new StageLevelPresenter(levelModel, levelView);
                _stageLevelPresenterList.Add(presenter);
            }
            
            foreach (StageLevelPresenter presenter in _stageLevelPresenterList)
            {
                presenter.Initialize();
            }
        }

        public void UnLockStageLevel(int level)
        {
            var stageLevelPresenter = _stageLevelPresenterList[level];
            stageLevelPresenter.Unlock();
        }
    }
}