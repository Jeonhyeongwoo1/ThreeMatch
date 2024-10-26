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
        private GameWinPopup _gameWinPopup;
        private MissionModel _missionModel;

        public void Initialize(GameWinPopup gameWinPopup, MissionModel missionModel)
        {
            _gameWinPopup = gameWinPopup;
            _missionModel = missionModel;
        }

        public async UniTask GameWinProcess(int stageLevel, int starCount)
        {
            UniTaskCompletionSource task = new UniTaskCompletionSource();

            _gameWinPopup.ShowAndHideCompleteLevelObj(true);
            await UniTask.WaitForSeconds(2f);
            
            _gameWinPopup.ShowAndHideCompleteLevelObj(false);
            
            var missionViewDataList = _missionModel.missionDataList.Value.Select(missionData => new MissionView.Data
                { missionType = missionData.missionType, count = missionData.removeCount }).ToList();
            _gameWinPopup.ShowPreCompletedMissionPopupObj(task, missionViewDataList);

            await task.Task;

            // _gameWinPopup.gameObject.SetActive(false);
            _gameWinPopup.ShowAndHideInGameWinMenuPopup(true, starCount, stageLevel.ToString());
        }
    }
}