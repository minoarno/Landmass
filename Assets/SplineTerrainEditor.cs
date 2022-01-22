using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineTerrainEditor : MonoBehaviour
{
    private Terrain _terrain;
    [SerializeField] private int _width = 256;
    [SerializeField] private int _height = 256;
    [SerializeField] private int _depth = 50;

    [SerializeField] private List<Spline> _splines = new List<Spline>();

    [Range(0f,1f)]
    [SerializeField] private float _StepSize = .03f;


    Color _fillerColor = Color.clear;
    Color[] _regionColors;
    Vector2[] _splinePoints;
    int _amountOfSplinePoints = 1;

    bool _dataIsUnchanged = true;

    // Start is called before the first frame update
    void Start()
    {
        _terrain = GetComponent<Terrain>();
        FillInColors();
        //_terrain.terrainData = CalculateVoronoi(_terrain.terrainData);
        _terrain.terrainData = CalculateDistanceField(_terrain.terrainData);
    }

    private void Update()
    {
        if (_dataIsUnchanged) return;
        UpdateSplineEditor();
        _dataIsUnchanged = true;
    }

    public void UpdateSplineEditor()
    {
        //_terrain.terrainData = CalculateVoronoi(_terrain.terrainData);
        _terrain.terrainData = CalculateDistanceField(_terrain.terrainData);
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

    private TerrainData CalculateVoronoi(TerrainData terrainData)
    {
        _splinePoints = GetSplineCoordinates();

        float[,] heights = new float[_width, _height];

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

    private TerrainData CalculateDistanceField(TerrainData terrainData)
    {
        _splinePoints = GetSplineCoordinates();

        float[,] heights = new float[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                //heights[x, z] = float.MinValue;
                heights[x, z] = 0f;
            }
        }

        Texture2D mainTexture = new Texture2D(_width, _height);

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                mainTexture.SetPixel(x, z, _fillerColor);
                mainTexture.Apply();
            }
        }
        
        //Draw Spline Lines
        for (int i = 0; i < (_splinePoints.Length - 1); i++)
        {
            DrawLine(ref mainTexture, _splinePoints[i], _splinePoints[i + 1], Color.black);
            mainTexture.Apply();
        }

        int amountOfTimes = 6;
        //for (int i = 0; i < amountOfTimes; i++)
        while(ThereAreStillSomeEmpty(ref mainTexture))
        {
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    Color pixelColor = mainTexture.GetPixel(x, z);
                    if (pixelColor.a != 0f)
                    {
                        Color newColor = new Color(pixelColor.r + _StepSize, pixelColor.r + _StepSize, pixelColor.r + _StepSize, 1f);

                        if (x - 1 >= 0 && (mainTexture.GetPixel(x - 1, z) == _fillerColor || mainTexture.GetPixel(x - 1, z).r > newColor.r))
                        {
                            mainTexture.SetPixel(x - 1, z, newColor);
                        }
                        if (x + 1 < _width && (mainTexture.GetPixel(x + 1, z) == _fillerColor || mainTexture.GetPixel(x + 1, z).r > newColor.r))
                        {
                            mainTexture.SetPixel(x + 1, z, newColor);
                        }
                        if (z - 1 >= 0 && (mainTexture.GetPixel(x, z - 1) == _fillerColor || mainTexture.GetPixel(x, z - 1).r > newColor.r))
                        {
                            mainTexture.SetPixel(x, z - 1, newColor);
                        }
                        if (z + 1 < _height && (mainTexture.GetPixel(x, z + 1) == _fillerColor || mainTexture.GetPixel(x, z + 1).r > newColor.r))
                        {
                            mainTexture.SetPixel(x, z + 1, newColor);
                        }
                    }
                }
            }
            mainTexture.Apply();
        }

        _terrain.materialTemplate.mainTexture = mainTexture;
        terrainData.size = new Vector3(_width, _depth, _height);
        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    public bool ThereAreStillSomeEmpty(ref Texture2D mainTexture)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                if (mainTexture.GetPixel(x,z) == _fillerColor)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void DrawLine(ref Texture2D tex, Vector2 p1, Vector2 p2, Color col)
    {
        Vector2 t = p1;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
        {
            t = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            tex.SetPixel((int)t.x, (int)t.y, col);
        }
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

    private void OnValidate()
    {
        _dataIsUnchanged = false;
    }
}
