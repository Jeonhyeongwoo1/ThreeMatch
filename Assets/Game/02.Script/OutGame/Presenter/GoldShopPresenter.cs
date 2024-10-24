using ThreeMatch.InGame.Presenter;
using ThreeMatch.OutGame.Data;
using ThreeMatch.OutGame.Popup;

namespace ThreeMatch.OutGame.Presenter
{
    public class GoldShopPresenter : BasePresenter
    {
        private UserModel _model;
        private GoldShopPopup _popup;

        public void Initialize(UserModel model, GoldShopPopup popup)
        {
            _model = model;
            _popup = popup;
        }

        public void OpenGoldShopPopup()
        {
            _popup.Open();
        }
    }
}