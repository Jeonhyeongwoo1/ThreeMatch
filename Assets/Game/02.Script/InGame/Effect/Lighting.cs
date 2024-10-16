using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Effect
{
    public class Lighting : MonoBehaviour, IPoolable
    {
        public PoolKeyType PoolKeyType { get; set; }

        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _duration = 0.8f;

        private Vector3 _pos1;
        private Vector3 _pos2;

        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }

        public void SetPosition(Vector3 from, Vector3 to)
        {
            _pos1 = from;
            _pos2 = to;
        }

        public void Spawn(Transform spawner)
        {
            transform.position = Vector3.zero;
            gameObject.SetActive(true);
            MoveAsync().Forget();
        }

        private async UniTask MoveAsync()
        {
            int count = 10;
            float elapsed = 0;
            
            while (elapsed < _duration)
            {
                Vector3[] positionList = new Vector3[count];
                _lineRenderer.positionCount = count;
                elapsed += Time.deltaTime;

                for (int i = 0; i < count; i++)
                {
                    positionList[i] = Vector3.Lerp(_pos1, _pos2, (float)i / (count - 1));
                    _lineRenderer.SetPosition(i, positionList[i]);
                }

                for (int i = 0; i < count; i++)
                {
                    positionList[i] = new Vector3(positionList[i].x,
                        positionList[i].y + Mathf.PerlinNoise(Random.value, Random.value) * Random.Range(-1, 1));
                    _lineRenderer.SetPosition(i, positionList[i]);
                }
                
                positionList[0] = _pos1;
                positionList[^1] = _pos2;
                await UniTask.Yield();
            }
            
            ((IPoolable)this).Enqueue();
        }
    }
}