using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.TerrainTools;

//How to overwrite the terraindata correctly
//https://www.youtube.com/watch?v=vFvwyu_ZKfU

[ExecuteInEditMode]
public class TerrainModifierLayer : MonoBehaviour
{
    public int Depth { get { return _depth; } set { _depth = value; } }
    protected int _width = 256;
    public int Width { get { return _width; } set { _width = value; } }
    protected int _height = 256;
    public int Height { get { return _height; } set { _height = value; } }

    [Header("Terrain Specifications")]
    [SerializeField] private int _depth = 20;

    [SerializeField] private float _terrainDetailScale = 50f;

    //[SerializeField] private bool _invert = false;
    [SerializeField] private int _octaves = 2;
    [Range(0,1)]
    [SerializeField] private float _persistance = 1f;
    [SerializeField] private float _lacunarity = 1f;

    [Header("Random Generator")]
    [SerializeField] private bool _fixedSeed = true;
    [SerializeField] private int _seed = 0;
    [SerializeField] private float _offsetX = 0f;
    [SerializeField] private float _offsetZ = 0f;

    // Start is called before the first frame update
    void Start()
    {

        if (_fixedSeed)
        {
            Random.InitState(_seed);
        }
    }

    virtual public float[,] GenerateHeights()
    {
        float[,] heights = new float[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int o = 0; o < _octaves; o++)
                {
                    float xCoord = ((float)x / _width) * frequency * _terrainDetailScale + _offsetX;
                    float zCoord = ((float)z / _height) * frequency * _terrainDetailScale + _offsetZ;

                    float perlinValue = Mathf.PerlinNoise(xCoord, zCoord) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= _persistance;
                    frequency *= _lacunarity;
                }

                heights[x, z] = noiseHeight;
            }
        }
        return heights;
    }

    //This is called when the values in the inspector are changed
    protected void OnValidate()
    {
        if (_lacunarity < 1)
        {
            _lacunarity = 1;
        }

        if (_octaves < 0)
        {
            _octaves = 0;
        }

        if (_terrainDetailScale < 1)
        {
            _terrainDetailScale = 1;
        }

        if (_fixedSeed)
        {
            Random.InitState(_seed);
        }

        DataIsChanged();
    }

    protected void OnEnable()
    {
        DataIsChanged();
    }

    protected void OnDisable()
    {
        DataIsChanged();
    }

    public void DataIsChanged()
    {
        TerrainManager terrainManager = gameObject.GetComponentInParent<TerrainManager>();
        if (terrainManager != null)
        {
            terrainManager.DataIsChanged();
        }
    }
}
