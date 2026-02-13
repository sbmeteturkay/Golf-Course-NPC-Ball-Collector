using System;
using UnityEngine;

namespace Game.Core
{
    public class ScoreSystem : MonoBehaviour
    {
        private int currentScore = 0;
    
        public int CurrentScore => currentScore;
    
        public event Action<int> OnScoreChanged;
    
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }
    
        public void ResetScore()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
        }
    }
}