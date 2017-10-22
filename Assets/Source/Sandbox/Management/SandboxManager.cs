using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

sealed internal class SandboxManager : MonoBehaviour
{
    internal static SandboxManager singleton;

    #region Values
    [SerializeField] private Player _player;
    public Player Player
    {
        get { return _player; }
    }
    internal event UnityAction<float> OnWorldAngleChanged;
    private float _worldAngle = 0;
    internal float WorldAngle
    {
        get { return _worldAngle; }
    }
    [SerializeField] private float _gravity = 20f;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        OnWorldAngleChanged.Invoke(_worldAngle);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        singleton = this;
        Physics2D.gravity = -Vector2.up * _gravity;
    }

    internal void RotateWorld(float direction)
    {
        if (OnWorldAngleChanged != null)
        {
            float angleAddition = 90 * direction;
            _worldAngle += angleAddition;
            if (Mathf.Abs(_worldAngle) >= 360)
                _worldAngle = 0;
            Physics2D.gravity = Quaternion.Euler(0, 0, angleAddition) * Physics2D.gravity;
            OnWorldAngleChanged.Invoke(_worldAngle);
        }
    }

    public void RestartGame()
    {
        // This method is used because the ui cant connect with a persistent singleton
        GameManager.singleton.ReloadActiveScene();
    }
    public void ReturnToMenu()
    {
        // This method is used because the ui cant connect with a persistent singleton
        GameManager.singleton.ReturnToMenu();
    } 
    #endregion
}
