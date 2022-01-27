using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SplineTerrainEditor : MonoBehaviour
{
    private Terrain _terrain;
    [SerializeField] private int _width = 256;
    [SerializeField] private int _height = 256;
    [SerializeField] private int _depth = 50;

    [SerializeField] private List<Spline> _splines = new List<Spline>();

    [Range(0f,1f)]
    [SerializeField] private float _StepSize = .03f;

    [System.Serializable]
    public enum MapShow
    {
        heightMap = 0,
        colorMap = 1,
        voronoi = 2
    }

    [SerializeField]
    private MapShow _mapShow = MapShow.colorMap;

    Color _fillerColor = Color.clear;
    float _fillerValue = float.MaxValue;

    Color[] _regionColors;
    Vector2[] _splinePoints;
    int _amountOfSplinePoints = 1;

    bool _dataIsUnchanged = true;

    // Start is called before the first frame update
    void Start()
    {
        _terrain = GetComponent<Terrain>();
        _terrain.terrainData.heightmapResolution = _width + 1;

        _splinePoints = GetSplineCoordinates();
        _amountOfSplinePoints = _splinePoints.Length;

        FillInColors();
        UpdateSplineEditor();
    }

    private void Update()
    {
        if (_dataIsUnchanged) return;

        UpdateSplineEditor();
        _dataIsUnchanged = true;
    }

    public void UpdateSplineEditor()
    {
        switch(_mapShow)
        {
            case MapShow.colorMap:
                _terrain.terrainData = DrawDistanceField(_terrain.terrainData);
                break;
            case MapShow.heightMap:
                _terrain.terrainData = CalculateDistanceField(_terrain.terrainData);
                break;
            case MapShow.voronoi:
                _terrain.terrainData = CalculateVoronoi(_terrain.terrainData);
                break;
        }
    }

    public void FillInColors()
    {
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

    private TerrainData DrawDistanceField(TerrainData terrainData)
    {
        _splinePoints = GetSplineCoordinates();

        float[,] heights = new float[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                heights[x, z] = (x + z);
            }
        }

        Texture2D mainTexture = new Texture2D(_width, _height);

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                mainTexture.SetPixel(x, z, _fillerColor);
            }
        }
        mainTexture.Apply();

        //Draw Spline Lines
        for (int i = 0; i < (_splinePoints.Length - 1); i++)
        {
            DrawLine(ref mainTexture, _splinePoints[i], _splinePoints[i + 1], Color.black);
        }
        mainTexture.Apply();

        while (ThereAreStillSomeEmpty(ref mainTexture))
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

    private TerrainData CalculateDistanceField(TerrainData terrainData)
    {
        _splinePoints = GetSplineCoordinates();

        float[,] heights = new float[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                heights[x, z] = _fillerValue;
            }
        }

        float maxHeightTerrain = gameObject.transform.position.y;

        //Draw Spline Lines
        for (int i = 0; i < (_splinePoints.Length - 1); i++)
        {
            DrawLine(ref heights, _splinePoints[i], _splinePoints[i + 1], 0f);
        }

        while (ThereAreStillSomeEmpty(ref heights))
        {
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    float pixelHeight = heights[x, z];
                    if (pixelHeight != _fillerValue)
                    {
                        float newHeight = pixelHeight + _StepSize;

                        if (x - 1 >= 0 && (heights[x - 1, z] == _fillerValue || heights[x - 1, z] > newHeight))
                        {
                            heights[x - 1, z] = newHeight;
                        }
                        if (x + 1 < _width && (heights[x + 1, z] == _fillerValue || heights[x + 1, z] > newHeight))
                        {
                            heights[x + 1, z] = newHeight;
                        }
                        if (z - 1 >= 0 && (heights[x, z - 1] == _fillerValue || heights[x, z - 1] > newHeight))
                        {
                            heights[x, z - 1] = newHeight;
                        }
                        if (z + 1 < _height && (heights[x, z + 1] == _fillerValue || heights[x, z + 1] > newHeight))
                        {
                            heights[x, z + 1] = newHeight;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                heights[x, z] = (1f - heights[x, z]) * _depth;
            }
        }

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
    public bool ThereAreStillSomeEmpty(ref float[,] grid)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                if (grid[x, z] == _fillerValue)
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

    public void DrawLine(ref float[,] grid, Vector2 p1, Vector2 p2, float value)
    {
        Vector2 t = p1;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
        {
            t = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            grid[(int)t.y,(int)t.x] = value;
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
        _splinePoints = GetSplineCoordinates();
        _amountOfSplinePoints = _splinePoints.Length;
    }
}
