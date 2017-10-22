using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
sealed internal class Profile
{
    [SerializeField] private int _coins;
    internal int Coins { get { return _coins; } set { _coins = value; } }
    internal int Score { get; set; }
    internal int HighScore { get; set; }
    [SerializeField] private bool[] _unlockedCostumes;
    internal bool[] UnlockedCostumes{ get { return _unlockedCostumes; } set { _unlockedCostumes = value; } }
    [SerializeField] private bool[] _unlockedEffects;
    internal bool[] UnlockedEffects { get { return _unlockedEffects; } set { _unlockedEffects = value; } }

    [SerializeField] internal BipedDesign[] bipedDesigns;

    internal void RefreshScore()
    {
        if (Score > HighScore)
            HighScore = Score;
        Score = 0;
    }
}
