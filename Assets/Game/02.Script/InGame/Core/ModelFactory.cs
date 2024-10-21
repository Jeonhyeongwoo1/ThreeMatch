
using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;

namespace ThreeMatch.InGame.Core
{
    public static class ModelFactory
    {
        private static readonly Dictionary<Type, IModel> _modelDict = new();

        public static T CreateOrGet<T>() where T : IModel, new()
        {
            if (!_modelDict.TryGetValue(typeof(T), out var model))
            {
                model = new T();
                _modelDict.Add(typeof(T), model);
            }

            return (T)model;
        }

        public static void ClearModelDict()
        {
            _modelDict.Clear();
        }
    }   
}