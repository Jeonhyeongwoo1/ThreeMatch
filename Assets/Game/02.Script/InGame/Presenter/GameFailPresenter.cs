using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;
using System.Linq;

namespace ThreeMatch.InGame.Presenter
{
    public class GameFailPresenter : BasePresenter
    {
        private GameFailedView _gameFailedView;
        private MissionModel _missionModel;
        
        public void Initialize(GameFailedView view, MissionModel missionModel)
        {
            _gameFailedView = view;
            _missionModel = missionModel;
        }

        public async UniTask GameFailProcess()
        {
            _gameFailedView.ShowAndHideFailTextObj(true);

            await UniTask.WaitForSeconds(2f, cancelImmediately: true);
            
            bool isShowAds = false;
            _gameFailedView.ShowAndHideFailTextObj(false);
            _gameFailedView.ShowAndHideFailPopupObj(true, isShowAds);
        }
    }
}