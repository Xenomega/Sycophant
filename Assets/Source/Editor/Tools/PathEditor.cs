using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Path))]
public class PathEditor : SceneViewExtender
{
    // Controls
    // Control: Remove 
    // Shift:  Snap 
    // ALt: Directional Movement

    // TODO: Ignore direction on freemove


    #region Values
    // The path to adjust
    public static Path SelectedPath;
    /// <summary>
    /// Defines the range in which the tangent handle will snap to 0.
    /// </summary>
    const float snapDistance = 0.2f;

    private enum RevertType
    {
        All,
        Path,
        Position,
        Rotation,
        Scale
    }
    #endregion

    #region Handle Attrubutes
    private Color handleColor = Color.grey;
    private Color handleLineColor = Color.grey;
    private Color nodeColor = Color.grey;
    private Color addNodeColor = Color.cyan;
    private Color linePreviewColor = Color.yellow;
    private Color deleteColor = Color.red;

    const float handleSize = 0.1f;
    const float addNodeSize = 0.1f;
    const float nodeSize = 0.1f;
    const float deleteSize = 0.1f;
    private bool preventFreemoveZAdjustment = true;

    // Inspector
    Path.Node lastModifiedNode;
    private bool inspectorCurvesExpanded = false;
    #endregion

    [MenuItem("GameObject/Create Other/Path/ Circle Curve")]
    static void CreateCircleCurve()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = "new_path";
        Path path = gameObject.AddComponent<Path>();
        gameObject.transform.position = SceneViewExtender.SceneCameraPivot;

