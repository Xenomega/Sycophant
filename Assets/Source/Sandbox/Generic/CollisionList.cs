using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionList : MonoBehaviour
{
    #region Values
    private Dictionary<GameObject, Collision> _collisions;
    private Dictionary<GameObject, Collision2D> _collisions2D;
    private ulong _lastListRefresh;
    public Dictionary<GameObject, Collision> Collisions
    {
        get
        {
            return _collisions;
        }
    }
    public Dictionary<GameObject, Collision2D> Collisions2D
    {
        get
        {
            return _collisions2D;
        }
    }

    #endregion

    #region Unity Functions
    private void Awake()
    {
        _collisions = new Dictionary<GameObject, Collision>();
        _collisions2D = new Dictionary<GameObject, Collision2D>();
    }
    private void OnCollisionStay(Collision collision)
    {
        // Refresh our list on every new update since exit does not catch destroyed/disabled objects.
        RefreshLookups();

        // Set our collision in our lookup.
        Collisions[collision.gameObject] = collision;
    }
    private void OnCollisionExit(Collision collision)
    {
        // Remove our collision from our lookup.
        Collisions.Remove(collision.gameObject);
    }
    private void OnCollisionStay2D(Collision2D collision2D)
    {
        // Refresh our list on every new update since exit does not catch destroyed/disabled objects.
        RefreshLookups();

        // Set our collision in our lookup.
        Collisions2D[collision2D.gameObject] = collision2D;
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        // Remove our collision from our lookup.
        Collisions2D.Remove(collision.gameObject);
    }
    private void RefreshLookups()
    {
        // If we already updated, stop, otherwise update our update count.
        if (_lastListRefresh == Globals.singleton.PhysicsUpdateCount)
            return;
        else
            _lastListRefresh = Globals.singleton.PhysicsUpdateCount;

        // Refresh our lookups as needed.
        _collisions.Clear();
        _collisions2D.Clear();
    }
    #endregion
}