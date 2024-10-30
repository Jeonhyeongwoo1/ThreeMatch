using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThreeMatch.InGame.Entity
{
    public class ResourceToken : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;

        private Action _onMoveDoneAction;
        private CancellationTokenSource _cts;
        
        public void Spawn(Sprite sprite, Vector3 scale, Vector3 spawnPosition, Action onMoveDoneAction = null)
        {
            _cts = new CancellationTokenSource();
            _sprite.sprite = sprite;
            transform.position = spawnPosition;
            transform.localScale = scale;
            _onMoveDoneAction = onMoveDoneAction;
            gameObject.SetActive(true);
        }

        public async UniTask MoveToRandomlyAroundSpawnPosition(bool wantToBezierMove, Vector3 spawnPosition,
            Vector3 destinationPosition, UniTaskCompletionSource completionSource = null)
        {
            float duration = 0.3f;
            float range = 2f;
            Vector3 movePosition =
                spawnPosition + new Vector3(Random.Range(-range, range), Random.Range(-range, range));

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 lerp = Vector3.Lerp(spawnPosition, movePosition, elapsed / duration);
                transform.position = lerp;
                await UniTask.Yield(cancellationToken: _cts.Token);
            }

            await UniTask.WaitForSeconds(0.7f, cancellationToken: _cts.Token);

            if (wantToBezierMove)
            {
                Vector3 posA = spawnPosition + destinationPosition;
                Vector3 posB = posA * 0.5f;
                Vector3 controlPoint =
                    posA / 2 + new Vector3(Random.Range(-posB.x, posB.x), Random.Range(-posB.y, posB.y));

                await BezierMoveAsync(movePosition, controlPoint, destinationPosition);
            }
            else
            {
                await MoveAsync(movePosition, destinationPosition);
            }

            if (completionSource != null)
            {
                completionSource.TrySetResult();
            }
        }

        private async UniTask MoveAsync(Vector3 from, Vector3 to)
        {
            float duration = .5f;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, elapsed / duration);
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
            
            Sleep();
        }

        public async UniTask BezierMoveAsync(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float duration = 0.5f;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 position = GetBezierPoint(p0, p1, p2, elapsed / duration);
                transform.position = position;
                await UniTask.Yield(cancellationToken: _cts.Token);
            }

            Sleep();
        }

        private void Sleep()
        {
            _cts?.Dispose();
            gameObject.SetActive(false);
            _onMoveDoneAction?.Invoke();
        }
        
        private Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            return Vector3.Lerp(a, b, t);
        }
    }
}