        path.Nodes = new List<Path.Node>() 
        {
            new Path.Node(new Vector3(0, 1, 0), new Vector3(0.56f, 0, 0), new Vector3(0.56f, 0, 0)),
            new Path.Node(new Vector3(1, 0, 0), new Vector3(0, -0.56f, 0), new Vector3(0, -0.56f, 0)), 
            new Path.Node(new Vector3(0, -1, 0),new Vector3(-0.56f, 0, 0), new Vector3(-0.56f, 0, 0)), 
            new Path.Node(new Vector3(-1, 0, 0), new Vector3(0, 0.56f, 0), new Vector3(0, 0.56f, 0))
        };
    }

    #region Unity Functions
    private void OnEnable()
    {
        // Define shape to edit
        SelectedPath = (Path)target;

        // Backup on first load
        if (SelectedPath.NodesBackup.Count == 0)
            Backup();
    }
    private void OnDisable()
    {
        // Define shape to edit
        SelectedPath = null;
    }
    private void OnSceneGUI()
    {
        if (SelectedPath != null)
        {
            GenericMenu();
            TransformCurve();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(SelectedPath);
    }
    private bool previousInspectorPassFlat;
    public override void OnInspectorGUI()
    {
        SelectedPath.Closed = EditorGUILayout.Toggle("Closed", SelectedPath.Closed);
        SelectedPath.Flat = EditorGUILayout.Toggle("Flat", SelectedPath.Flat);

        if (SelectedPath.Flat && !previousInspectorPassFlat)
            for (int i = 0; i < SelectedPath.Nodes.Count; i++)
            {
                SelectedPath.Nodes[i].TangentA.z = 0;
                SelectedPath.Nodes[i].TangentB.z = 0;
            }
        previousInspectorPassFlat = SelectedPath.Flat;

        SelectedPath.Resolution = EditorGUILayout.IntSlider("Curve Resolution", SelectedPath.Resolution, 2, 120);
        inspectorCurvesExpanded = EditorGUILayout.Foldout(inspectorCurvesExpanded, "Path Curves");

        if (inspectorCurvesExpanded)
            {
                for (int i = 0; i < SelectedPath.Nodes.Count; i++)
                {
                    Path.Node node = SelectedPath.Nodes[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    bool selectedNode = EditorGUILayout.Foldout(lastModifiedNode == node, "Curve " + i);
                    GUILayout.EndHorizontal();
                    if (selectedNode)
                    {
                        node.TangentType = (Path.Node.Type)EditorGUILayout.EnumPopup("Type", node.TangentType);
                        node.Position = EditorGUILayout.Vector3Field("Postion", node.Position);
                        node.TangentA = EditorGUILayout.Vector3Field("Tangent A", node.TangentA);
                        node.TangentB = EditorGUILayout.Vector3Field("Tangent B", node.TangentB);

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("+", GUILayout.Width(30)))
                        {
                            EditorGUI.BeginChangeCheck();
                            Undo.RecordObject(SelectedPath, "Add Path Node");
                            // Remove node
                            SelectedPath.Add(i, 0.5f);
                            EditorGUI.EndChangeCheck();
                        }
                        if (SelectedPath.Nodes.Count > 2)
                        {
                            if (GUILayout.Button("X", GUILayout.Width(30)))
                            {
                                EditorGUI.BeginChangeCheck();
                                Undo.RecordObject(SelectedPath, "Remove Path Node");
                                // Remove node
                                SelectedPath.Remove(node);

                                EditorGUI.EndChangeCheck();
                            }
                            if (node != SelectedPath.Nodes[SelectedPath.Nodes.Count - 1])
                            {
                                if (GUILayout.Button(@"\/"))
                                {
                                    SelectedPath.Remove(node);
                                    SelectedPath.Nodes.Insert(i + 1, node);
                                }
                            }
                            if (node != SelectedPath.Nodes[0])
                            {
                                if (GUILayout.Button(@"/\"))
                                {
                                    SelectedPath.Remove(node);
                                    SelectedPath.Nodes.Insert(i - 1, node);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                }
        }
    }
    #endregion

    #region Context Menu
    private void GenericMenu()
    {
        ContextMenu.AddItem(new GUIContent("Path Tool/Align Local/X"), false, AlignGenericMenuCallback, 0);
        ContextMenu.AddItem(new GUIContent("Path Tool/Align Local/Y"), false, AlignGenericMenuCallback, 1);
        ContextMenu.AddItem(new GUIContent("Path Tool/Align Local/Z"), false, AlignGenericMenuCallback, 2);
        ContextMenu.AddSeparator("Path Tool/");

        ContextMenu.AddItem(new GUIContent("Path Tool/Backup"), false, GenericMenuCallback, 0);

        // Revert
        ContextMenu.AddItem(new GUIContent("Path Tool/Revert/Path"), false, RevertGenericMenuCallback, 1);
        ContextMenu.AddItem(new GUIContent("Path Tool/Revert/Position"), false, RevertGenericMenuCallback, 2);
        ContextMenu.AddItem(new GUIContent("Path Tool/Revert/Rotation"), false, RevertGenericMenuCallback, 3);
        ContextMenu.AddItem(new GUIContent("Path Tool/Revert/Scale"), false, RevertGenericMenuCallback, 4);
        ContextMenu.AddSeparator("Path Tool/Revert/");
        ContextMenu.AddItem(new GUIContent("Path Tool/Revert/All"), false, RevertGenericMenuCallback, 0);
    }
    private void AlignGenericMenuCallback(object obj)
    {
        Align((Axis)obj);
    }
    public void RevertGenericMenuCallback(object obj)
    {
        Revert((RevertType)obj);
    }
    private void GenericMenuCallback(object obj)
    {
        switch ((int)obj)
        {
            case 0:
                Backup();
                break;
        }
    }
    #endregion

    #region Core Functions
    private void TransformCurve()
    {
        Event currentEvent = Event.current;
        bool showingDeleteInput = currentEvent.control;
        bool freeMoveNodeInput = !currentEvent.alt;
        bool snapInput = currentEvent.shift;

        Vector3 position = SelectedPath.transform.position;
        Quaternion rotation = SelectedPath.transform.rotation;
        Vector3 scale = SelectedPath.transform.localScale;

        #region Display Path
        // Display curves
        if (!showingDeleteInput)
            Handles.color = linePreviewColor;
        else
            Handles.color = deleteColor;

        Vector3[] curvePreviewPoints = SelectedPath.CurvePoints(true, true, true);
        // Render a few times for thickness - setting a width looks like shit
        Handles.DrawAAPolyLine(curvePreviewPoints);
        Handles.DrawAAPolyLine(curvePreviewPoints);
        Handles.DrawAAPolyLine(curvePreviewPoints);
        Handles.DrawAAPolyLine(curvePreviewPoints);
        Handles.DrawAAPolyLine(curvePreviewPoints);
        #endregion

        for (int i = 0; i < SelectedPath.Nodes.Count; i++)
        {
            // Define node for editing
            Path.Node node = SelectedPath.Nodes[i];

            #region Node Adjustment Handle
            // Adjust node
            Handles.color = nodeColor;

            Vector3 nodePositionScale = Vector3.Scale(node.Position, scale) + position;
            Vector3 nodePosition = nodePositionScale
                .RotateAround(position, rotation);

            // Directional movement
            Quaternion directionalMovementRotation = Quaternion.identity;
            if (Tools.pivotRotation == PivotRotation.Local)
                directionalMovementRotation = rotation;

            // Modify node position in world space
            if (!showingDeleteInput)
                if (freeMoveNodeInput)
                    nodePosition = Handles.FreeMoveHandle(nodePositionScale.RotateAround(position, rotation), Quaternion.identity, nodeSize, Vector3.zero, Handles.DotCap);
                else
                    nodePosition = Handles.PositionHandle(nodePositionScale.RotateAround(position, rotation), directionalMovementRotation);

            // Convert modified node back to local space
            Vector3 nodePositionStored = ((nodePosition).RotateAround(position, Quaternion.Inverse(rotation)) - position).Divide(scale);

            if (nodePositionStored != node.Position)
            {
                lastModifiedNode = node;

                // Apply node adjustments
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(SelectedPath, "Adjust Path Node");
                if (SelectedPath.Flat)
                    nodePositionStored.z = 0;

                if (snapInput)
                {

                    if (i > 0)
                    {
                        Path.Node previousNode = SelectedPath.Nodes[i - 1];

                        float axisDistanceX = Mathf.Abs(nodePositionStored.x - previousNode.Position.x);
                        float axisDistanceY = Mathf.Abs(nodePositionStored.y - previousNode.Position.y);
                        float axisDistanceZ = Mathf.Abs(nodePositionStored.z - previousNode.Position.z);
                        if (axisDistanceX < snapDistance)
                            nodePositionStored.x = previousNode.Position.x;
                        if (axisDistanceY < snapDistance)
                            nodePositionStored.y = previousNode.Position.y;
                        if (axisDistanceZ < snapDistance)
                            nodePositionStored.z = previousNode.Position.z;
                    }
                    if (i < SelectedPath.Nodes.Count - 1)
                    {
                        Path.Node nextNode = SelectedPath.Nodes[i + 1];
                        float axisDistanceX = Mathf.Abs(nodePositionStored.x - nextNode.Position.x);
                        float axisDistanceY = Mathf.Abs(nodePositionStored.y - nextNode.Position.y);
                        float axisDistanceZ = Mathf.Abs(nodePositionStored.z - nextNode.Position.z);
                        if (axisDistanceX < snapDistance)
                            nodePositionStored.x = nextNode.Position.x;
                        if (axisDistanceY < snapDistance)
                            nodePositionStored.y = nextNode.Position.y;
                        if (axisDistanceZ < snapDistance)
                            nodePositionStored.z = nextNode.Position.z;
                    }
                }

                // Current fix for math loss
                if(currentEvent.button == 0)
                    node.Position = nodePositionStored;
                EditorGUI.EndChangeCheck();
            }
            #endregion

            if (!showingDeleteInput)
            {
                #region Adjust Tangents

                Handles.color = handleColor;
                Vector3 newTangentA = node.TangentA.RotateAround(Vector3.zero, rotation);
                Vector3 newTangentB = node.TangentB.RotateAround(Vector3.zero, rotation);

                Vector3 directionAToB = ((newTangentA + nodePosition) - (-newTangentB + nodePosition)).normalized;
                Vector3 directionToCenterA = ((newTangentA + nodePosition) - nodePosition).normalized;
                Vector3 directionToCenterB = ((-newTangentB + nodePosition) - nodePosition).normalized;

                // Adjust curve tangent based on type,  show handles for adjustment
                switch (node.TangentType)
                {
                    // Handles mirror eachother
                    case Path.Node.Type.Auto:
                        newTangentA = freeMoveNodeInput ?
                            Handles.FreeMoveHandle((directionAToB * newTangentB.magnitude) + nodePosition, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleCap) - nodePosition :
                             Handles.PositionHandle((directionAToB * newTangentB.magnitude) + nodePosition, directionalMovementRotation) - nodePosition;

                        newTangentB = freeMoveNodeInput ?
                            -(Handles.FreeMoveHandle((-directionAToB * newTangentA.magnitude) + nodePosition, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleCap) - nodePosition) :
                            -(Handles.PositionHandle((-directionAToB * newTangentA.magnitude) + nodePosition, directionalMovementRotation) - nodePosition);
                        break;
                    // Handles tilt is aligned, but distance is isolated
                    case Path.Node.Type.Aligned:
                        newTangentA = freeMoveNodeInput ?
                            Handles.FreeMoveHandle((directionAToB * newTangentA.magnitude) + nodePosition, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleCap) - nodePosition :
                            Handles.PositionHandle((directionAToB * newTangentA.magnitude) + nodePosition, directionalMovementRotation) - nodePosition;

                        newTangentB = freeMoveNodeInput ?
                            -(Handles.FreeMoveHandle((-directionAToB * newTangentB.magnitude) + nodePosition, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleCap) - nodePosition) :
                            -(Handles.PositionHandle((-directionAToB * newTangentB.magnitude) + nodePosition, directionalMovementRotation) - nodePosition);
                        break;
                    // Each handle moves independently
                    case Path.Node.Type.Free:
                        newTangentA = freeMoveNodeInput ?
                            Handles.FreeMoveHandle((directionToCenterA * newTangentA.magnitude) + nodePosition, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleCap) - nodePosition :
                            Handles.PositionHandle((directionToCenterA * newTangentA.magnitude), directionalMovementRotation) - nodePosition;
                        newTangentB = freeMoveNodeInput ?
                            -(Handles.FreeMoveHandle((directionToCenterB * newTangentB.magnitude) + nodePosition, Quaternion.identity, handleSize, Vector3.zero, Handles.CircleCap) - nodePosition) :
                            -(Handles.PositionHandle((directionToCenterB * newTangentB.magnitude), directionalMovementRotation) - nodePosition);
                        break;
                    // The node represents a line rather than a curve
                    case Path.Node.Type.Vector:
                        newTangentA = Vector3.zero;
                        newTangentB = Vector3.zero;
                        break;
                }

                // Snap tangent if in range of snap distance
                if (snapInput && node.TangentType != Path.Node.Type.Aligned)
                {
                    // Snap before tangent adjustment application
                    if (Mathf.Abs(newTangentB.x) < snapDistance)
                        newTangentB.x = 0;
                    if (Mathf.Abs(newTangentB.y) < snapDistance)
                        newTangentB.y = 0;
                    if (Mathf.Abs(newTangentB.z) < snapDistance)
                        newTangentB.z = 0;

                    if (Mathf.Abs(newTangentA.x) < snapDistance)
                        newTangentA.x = 0;
                    if (Mathf.Abs(newTangentA.y) < snapDistance)
                        newTangentA.y = 0;
                    if (Mathf.Abs(newTangentA.z) < snapDistance)
                        newTangentA.z = 0;
                }

                // Show Tangent Handle Visual Additions
                // Get direction to handle minus the handles radius so that its flush with the edge of the circle
                Vector3 handleADistanceToEdge = handleSize * ((newTangentA + nodePosition) - (-newTangentA + nodePosition)).normalized;
                Vector3 handleBDistanceToEdge = handleSize * ((newTangentB + nodePosition) - (-newTangentB + nodePosition)).normalized;
                Handles.color = handleLineColor;
                Handles.DrawLine((newTangentA + nodePosition) - handleADistanceToEdge, nodePosition);
                Handles.DrawLine((-newTangentB + nodePosition) + handleBDistanceToEdge, nodePosition);

                newTangentA = newTangentA.RotateAround(Vector3.zero, Quaternion.Inverse(rotation));
                newTangentB = newTangentB.RotateAround(Vector3.zero, Quaternion.Inverse(rotation));

                // Mark this node as the last modified
                if (newTangentA != node.TangentA || newTangentB != node.TangentB)
                {
                    lastModifiedNode = node;

                    if (SelectedPath.Flat)
                    {
                        if (newTangentA.z != node.TangentA.z)
                            newTangentA.z = 0;
                        if (newTangentB.z != node.TangentB.z)
                            newTangentB.z = 0;
                    }
                    // Apply tangent adjustment
                    // Current fix for math loss
                    if (currentEvent.button == 0)
                    {
                        // Apply node adjustments
                        EditorGUI.BeginChangeCheck();
                        Undo.RecordObject(SelectedPath, "Adjust Path Tangent");
                        node.TangentA = newTangentA;
                        node.TangentB = newTangentB;
                        EditorGUI.EndChangeCheck();
                    }
                }

                #endregion

                #region Add new node
                Handles.color = addNodeColor;
                if (i < SelectedPath.Nodes.Count - 1)
                    if (Handles.Button((Path.CurvePoint(SelectedPath.Nodes[i], SelectedPath.Nodes[i + 1], 0.5f, scale) + position).RotateAround(position, rotation), Quaternion.identity, addNodeSize, addNodeSize, Handles.DotCap))
                    {
                        EditorGUI.BeginChangeCheck();
                        Undo.RecordObject(SelectedPath, "Add Path Node");
                        SelectedPath.Add(i, 0.5f);
                        lastModifiedNode = SelectedPath.Nodes[i + 1];
                        EditorGUI.EndChangeCheck();
                    }
                #endregion
            }

            #region Remove node
            if (showingDeleteInput)
            {
                Handles.color = deleteColor;
                if (Handles.Button(nodePosition, Quaternion.identity, deleteSize, deleteSize, Handles.DotCap))
                {
                    EditorGUI.BeginChangeCheck();
                    Undo.RecordObject(SelectedPath, "Remove Path Node");
                    // Remove node
                    SelectedPath.Remove(node);
                    EditorGUI.EndChangeCheck();
                }
            }
            #endregion
        }

        if (!showingDeleteInput)
        {
            // Show add button for closing
            int lastNode = SelectedPath.Nodes.Count - 1;
            Handles.color = addNodeColor;
            if (SelectedPath.Closed)
                if (Handles.Button((Path.CurvePoint(SelectedPath.Nodes[SelectedPath.Nodes.Count - 1], SelectedPath.Nodes[0], 0.5f, scale) + position).RotateAround(position, rotation), Quaternion.identity, addNodeSize, addNodeSize, Handles.DotCap))
                    SelectedPath.Add(lastNode, 0.5f);
        }
    }

    private void Revert(RevertType revertType)
    {
        EditorGUI.BeginChangeCheck();
        switch (revertType)
        {
            case RevertType.All:
                Undo.ClearUndo(SelectedPath);
                // Clear the current list
                SelectedPath.Nodes.Clear();
                // Add each new node
                foreach (Path.Node node in SelectedPath.NodesBackup)
                    SelectedPath.Nodes.Add(new Path.Node(node.Position, node.TangentA, node.TangentB));

                SelectedPath.transform.position = SelectedPath.PositionBackup;
                SelectedPath.transform.rotation = SelectedPath.RotationBackup;
                SelectedPath.transform.localScale = SelectedPath.ScaleBackup;
                break;
            case RevertType.Path:
                SelectedPath.Nodes.Clear();
                foreach (Path.Node node in SelectedPath.NodesBackup)
                    SelectedPath.Nodes.Add(new Path.Node(node.Position, node.TangentA, node.TangentB));
                break;
            case RevertType.Position:
                SelectedPath.transform.position = SelectedPath.PositionBackup;
                break;
            case RevertType.Rotation:
                SelectedPath.transform.rotation = SelectedPath.RotationBackup;
                break;
            case RevertType.Scale:
                SelectedPath.transform.localScale = SelectedPath.ScaleBackup;
                break;
        }
        EditorGUI.EndChangeCheck();
    }
    public static void Backup()
    {
        if (SelectedPath)
        {
            SelectedPath.NodesBackup.Clear();
            // Backup nodes list
            foreach (Path.Node node in SelectedPath.Nodes)
                SelectedPath.NodesBackup.Add(new Path.Node((Vector3)node.Position, (Vector3)node.TangentA, (Vector3)node.TangentB));
            // Backup transform
            SelectedPath.PositionBackup = (Vector3)SelectedPath.transform.position;
            SelectedPath.RotationBackup = (Quaternion)SelectedPath.transform.rotation;
            SelectedPath.ScaleBackup = (Vector3)SelectedPath.transform.localScale;
        }
    }

    private void Align(Axis axis)
    {
        EditorGUI.BeginChangeCheck();

        // Define centroid 
        List<Vector3> nodes = new List<Vector3>();
        foreach (Path.Node node in SelectedPath.Nodes)
            nodes.Add(node.Position);
        Vector3 centroid = nodes.Centroid();

        switch (axis)
        {
            case Axis.X:
                Undo.RecordObject(SelectedPath, "Path Align All X");
                foreach (Path.Node node in SelectedPath.Nodes)
                {
                    // Align node with centroid x
                    node.Position.x = centroid.x;
                    // Reset the X axis of both tangents
                    node.TangentA.x = 0;
                    node.TangentB.x = 0;
                }
                break;
            case Axis.Y:
                Undo.RecordObject(SelectedPath, "Path Align All Y");
                foreach (Path.Node node in SelectedPath.Nodes)
                {
                    // Align node with centroid y
                    node.Position.y = centroid.y;
                    // Reset the Y axis of both tangents
                    node.TangentA.y = 0;
                    node.TangentB.y = 0;
                }
                break;
            case Axis.Z:
                Undo.RecordObject(SelectedPath, "Path Align All Z");
                foreach (Path.Node node in SelectedPath.Nodes)
                {
                    // Align node with centroid z
                    node.Position.z = centroid.z;
                    // Reset the Z axis of both tangents
                    node.TangentA.z = 0;
                    node.TangentB.z = 0;
                }
                break;
        }
        EditorGUI.EndChangeCheck();
    }
    #endregion
}

public class PathEditorIOHooks : AssetModificationProcessor
{
    // Backup path on save
    public static string[] OnWillSaveAssets(string[] paths)
    {
       // PathEditor.Backup();
        return paths;
    }
}