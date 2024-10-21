using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.InGame.Core
{
    public class MainDispatcher : MonoBehaviour
    {
        private static Queue<Action> _queueAction = new Queue<Action>();

        public static void Enqueue(Action action)
        {
            lock (_queueAction)
            {
                _queueAction.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (_queueAction)
            {
                while (_queueAction.Count > 0)
                {
                    _queueAction.Dequeue().Invoke();
                }
            }
        }
    }
}