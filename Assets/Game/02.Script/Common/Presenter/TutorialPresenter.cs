using System;
using ThreeMatch.Common.Popup;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;

namespace ThreeMatch.Common.Presenter
{
    public class TutorialPresenter : BasePresenter
    {
        private TutorialPopup _popup;
        private IDisposable _disposable;
        
        public void Initialize(TutorialPopup popup)
        {
            _popup = popup;
            _disposable?.Dispose();
            _disposable = EventManager.Subscribe(nameof(OpenPopup), OpenPopup);
        }

        public void OpenPopup()
        {
            _popup.OpenPopup();
        }

        public void ClosePopup()
        {
            _popup.ClosePopup();
        }
    }
}