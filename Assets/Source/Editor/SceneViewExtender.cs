using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Globalization;
using System;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class SceneViewExtender : Editor
{
    // TODO: Undo shit
    #region Values
    public static Vector3 SceneCameraPivot;
    public static SceneView SceneView;
    public static RaycastHit MouseRay
    {
        get
        {
            // Cast a ray into the scene view to determine surface attributes
            Event currentEvent = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            RaycastHit raycastHit = new RaycastHit();
            Physics.Raycast(ray, out raycastHit);
            return raycastHit;
        }
    }
    public static Quaternion LookAtSceneCamera(Vector3 objectPosition)
    {
        if (objectPosition != SceneView.camera.transform.position)
            return Quaternion.LookRotation(objectPosition - SceneView.camera.transform.position);
        else
            return Quaternion.identity;
    }

    public static GenericMenu ContextMenu = new GenericMenu();
    public static bool MouseInputPassive;
    public static Color DefaultHandleColor = new Color32(255, 255, 255, 60);
    public static float CameraTilt;
    public class Ruler
    {
        public Axis Axis;
        public Vector3 Position;
        public int Extent;
        public Ruler(Vector3 position, int extent, Axis axis)
        {
            Position = position;
            Extent = extent;
            Axis = axis;
        }
    }
    public static List<Ruler> Rulers = new List<Ruler>();
    #endregion

    #region Initialization / Update
    // Initialize
    static SceneViewExtender()
    {
        SceneView.onSceneGUIDelegate += OnScene;
    }
    private static void OnScene(SceneView sceneView)
    {
        SceneView = sceneView;
        SceneCameraPivot = SceneView.pivot;
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.Alpha1)
            CameraTilt -= 90;
        if (currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.Alpha2)
            CameraTilt += 90;

        sceneView.rotation = Quaternion.Euler(0, 0, CameraTilt);
    }
    #endregion
    
    
}