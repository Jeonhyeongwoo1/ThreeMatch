
using System;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;

namespace ThreeMatch.Core
{
    public static class ModelFactory
    {
        private static readonly Dictionary<Type, IModel> ModelDict = new();

        public static T CreateOrGet<T>() where T : IModel, new()
        {
            if (!ModelDict.TryGetValue(typeof(T), out var model))
            {
                model = new T();
                ModelDict.Add(typeof(T), model);
            }

            return (T)model;
        }

        public static void ClearModelDict()
        {
            ModelDict.Clear();
        }
    }   
}