using System;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
using ThreeMatch.Shared.Popup;

namespace ThreeMatch.Shared.Presenter
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