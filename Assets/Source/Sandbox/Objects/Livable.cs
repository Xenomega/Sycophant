using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class Livable : MonoBehaviour
{
    #region Values
    [SerializeField] protected float _lifeSpan;
    #endregion

    #region Unity Functions
    protected virtual void Awake()
    {
        Destroy(this.gameObject, _lifeSpan);
    }
    #endregion
}
