using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.View;

namespace ThreeMatch.OutGame.Presenter
{
    public class StageLevelListPresenter : BasePresenter
    {
        private StageLevelListView _listView;
        private StageLevelListModel _listModel;
        private List<StageLevelPresenter> _stageLevelPresenterList;

        public void Initialize(StageLevelListView view, StageLevelListModel stageLevelListModel, Action<int> onSelectStageLevel)
        {
            _listView = view;
            _listModel = stageLevelListModel;
            
            List<StageLevelView> list = _listView.StageLevelViewList;
            _stageLevelPresenterList = new List<StageLevelPresenter>(list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                StageLevelView levelView = list[i];
                StageLevelModel levelModel = stageLevelListModel.stageLevelModelList.Value[i];
                StageLevelPresenter presenter = new StageLevelPresenter(levelModel, levelView, onSelectStageLevel);
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