using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SplineTerrainEditor : MonoBehaviour
{
    private Terrain _terrain;
    [SerializeField] private int _width = 256;
    [SerializeField] private int _height = 256;
    [SerializeField] private int _depth = 50;

    [SerializeField] private List<Spline> _splines = new List<Spline>();

    Color[] _regionColors;
    Vector2[] _splinePoints;
    int _amountOfSplinePoints = 1;

    // Start is called before the first frame update
    void Start()
    {
        _terrain = GetComponent<Terrain>();
        FillInColors();
        _terrain.terrainData = CalculateMountain(_terrain.terrainData);
    }

    // Update is called once per frame
    void Update()
    {
        _terrain.terrainData = CalculateMountain(_terrain.terrainData);
    }

    public void FillInColors()
    {
        _splinePoints = GetSplineCoordinates();
        _amountOfSplinePoints = _splinePoints.Length;

        _regionColors = new Color[_amountOfSplinePoints];
        for (int i = 0; i < _amountOfSplinePoints; i++)
        {
            _regionColors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }

    private TerrainData CalculateMountain(TerrainData terrainData)
    {
        _splinePoints = GetSplineCoordinates();

        float[,] heights = new float[_width, _height];

        Color[,] colors = new Color[_width, _height]; 

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                heights[x, z] = 0f;
            }
        }

        Texture2D mainTexture = new Texture2D(_width, _height);

        Color[] pixelColors = new Color[_width * _height];
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float distance = float.MaxValue;
                int value = 0;

                for (int i = 0; i < _amountOfSplinePoints; i++)
                {
                    float newDistance = Vector2.Distance(new Vector2(x, y), _splinePoints[i]);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        value = i;
                    }
                }

                pixelColors[x + y * _width] = _regionColors[value % _amountOfSplinePoints];
            }
        }
        mainTexture.SetPixels(pixelColors);
        mainTexture.Apply();

        _terrain.materialTemplate.mainTexture = mainTexture;
        terrainData.size = new Vector3(_width, _depth, _height);
        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    Vector2[] GetSplineCoordinates()
    {
        _splines.Clear();
        Spline[] splines = GetComponentsInChildren<Spline>();
        foreach (Spline spline in splines)
        {
            _splines.Add(spline);
        }

        List<Vector2> vector2s = new List<Vector2>();
        Vector3[] vector3s = _splines[0].GetInterpolatedPositions();

        Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        for (int i = 0; i < vector3s.Length; i++)
        {
            vector2s.Add(new Vector2(vector3s[i].x - pos.x,vector3s[i].z - pos.y));
        }

        return vector2s.ToArray();
    }
}
