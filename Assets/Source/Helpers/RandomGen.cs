using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Wrapper for UnityEngine's Random generator, which allows for the storing/restoring of state's and editing underlying state variables.
/// Each of this class will have it's own random state.
/// </summary>
public class RandomGen
{
    private State _state;
    /// <summary>
    /// The state of our random generator. Matches Unity, but exposes variables accordingly.
    /// </summary>
    public State state
    {
        get { return _state; }
        set { _state = value; }
    }
    

    /// <summary>
    /// The default constructor for the object.
    /// </summary>
    public RandomGen()
    {
        state = new State();
        ResetState();
    }

    /// <summary>
    /// Returns a random integer number between min [inclusive] and max [exclusive].
    /// </summary>
    /// <param name="min">The minimum [inclusive] number to allow in the range.</param>
    /// <param name="max">The maximum [exclusive] number to allow in the range.</param>
    /// <returns>Returns a random integer number provided between the range.</returns>
    public unsafe int Range(int min, int max)
    {
        // Backup our original state
        UnityEngine.Random.State backupState = UnityEngine.Random.state;

        // Cast our state struct into Unity engine's state struct to set our random state.
        var genState = state;
        UnityEngine.Random.state = *((UnityEngine.Random.State*)&genState);

        // Generate a random number now.
        int result = UnityEngine.Random.Range(min, max);

        // Cast UnityEngine's new state back to our struct.
        var genState2 = UnityEngine.Random.state;
        state = *((State*)&genState2);

        // Restore our random state and return it.
        UnityEngine.Random.state = backupState;
        return result;
    }
    /// <summary>
    /// Returns a random float number between min [inclusive] and max [inclusive].
    /// </summary>
    /// <param name="min">The minimum [inclusive] number to allow in the range.</param>
    /// <param name="max">The maximum [inclusive] number to allow in the range.</param>
    /// <returns>Returns a random float number provided between the range.</returns>
    public unsafe float Range(float min, float max)
    {
        // Backup our original state
        UnityEngine.Random.State backupState = UnityEngine.Random.state;

        // Cast our state struct into Unity engine's state struct to set our random state.
        var genState = state;
        UnityEngine.Random.state = *((UnityEngine.Random.State*)&genState);

        // Generate a random number now.
        float result = UnityEngine.Random.Range(min, max);

        // Cast UnityEngine's new state back to our struct.
        var genState2 = UnityEngine.Random.state;
        state = *((State*)&genState2);

        // Restore our random state and return it.
        UnityEngine.Random.state = backupState;
        return result;
    }
    public void ResetState()
    {
        // Set our state variables to random numbers.
        _state.s0 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _state.s1 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _state.s2 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _state.s3 = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }

    public string GetStateJson()
    {
        // Serialize our object into a json string
        return JsonUtility.ToJson(state);
    }
    public void SetStateJson(string jsonState)
    {
        // Deserialize our json string into the state object.
        _state = JsonUtility.FromJson<State>(jsonState);
    }

    /// <summary>
    /// The state of our random generator. Matches Unity, but exposes variables accordingly.
    /// </summary>
    [Serializable]
    public struct State
    {
        [SerializeField]
        public int s0;
        [SerializeField]
        public int s1;
        [SerializeField]
        public int s2;
        [SerializeField]
        public int s3;
    }
}