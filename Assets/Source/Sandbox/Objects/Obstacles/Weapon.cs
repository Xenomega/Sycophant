using System.Collections;
using System.Collections.Generic;
using UnityEngine;

sealed internal class Weapon : MonoBehaviour
{
    #region Values
    internal bool AllowFire
    {
        get; set;
    }
    [SerializeField] private AudioSource _fireSound;
    [SerializeField] private GameObject _ejection;

    [SerializeField] private float _fireRecoveryDelay;
    private float _fireRecoveredTime;

    internal bool Recovering { get { return Time.time < _fireRecoveredTime; } }
    #endregion

    #region Unity Functions
    private void Update()
    {
        UpdateFire();
    }
    #endregion

    #region Functions
    private void UpdateFire()
    {
        if (!AllowFire || Recovering)
            return;
        _fireRecoveredTime = Time.time + _fireRecoveryDelay;

        Eject();
    }

    private void Eject()
    {
        if (_fireSound != null)
            _fireSound.Play();
        GameObject newEjectionGameObject = Instantiate(_ejection, this.transform.position, this.transform.rotation, Globals.singleton.containers.projectiles);
    } 
    #endregion
}
