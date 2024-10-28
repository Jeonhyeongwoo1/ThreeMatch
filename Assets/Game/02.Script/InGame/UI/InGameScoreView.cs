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
        public Transform ComboCountSpawnPivotTransform => _comboCountSpawnPivotTransform;
        
        [SerializeField] private Image _progressbarImage;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Transform _comboCountSpawnPivotTransform;

        public void UpdateScore(float progress, string score)
        {
            _progressbarImage.fillAmount = progress;
            _scoreText.text = score;
        }
    }
}