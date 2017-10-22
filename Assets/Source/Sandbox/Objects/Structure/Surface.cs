using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed internal class Surface : MonoBehaviour
{
    [SerializeField] private bool _killImpactor;
    public bool KillImpactor
    {
        get { return _killImpactor; }
    }

    [SerializeField] private float _bounciness;
    public float Bounciness
    {
        get { return _bounciness; }
    }
    public bool HasBounce
    {
        get { return _bounciness > 0; }
    }
}
