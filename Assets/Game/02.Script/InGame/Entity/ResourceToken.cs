using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ThreeMatch.InGame.Entity
{
    public class ResourceToken : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sprite;

        private Vector3 _spawnPosition;
        private Vector3 _destinationPosition;
        private Vector3 _controlPoint;
        private float _elapsed = 0;
        private Action<ResourceToken> _onMoveDoneAction;

        public void Spawn(Sprite sprite, Vector3 scale, Vector3 spawnPosition, Vector3 destinationPosition, Action<ResourceToken> onMoveDoneAction)
        {
            _sprite.sprite = sprite;
            transform.position = spawnPosition;
            transform.localScale = scale;
            _spawnPosition = spawnPosition;
            _destinationPosition = destinationPosition;
            Vector3 posA = spawnPosition + destinationPosition;
            Vector3 posB = posA * 0.5f;
            _controlPoint = posA / 2 + new Vector3(Random.Range(-posB.x, posB.x), Random.Range(-posB.y, posB.y));
            _elapsed = 0;
            _onMoveDoneAction = onMoveDoneAction;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            float duration = .5f;
            _elapsed += Time.deltaTime;
            if (_elapsed < duration)
            {
                Vector3 position = GetBezierPoint(_spawnPosition, _controlPoint, _destinationPosition,
                    _elapsed / duration);

                transform.position = position;
            }

            if (_elapsed > duration)
            {
                _elapsed = 0;
                Sleep();
            }
        }

        private void Sleep()
        {
            gameObject.SetActive(false);
            _onMoveDoneAction?.Invoke(this);
        }
        
        private Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            return Vector3.Lerp(a, b, t);
        }
    }
}
