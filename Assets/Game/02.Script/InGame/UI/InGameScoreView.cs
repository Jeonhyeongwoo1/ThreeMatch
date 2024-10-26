using System.Collections;
using System.Collections.Generic;
using ThreeMatch.InGame.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThreeMatch.InGame.UI
{
    public class InGameScoreView : MonoBehaviour, IView
    {
        [SerializeField] private Image _progressbarImage;
        [SerializeField] private TextMeshProUGUI _scoreText;

        public void UpdateScore(float progress, string score)
        {
            _progressbarImage.fillAmount = progress;
            _scoreText.text = score;
        }
    }
}