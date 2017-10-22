using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]

sealed internal class Trigger : MonoBehaviour
{
    #region Values
    internal int ChamberNumber { get; set; }
    internal Chamber Chamber { get; set; }
    [SerializeField] private bool _allowOneEntry = true;
    private bool _hasBeenEntered;
    public bool HasBeenEntered
    {
        get { return _hasBeenEntered; }
        set { _hasBeenEntered = value; }
    } 
    #endregion

    #region Unity Functions
    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        ProcessEntry(collider2D, true);
    }
    private void OnTriggerExit2D(Collider2D collider2D)
    {
        ProcessEntry(collider2D, false);
    } 
    #endregion

    #region Functions
    private void ProcessEntry(Collider2D collider2D, bool enter)
    {
        if (collider2D.gameObject.layer != Globals.BIPED_LAYER)
            return;

        if (enter)
        {
            Chamber.SetWeaponsActive(true);
            if (_hasBeenEntered && _allowOneEntry)
                return;

            Biped biped = collider2D.gameObject.GetComponent<Biped>();
            biped.Player.AddPoints(PointType.Chamber);

            AdvanceGeneration();
            _hasBeenEntered = true;
        }
        else if (collider2D.gameObject.activeSelf)
        {
            Chamber.SetWeaponsActive(false);
        }
    }

    private void AdvanceGeneration()
    {
        // Update our autogeneration with respect to the chamber number we've entered.
        ChamberManager.singleton.UpdateAutoGeneration(ChamberNumber);
    } 
    #endregion
}
