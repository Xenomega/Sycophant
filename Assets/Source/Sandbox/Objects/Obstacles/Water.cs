using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Path))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]

sealed internal class Water :  Orientable
{
    #region Values
    private bool _checkedStay;
    [SerializeField]
    private float _waveFrequency;
    [SerializeField] private float _waveHeightMax;
    [SerializeField] private float _settleDuration;
    [SerializeField] private AnimationCurve _settleScaleOverLifetime;

    private float _settleScale;
    [SerializeField] private float _settledTime;

    [SerializeField] private Color _color;
    [SerializeField] private Material _material;
    [SerializeField] private int _orderInLayer;

    private Path _path;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private PolygonCollider2D _polygonCollider2D;

    private class WaveNode
    {
        internal int pathNode;
        internal float tangentWeight;
        internal Vector3 normal;
        internal Vector3 tangent;
    }
    private List<WaveNode> _waveNodes = new List<WaveNode>();
    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Update()
    {
        UpdateWave();
        UpdateMesh();
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        ProcessTrigger(collider2D, true);
    }
    private void OnTriggerExit2D(Collider2D collider2D)
    {
        ProcessTrigger(collider2D, false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(_meshFilter.sharedMesh != null)
            DestroyImmediate(_meshFilter.sharedMesh);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        this.gameObject.layer = Globals.WATER_LAYER;

        _path = this.gameObject.GetComponent<Path>();
        _meshFilter = this.gameObject.GetComponent<MeshFilter>();
        _meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        _polygonCollider2D = this.gameObject.GetComponent<PolygonCollider2D>();

        _material.SetColor("_Color", _color);
        _meshRenderer.sharedMaterial = _material;

        _polygonCollider2D.isTrigger = true;

        // NOTE: This is to make sure that objects that start within the trigger - trigger enter
        this._polygonCollider2D.enabled = false;
        this._polygonCollider2D.enabled = true;


        _meshFilter.sharedMesh = new Mesh();
        _meshFilter.sharedMesh.name = this.gameObject.name + "_water";
        _meshFilter.sharedMesh.hideFlags = HideFlags.HideAndDontSave;

        _meshRenderer.sortingOrder = _orderInLayer;

        Vector2[] points = GetPathPoints();
        _polygonCollider2D.SetPath(0, points);

        PopulatWaveNodes();
        UpdateMesh(true);
    }

    protected override void OnWorldAngleChanged(float worldAngle)
    {
        if (_meshRenderer != null &&_meshRenderer.isVisible)
            _settledTime = Time.time + _settleDuration;
    }

    private void PopulatWaveNodes()
    {
        for (int i = 0; i < _path.Nodes.Count; i++)
        {
            if (_path.Nodes[i].TangentType == Path.Node.Type.Auto)
            {
                WaveNode waveNode = new WaveNode();
                waveNode.pathNode = i;

                Vector3 pointBefore = Vector3.zero;
                Vector3 pointAfter = Vector3.zero;

                // Define tangent
                if (waveNode.pathNode == 0)
                {
                    pointAfter = _path.Nodes[waveNode.pathNode + 1].Position;
                    pointBefore = _path.Nodes[_path.Nodes.Count - 1].Position;
                }
                else if (waveNode.pathNode == _path.Nodes.Count - 1)
                {
                    pointAfter = _path.Nodes[0].Position;
                    pointBefore = _path.Nodes[waveNode.pathNode - 1].Position;
                }
                else
                {
                    pointAfter = _path.Nodes[waveNode.pathNode + 1].Position;
                    pointBefore = _path.Nodes[waveNode.pathNode - 1].Position;
                }

                // Describe values
                waveNode.tangent = (pointAfter - pointBefore).normalized;
                waveNode.tangentWeight = Vector3.Distance(pointBefore, pointAfter) / Mathf.PI;
                waveNode.normal = Vector3.Cross(waveNode.tangent, Vector3.forward);

                _waveNodes.Add(waveNode);
            }
        }
    }
    private Vector2[] GetPathPoints()
    {
        return _path.CurvePoints2D(false, true, false);
    }

    private void UpdateWave()
    {
        float durationRemaining = _settledTime - Time.time;
        float durationScale = durationRemaining / _settleDuration;
        _settleScale = _settleScaleOverLifetime.Evaluate(1 - durationScale);

        if (_settleScale <= 0)
            return;
        if (!_meshRenderer.isVisible)
            return;

        float frequency = Mathf.Sin(Time.timeSinceLevelLoad * _waveFrequency) * (_waveHeightMax * _settleScale);

        foreach (WaveNode waveNode in _waveNodes)
        {
            Vector2 normal = waveNode.normal;
            Vector2 tangent = waveNode.tangent;
            float tangentWeight = waveNode.tangentWeight;
            // Define new tangents
            _path.Nodes[waveNode.pathNode].TangentA = (normal * frequency) + (tangent * tangentWeight);
            _path.Nodes[waveNode.pathNode].TangentB = (normal * frequency) + (tangent * tangentWeight);
        }

    }
    private void UpdateMesh(bool initialize = false)
    {
        if (!initialize)
        {
            if (!_meshRenderer.isVisible)
                return;
            if (_settleScale <= 0)
                return;
        }

        Vector2[] pathPoints = GetPathPoints();

        _polygonCollider2D.points = pathPoints;

        int[] indices = TriangulationHelper.Triangulate2D(pathPoints);

        Vector3[] vertices = new Vector3[pathPoints.Length];
        int vertexCount = vertices.Length;
        for (int i = 0; i < vertexCount; i++)
            vertices[i] = pathPoints[i];

        Mesh mesh = _meshFilter.sharedMesh;

        // Update mesh
        if (mesh.vertexCount == 0 || mesh.vertexCount == vertexCount)
        {
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }

    private void ProcessTrigger(Collider2D collider2D, bool enter)
    {
        Submergable submergable = collider2D.gameObject.GetComponent<Submergable>();
        if (submergable != null)
            submergable.Submerge(enter);
    }
    #endregion
}
