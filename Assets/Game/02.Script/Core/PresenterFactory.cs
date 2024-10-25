using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Presenter;

namespace ThreeMatch.Core
{
    public static class PresenterFactory
    {
        private static readonly Dictionary<Type, BasePresenter> PresenterDict = new();

        public static T CreateOrGet<T>() where T : BasePresenter, new()
        {
            if (!PresenterDict.TryGetValue(typeof(T), out var presenter))
            {
                presenter = new T();
                PresenterDict.Add(typeof(T), presenter);
            }

            return (T)presenter;
        }

        public static void Clear()
        {
            PresenterDict.Clear();
        }
    }
}