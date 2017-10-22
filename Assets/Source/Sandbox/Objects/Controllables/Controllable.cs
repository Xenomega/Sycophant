using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Describes an object which is controllable by a player and controller instance.
/// </summary>
internal class Controllable : MonoBehaviour
{
    #region Values
    protected Player _player;
    protected Controller _controller;
    private bool _eventsState;

    /// <summary>
    /// The player object which assumes ownership over this object.
    /// </summary>
    internal Player Player { get { return _player; } }
    /// <summary>
    /// The controller object which assumes control over this object.
    /// </summary>
    internal Controller Controller { get { return _controller; } }
    #endregion

    #region Functions
    /// <summary>
    /// Sets the controllable aspects for this object, given a player object.
    /// </summary>
    /// <param name="player">The player object to assume ownership of this object.</param>
    internal void SetAspects(Player player)
    {
        SetAspects(player, player.Controller);
    }
    /// <summary>
    /// Sets the controllable aspects for this object, given a controllable object to copy aspects from.
    /// </summary>
    /// <param name="controllable">The controllable object to copy aspects from.</param>
    internal void SetAspects(Controllable controllable)
    {
        SetAspects(controllable._player, controllable._controller);
    }
    /// <summary>
    /// Sets the controllable aspects for this object, given a player and controller object.
    /// </summary>
    /// <param name="player">The player object to assume ownership of this object.</param>
    /// <param name="controller">The controller object to assume control of this object.</param>
    internal virtual void SetAspects(Player player, Controller controller)
    {
        // Set the controllable aspects accordingly.
        _player = player;
        _controller = controller;
    }
    /// <summary>
    /// Sets input event handlers, given a state.
    /// </summary>
    /// <param name="state">The state which indicates whether to add or remove event handlers.</param>
    /// <returns>Indicates whether input event handler state has changed.</returns>
    internal virtual bool SetInputEvents(bool state)
    {
        // If the state is unchanged, return false.
        if (state == _eventsState)
            return false;

        // Otherwise we set the new state.
        _eventsState = state;
        return true;
    }
    #endregion
}
