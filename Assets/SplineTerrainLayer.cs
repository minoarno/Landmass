using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTerrainLayer : TerrainModifierLayer
{
    float _fillerValue = float.MaxValue;
    Vector2[] _splinePoints;
    int _amountOfSplinePoints = 1;
    [Range(0f, 0.1f)]
    [SerializeField] private float _StepSize = .003f;

    [SerializeField] private List<Spline> _splines = new List<Spline>();

    [System.Serializable]
    public enum TerrainType
    {
        Ravine = 0,
        Mountain = 1
    }

    [SerializeField] private TerrainType _terrainType;

    public override float[,] GenerateHeights()
    {
        _splinePoints = GetSplineCoordinates();
        _amountOfSplinePoints = _splinePoints.Length;

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


        switch (_terrainType)
        {
            case TerrainType.Ravine:
            default:
                break;
            case TerrainType.Mountain:
                for (int x = 0; x < _width; x++)
                {
                    for (int z = 0; z < _height; z++)
                    {
                        heights[x, z] = (1f - heights[x, z]);
                    }
                }
                break;
        }

        return heights;
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

    public void DrawLine(ref float[,] grid, Vector2 p1, Vector2 p2, float value)
    {
        Vector2 t = p1;
        float frac = 1 / Mathf.Sqrt(Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2));
        float ctr = 0;

        while ((int)t.x != (int)p2.x || (int)t.y != (int)p2.y)
        {
            t = Vector2.Lerp(p1, p2, ctr);
            ctr += frac;
            grid[(int)t.y, (int)t.x] = value;
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
            vector2s.Add(new Vector2(vector3s[i].x - pos.x, vector3s[i].z - pos.y));
        }

        return vector2s.ToArray();
    }
}
