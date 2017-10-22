using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
sealed internal class Geometry : MonoBehaviour
{
    #region Values
    [SerializeField] private Color _color;
    [SerializeField] private Material _material;
    [SerializeField] private int _orderInLayer;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private PolygonCollider2D _polygonCollider2D;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
        BuildMesh();
    }

    private void OnDrawGizmos()
    {
        PolygonCollider2D polygonCollider2D = this.gameObject.GetComponent<PolygonCollider2D>();
        int[] indices = TriangulationHelper.Triangulate2D(polygonCollider2D.points);

        Vector3[] vertices = new Vector3[polygonCollider2D.points.Length];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = polygonCollider2D.points[i] + polygonCollider2D.offset;

        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();

        MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();

        if (meshFilter.sharedMesh == null || meshFilter.sharedMesh.vertices.Length != vertices.Length)
            meshFilter.sharedMesh = new Mesh();

        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.triangles = indices;
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();
        meshRenderer.sortingOrder = _orderInLayer;

        meshFilter.sharedMesh.name = "geometry";
        meshFilter.sharedMesh.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDestroy()
    {
        if (_meshFilter.sharedMesh != null)
            DestroyImmediate(_meshFilter.sharedMesh);
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _meshFilter = this.gameObject.GetComponent<MeshFilter>();
        _meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        _polygonCollider2D = this.gameObject.GetComponent<PolygonCollider2D>();

        _material.SetColor("_Color", _color);
        _meshRenderer.sharedMaterial = _material;
    }

    private void BuildMesh()
    {
        int[] indices = TriangulationHelper.Triangulate2D(_polygonCollider2D.points);

        Vector3[] vertices = new Vector3[_polygonCollider2D.points.Length];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = _polygonCollider2D.points[i] + _polygonCollider2D.offset;

        if(_meshFilter.sharedMesh == null)
            _meshFilter.sharedMesh = new Mesh();

        if (_meshFilter.sharedMesh.vertices != vertices)
        {
            _meshFilter.sharedMesh.vertices = vertices;
            _meshFilter.sharedMesh.triangles = indices;
            _meshFilter.sharedMesh.RecalculateNormals();
            _meshFilter.sharedMesh.RecalculateBounds();
            _meshRenderer.sortingOrder = _orderInLayer;

            _meshFilter.sharedMesh.name = "geometry";
            _meshFilter.sharedMesh.hideFlags = HideFlags.HideAndDontSave;
        }
    } 
    #endregion
}

