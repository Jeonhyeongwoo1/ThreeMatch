using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using UnityEngine;

namespace ThreeMatch.InGame.Presenter
{
    public class GameWinPresenter : BasePresenter
    {
        private GameWinView _gameWinView;
        private MissionModel _missionModel;

        public void Initialize(GameWinView gameWinView, MissionModel missionModel)
        {
            _gameWinView = gameWinView;
            _missionModel = missionModel;
        }

        public async UniTask GameWinProcess()
        {
            UniTaskCompletionSource task = new UniTaskCompletionSource();

            _gameWinView.ShowAndHideCompleteLevelObj(true);
            await UniTask.WaitForSeconds(2f);
            
            _gameWinView.ShowAndHideCompleteLevelObj(false);
            
            var missionViewDataList = _missionModel.missionDataList.Value.Select(missionData => new MissionView.Data
                { missionType = missionData.missionType, count = missionData.removeCount }).ToList();
            _gameWinView.ShowPreCompletedMissionPopupObj(task, missionViewDataList);

            await task.Task;

            _gameWinView.gameObject.SetActive(false);
            _gameWinView.ShowAndHideInGameWinMenuPopup(true);
        }
    }
}