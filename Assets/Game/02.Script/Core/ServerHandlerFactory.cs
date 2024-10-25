using System;
using System.Collections.Generic;
using ThreeMatch.Firebase;
using ThreeMatch.Server;
using UnityEngine;

namespace ThreeMatch.Core
{
    public class ServerHandlerFactory
    {
        private static readonly Dictionary<Type, ServerRequestHandler> ServerRequestHandlerDict = new();

        public static T Create<T>(params object[] injectParams) where T : ServerRequestHandler
        {
            if (!ServerRequestHandlerDict.TryGetValue(typeof(T), out var handler))
            {
                handler = (T)Activator.CreateInstance(typeof(T), injectParams);
                ServerRequestHandlerDict.Add(typeof(T), handler);
            }

            return (T)handler;
        }

        public static T Get<T>() where T : ServerRequestHandler
        {
            if (ServerRequestHandlerDict.TryGetValue(typeof(T), out var handler))
            {
                return (T) handler;
            }
            
            Debug.LogError($"Failed Get {typeof(T)}");
            return null;
        }
        
        public static void ClearDict()
        {
            ServerRequestHandlerDict.Clear();
        }

        public static void InitializeServerHandlerRequest(FirebaseController firebaseController)
        {
            ClearDict();
            Create<ServerUserRequestHandler>(firebaseController);
            Create<ServerStageRequestHandler>(firebaseController);
        }
    }
}