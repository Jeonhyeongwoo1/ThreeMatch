using ThreeMatch.InGame.Interface;
using UniRx;

namespace ThreeMatch.InGame.Model
{
    public class InGameItemModel : IModel
    {
        public ReactiveProperty<int> hammerItemCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> verticalRocketItemCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> horizontalRocketItemCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> shuffleItemCount = new ReactiveProperty<int>();
    }
}