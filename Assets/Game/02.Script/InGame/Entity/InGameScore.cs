using ThreeMatch.InGame.Manager;
using ThreeMatch.InGame.UI;

namespace ThreeMatch.InGame.Entity
{
    public class InGameScore
    {
        private int _aimScore;
        private int _currentScore;
        private InGameScoreView _inGameScoreView;
        
        public InGameScore(int aimScore)
        {
            _aimScore = aimScore;
            _currentScore = 0;

            _inGameScoreView = UIManager.Instance.GetView<InGameScoreView>();
            _inGameScoreView?.UpdateScore(0, 0.ToString());
        }

        public void AddScore(int score, int comboCount)
        {
            _currentScore += score;

            if (comboCount > 0)
            {
                var pool = ObjectPoolManager.Instance.GetPool(PoolKeyType.ComboCountText);
                var comboBehaviour = pool.Get<ComboBehaviour>();
                comboBehaviour.SetComboCount(comboCount.ToString());
                comboBehaviour.Spawn(_inGameScoreView.ComboCountSpawnPivotTransform);
            }

            float ratio = (float)_currentScore / _aimScore;
            _inGameScoreView.UpdateScore(ratio, _currentScore.ToString());
        }

        public int GetStarCount()
        {
            float[] starRatioArray = { 0.2f, 0.5f, 1f};
            float ratio = (float)_currentScore / _aimScore;

            int starCount = 0;
            foreach (float value in starRatioArray)
            {
                if (value <= ratio)
                {
                    starCount++;
                }
            }

            return starCount;
        }
    }
}