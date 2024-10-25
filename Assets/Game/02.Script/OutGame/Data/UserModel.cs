using System;
using ThreeMatch.InGame.Interface;
using UniRx;

namespace ThreeMatch.OutGame.Data
{
    public class UserModel : IModel
    {
        public ReactiveProperty<int> heart = new ReactiveProperty<int>();
        public ReactiveProperty<long> money = new ReactiveProperty<long>();

        public ReactiveProperty<DateTime> heartRechargeTime = new ReactiveProperty<DateTime>();
        public string userId;
        public bool isAnonymous;
    }
}