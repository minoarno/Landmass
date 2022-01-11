using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.TerrainTools;

//How to overwrite the terraindata correctly
//https://www.youtube.com/watch?v=vFvwyu_ZKfU
[ExecuteInEditMode]
public class TerrainModifier : MonoBehaviour
{
    [Header("Terrain Specifications")]
    [SerializeField] int _depth = 20;
    [SerializeField] int _width = 256;
    [SerializeField] int _height = 256;
    [SerializeField] float _terrainDetailScale = 50f;

    [Header("Random Generator")]
    [SerializeField] bool _fixedSeed = true;
    [SerializeField] int _seed = 0;
    [SerializeField] float _offsetX = 0f;
    [SerializeField] float _offsetZ = 0f;

    // Start is called before the first frame update
    void Start()
    {

        if (_fixedSeed)
        {
            Random.InitState(_seed);
        }

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = _width + 1;
        terrainData.size = new Vector3(_width, _depth, _height);

        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                heights[x, z] = CalculateHeight(x, z);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int z)
    {
        float xCoord = (float)x / _width * _terrainDetailScale + _offsetX;
        float zCoord = (float)z / _height * _terrainDetailScale + _offsetZ;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }

    // Update is called once per frame
    void Update()
    {

        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

    }
}
