using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal abstract class Vibration
{
    /// <summary>
    /// Defines the id of the controller that is supposed to recieve vibration.
    /// </summary>
    internal int controllerId;
    /// <summary>
    /// Defines the intensity of the vibration.
    /// </summary>
    internal float intensity;
    /// <summary>
    /// Defines the duration of the vibration.
    /// </summary>
    internal float startTime;
    /// <summary>
    /// Defines the remaining duration of the vibration.
    /// </summary>
    internal float killTime;

    /// <summary>
    /// Sets vibration effects for a provided controller with a given intensity and duration.
    /// </summary>
    /// <param name="controllerID">The controller to apply vibration to.</param>
    /// <param name="intensity">The intensity of the vibration to apply.</param>
    /// <param name="duration">The duration in seconds for the vibration effect.</param>
    internal Vibration(int controllerID, float intensity, float duration)
    {
        controllerId = controllerID;
        this.intensity = intensity;
        startTime = Time.time;
        killTime = Time.time + duration;
    }
}

