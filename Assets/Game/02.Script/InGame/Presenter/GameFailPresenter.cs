using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using System.Linq;

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

        public async UniTask GameFailProcess()
        {
            _gameFailedPopup.ShowAndHideFailTextObj(true);

            await UniTask.WaitForSeconds(2f, cancelImmediately: true);
            
            bool isShowAds = false;
            _gameFailedPopup.ShowAndHideFailTextObj(false);
            _gameFailedPopup.ShowAndHideFailPopupObj(true, isShowAds);
        }
    }
}