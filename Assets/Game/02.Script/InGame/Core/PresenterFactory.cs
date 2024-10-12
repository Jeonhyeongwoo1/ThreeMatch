using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Presenter;

namespace ThreeMatch.InGame.Core
{
    public static class PresenterFactory
    {
        private static readonly Dictionary<Type, BasePresenter> _presenterDict = new();

        public static T CreateOrGet<T>() where T : BasePresenter, new()
        {
            if (!_presenterDict.TryGetValue(typeof(T), out var presenter))
            {
                presenter = new T();
                _presenterDict.Add(typeof(T), presenter);
            }

            return (T)presenter;
        }

        public static void Clear()
        {
            _presenterDict.Clear();
        }
    }
}