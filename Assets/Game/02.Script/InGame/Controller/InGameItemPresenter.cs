using ThreeMatch.InGame.Core;
using ThreeMatch.InGame.Model;
using ThreeMatch.InGame.UI;

namespace ThreeMatch.InGame.Presenter
{
    public class InGameItemPresenter
    {
        private InGameItemView _inGameItemView;
        private InGameItemModel _inGameItemModel;
        
        public void Initialize(InGameItemView view)
        {
            _inGameItemModel = ModelFactory.CreateOrGet<InGameItemModel>();
            _inGameItemView = view;
        }
    }
}