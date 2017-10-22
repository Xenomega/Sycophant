using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

sealed public class AdManager : MonoBehaviour
{
    internal AdManager singleton;
    private const string PLACEMENT_ID_VIDEO = "rewardedVideoZone";

    private void Awake()
    {
        singleton = this;
    }
    
    public void ShowRewardVideo()
    {
#if UNITY_ADS
        ShowOptions showOptions = new ShowOptions();
        showOptions.resultCallback = RewardVideoCallback;
        Advertisement.Show(PLACEMENT_ID_VIDEO, showOptions);
#endif
    }

#if UNITY_ADS
    private void RewardVideoCallback(ShowResult showResult)
    {
        switch (showResult)
        {
            case ShowResult.Finished:
                break;
            case ShowResult.Skipped:
                break;
            case ShowResult.Failed:
                break;
        }
    }
#endif
}
