using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.View;

namespace ThreeMatch.OutGame.Presenter
{
    public class StageLevelPresenter
    {
        private StageLevelModel _model;
        private StageLevelView _view;

        public StageLevelPresenter(StageLevelModel model, StageLevelView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _view.UpdateUI(_model.isLock, _model.starCount, _model.level);
        }

        public void Unlock()
        {
            _model.isLock = false;
            _view.UpdateUI(_model.isLock, _model.starCount, _model.level);
        }
    }
}