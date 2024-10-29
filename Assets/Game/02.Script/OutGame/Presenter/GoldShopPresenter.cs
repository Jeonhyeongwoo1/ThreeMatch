using System;
using ThreeMatch.InGame.Presenter;
using ThreeMatch.Manager;
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
            
            _popup.Initialize(OnBuyProduct, null);
        }

        private void OnBuyProduct(int productId)
        {
            IAPManager.Instance.BuyConsumable();
        }

        public void OpenGoldShopPopup()
        {
            _popup.Open();
        }
    }
}