using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStartingPosition : MonoBehaviour
{
    [SerializeField] private Vector2 _startingPosition;
    private Vector3 WorldStartingPosition
    {
        get
        {
            return this.transform.TransformPoint(_startingPosition);
        }
    }
    private void Start()
    {
        GameCamera gameCamera = FindObjectOfType<GameCamera>();
        gameCamera.PlaceWithOffset(WorldStartingPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(WorldStartingPosition, new Vector3(20, 20, 0));
    }
}
