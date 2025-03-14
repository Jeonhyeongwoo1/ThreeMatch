using System;
using ThreeMatch.InGame.Data;
using ThreeMatch.InGame.Interface;
using UnityEngine;

namespace ThreeMatch.InGame.Entity
{
    public class SplashParticle : MonoBehaviour, IPoolable
    {
        public PoolKeyType PoolKeyType { get; set; }

        private ParticleSystem Particle
        {
            get
            {
                if (_particle == null)
                {
                    TryGetComponent(out _particle);
                }

                return _particle;
            }
        }

        [SerializeField] private float _lifeTime = 2f;
        [SerializeField] private bool _useLifeCycle = true;
        [SerializeField] private ParticleResourceConfigData _resourceConfig;

        private ParticleSystem _particle;
        private float _elapsed = 0;

        private void Update()
        {
            if (!_useLifeCycle)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed > _lifeTime)
            {
                ((IPoolable)this).Sleep();
            }
        }

        private void OnDisable()
        {
            _elapsed = 0;
        }

        public void Spawn(Transform spawner, Action callback = null)
        {
            transform.position = spawner.position;
            gameObject.SetActive(true);

            Particle.Play();
        }

        public void SetParticle(CellImageType cellImageType)
        {
            int index = (int)cellImageType;
            Sprite sprite = _resourceConfig.CellSpriteArray[index];
            Particle.textureSheetAnimation.SetSprite(0, sprite);
        }

        public T Get<T>() where T : MonoBehaviour
        {
            return this as T;
        }
    }
}