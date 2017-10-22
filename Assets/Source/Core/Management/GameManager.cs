using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

sealed internal class GameManager : MonoBehaviour
{
    #region Values
    internal static GameManager singleton;

    [SerializeField] private UISignal _menuUISignal;

    internal const string MENU_SCENE_NAME = "title_menu";
    internal const string GAME_SCENE_NAME = "game_base";
    [SerializeField] private Scene _titleMenu;
    [SerializeField] private Scene _gameScene;

    [SerializeField] private Profile _defaultProfile;
    [SerializeField] private Profile _profile;
    internal Profile Profile { get { return _profile; } }

    private const string PROFILE_SAVE_KEY = "profile_data";
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (singleton != null)
            Destroy(singleton.gameObject);

        singleton = this;
        DontDestroyOnLoad(this.gameObject);

        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        GameSaveManager.Initialize();
        LoadProfile();
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == GAME_SCENE_NAME)
            _profile.RefreshScore();

        GC.Collect();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GAME_SCENE_NAME);
    }

    public void ReloadActiveScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE_NAME);
    }
    internal static void ForceReturnToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE_NAME);
    }

    private void LoadProfile()
    {
        Debug.Log("REMINDER: Saving and loading profile data needs to work field wise, and cannot be a serialization of profile.");
        Profile profile = (Profile)GameSaveManager.GetObject(PROFILE_SAVE_KEY);
        _profile = profile != null ? profile : _defaultProfile;
        PopulateUnlocks();

        if (_menuUISignal != null)
            _menuUISignal.Output(SandboxValue.CoinCount, _profile.Coins.ToString());
    }
    internal void SaveProfile()
    {
        if (_menuUISignal != null)
            _menuUISignal.Output(SandboxValue.CoinCount, _profile.Coins.ToString());

        SetProfileUnlocks();
        GameSaveManager.SetObject(PROFILE_SAVE_KEY, _profile);
    }

    private void PopulateUnlocks()
    {
        for (int i = 0; i < _profile.UnlockedCostumes.Length; i++)
            InvestmentManager.singleton.UnlockableCostumes[i].Unlocked = _profile.UnlockedCostumes[i];

        for (int i = 0; i < _profile.UnlockedEffects.Length; i++)
            InvestmentManager.singleton.UnlockableEffects[i].Unlocked = _profile.UnlockedEffects[i];
    }
    private void SetProfileUnlocks()
    {
        _profile.UnlockedCostumes = new bool[InvestmentManager.singleton.UnlockableCostumes.Length];
        for (int i = 0; i < _profile.UnlockedCostumes.Length; i++)
            _profile.UnlockedCostumes[i] = InvestmentManager.singleton.UnlockableCostumes[i].Unlocked;

        _profile.UnlockedEffects = new bool[InvestmentManager.singleton.UnlockableEffects.Length];
        for (int i = 0; i < _profile.UnlockedEffects.Length; i++)
            _profile.UnlockedEffects[i] = InvestmentManager.singleton.UnlockableEffects[i].Unlocked;
    } 
    #endregion
}
