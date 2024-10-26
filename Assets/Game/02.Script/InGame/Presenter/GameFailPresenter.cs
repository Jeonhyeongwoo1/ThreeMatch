using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;

namespace ThreeMatch.InGame.Presenter
{
    public class GameFailPresenter : BasePresenter
    {
        private GameFailedPopup _gameFailedPopup;
        private MissionModel _missionModel;
        
        public void Initialize(GameFailedPopup popup, MissionModel missionModel)
        {
            _gameFailedPopup = popup;
            _missionModel = missionModel;
        }

        public async UniTask GameFailProcess(int starCount, int stageLevel)
        {
            _gameFailedPopup.ShowAndHideFailTextObj(true);

            await UniTask.WaitForSeconds(2f, cancelImmediately: true);
            
            _gameFailedPopup.ShowAndHideFailTextObj(false);
            _gameFailedPopup.ShowAndHideFailPopupObj(true, starCount, stageLevel.ToString());
        }
    }
}