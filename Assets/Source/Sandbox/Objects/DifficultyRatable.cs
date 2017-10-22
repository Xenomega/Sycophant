using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyRatable : MonoBehaviour
{
    [SerializeField]
    protected Difficulty _difficulty;
    internal Difficulty Difficulty {get{ return _difficulty; }}
}
