using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    private Mesh _mesh;

    private Vector3[] _vertices;
    private int[] _triangles;

    [SerializeField] private int _xSize = 20;
    [SerializeField] private int _zSize = 20;

    [SerializeField] private float _elevationScale = 2f;

    [Header("Noise Parameters")]
    [SerializeField] private float _noiseScale = .3f;
    [SerializeField] private Vector2 _noiseOffset = Vector2.zero;
    public Vector2 NoiseOffset { get; }

    // Start is called before the first frame update
    private void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateShape();
        UpdateMesh();
    }

    private void CreateShape()
    {
        _vertices = new Vector3[(_xSize + 1) * (_zSize + 1)];

        for (int i = 0, z = 0; z <= _zSize; z++)
        {
            for (int x = 0; x <= _xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * _noiseScale + _noiseOffset.x, z * _noiseScale + _noiseOffset.y) * _elevationScale;
                _vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }


        _triangles = new int[_xSize * _zSize * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < _zSize; z++)
        {
            for (int x = 0; x < _xSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + _xSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + _xSize + 1;
                _triangles[tris + 5] = vert + _xSize + 2;

                vert++;
                tris += 6;

            }
            vert++;
        }
    }

    private void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        _mesh.RecalculateTangents();
    }

    private void OnDrawGizmos()
    {
        //if (_vertices == null) return;
        //
        //
        //for (int i = 0; i < _vertices.Length; i++)
        //{
        //    Gizmos.DrawSphere(_vertices[i], .1f);
        //}
    }
}
