using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

sealed internal class InvestmentManager : MonoBehaviour
{
    internal static InvestmentManager singleton;

    [SerializeField] private Unlockable[] _unlockableCustumes;
    internal Unlockable[] UnlockableCostumes { get { return _unlockableCustumes; } }

    [SerializeField] private Sprite[] _custumeSprites;
    internal Sprite[]  CostumeSprites
    {
        get { return _custumeSprites; }
        set { _custumeSprites = value; }
    }

    [SerializeField] private Unlockable[] _unlockableEffects;
    internal Unlockable[] UnlockableEffects { get { return _unlockableEffects; } }

    [SerializeField] private GameObject[] _effects;
    internal GameObject[] Effects
    {
        get { return _effects; }
        set { _effects = value; }
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (singleton != null)
        {
            Destroy(this.gameObject);
            return;
        }
        singleton = this;

        DontDestroyOnLoad(this.gameObject);
    }

    internal void Save()
    {
        GameManager.singleton.SaveProfile();
    }

    internal int GetUnlockableIndex(Unlockable unlockable, Unlockable[] unlockables)
    {
        for (int i = 0; i < unlockables.Length; i++)
        {
            if (unlockables[i] == unlockable)
                return i;
        }
        return -1;
    }
}
