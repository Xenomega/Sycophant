using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal abstract class Orientable : MonoBehaviour
{
    #region Unity Functions
    protected virtual void Awake()
    {
        SandboxManager.singleton.OnWorldAngleChanged += OnWorldAngleChanged;
    }
    protected virtual void OnDestroy()
    {
        SandboxManager.singleton.OnWorldAngleChanged -= OnWorldAngleChanged;
    }
    #endregion

    #region Functions
    protected virtual void OnWorldAngleChanged(float worldAngle) { }
    #endregion
}
