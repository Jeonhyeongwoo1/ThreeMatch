using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ThreeMatch.Manager
{
    public static class EventManager
    {
        static EventManager()
        {
            EventContainer.SilentRegisterEvent();
        }
        
        private static readonly Dictionary<string, Subject<Unit>> _eventDict = new();

        public static void Register(string eventName)
        {
            if (_eventDict.ContainsKey(eventName))
            {
                Debug.Log($"Subject already exists for event: {eventName}");
                return;
            }

            _eventDict[eventName] = new Subject<Unit>();
        }

        public static IDisposable Subscribe(string eventName, Action action)
        {
            if (_eventDict.TryGetValue(eventName, out var subject))
            {
                return subject.Subscribe(_ => action.Invoke());
            }

            Debug.LogError($"No registered event: {eventName}");
            return null;
        }

        public static void RaiseEvent(string eventName)
        {
            if (_eventDict.TryGetValue(eventName, out var subject))
            {
                subject.OnNext(Unit.Default);
            }
            else
            {
                Debug.LogError($"Failed to raise event: {eventName}");
            }
        }

        public static void DisposeSubject(string eventName)
        {
            if (_eventDict.TryGetValue(eventName, out var subject))
            {
                subject.Dispose();
                _eventDict.Remove(eventName);
            }
            else
            {
                Debug.LogWarning($"Failed to get subject for event: {eventName}");
            }
        }
    }

}