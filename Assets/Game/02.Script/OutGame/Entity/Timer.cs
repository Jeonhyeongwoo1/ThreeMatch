using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ThreeMatch.OutGame.Entity
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private string _prefixText;

        private CancellationTokenSource _timerCts;
        
        public void StartTimer(DateTime finishTime, Action done)
        {
            SafeCancelTimerTokenSource();
            _timerCts = new CancellationTokenSource();
            StartTimerAsync(_timerCts.Token, finishTime, done).Forget();
        }

        private void SafeCancelTimerTokenSource()
        {
            if (_timerCts != null)
            {
                _timerCts.Cancel();
                _timerCts = null;
            }
        }

        public void UpdateTimerText(string value)
        {
            _timerText.text = value;
        }

        public void StopTimer()
        {
            SafeCancelTimerTokenSource();
        }

        private async UniTaskVoid StartTimerAsync(CancellationToken timerCts, DateTime finishTime, Action done)
        {
            DateTime startTime = DateTime.UtcNow;
            TimeSpan timeSpan = TimeSpan.Zero;
            
            timeSpan = finishTime - startTime;
            _timerText.text = _prefixText + timeSpan.ToString("mm':'ss");
            
            do
            {
                await UniTask.WaitForSeconds(1f, cancellationToken: timerCts);
                timeSpan = finishTime - startTime;
                _timerText.text = _prefixText + timeSpan.ToString("mm':'ss");
                startTime = DateTime.UtcNow;
            } while (timeSpan.TotalSeconds > 0);
            
            done?.Invoke();
        }
    }
}