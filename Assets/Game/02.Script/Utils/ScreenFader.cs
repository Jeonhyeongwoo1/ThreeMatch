using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.Utils
{
    public class ScreenFader : MonoBehaviour
    {
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration;
        
        public async UniTask FadeOut()
        {
            _fadeImage.raycastTarget = true;
            _fadeImage.DOFade(1, _fadeDuration);
            await UniTask.WaitForSeconds(_fadeDuration, cancelImmediately: true);
        }

        public async UniTask FadeIn()
        {
            _fadeImage.raycastTarget = false;
            _fadeImage.DOFade(0, _fadeDuration);
            await UniTask.WaitForSeconds(_fadeDuration, cancelImmediately: true);
        }
    }
}