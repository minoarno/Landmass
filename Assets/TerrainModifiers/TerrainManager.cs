using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainManager : MonoBehaviour
{
    [System.Serializable]
    public enum LayerInteraction
    {
        ADDITIVE = 0,
        LERPED = 1
    }

    [Header("Terrain Specifications")]
    private Terrain _terrain;
    private int _depth = 20;
    [SerializeField] private int _width = 256;
    [SerializeField] private int _height = 256;

    private List<TerrainModifierLayer> _terrains = new List<TerrainModifierLayer>();

    [SerializeField] private LayerInteraction _layerInteraction;

    private bool _dataIsUnchanged = true;

    // Start is called before the first frame update
    void Start()
    {
        TerrainUpdate();
    }

    private void Update()
    {
        if (_dataIsUnchanged) return;
        TerrainUpdate();
        _dataIsUnchanged = true;
    }

    void TerrainUpdate()
    {
        if (_terrain == null)
        {
            _terrain = GetComponent<Terrain>();
        }
        _terrains.Clear();
        TerrainModifierLayer[] terrains = GetComponentsInChildren<TerrainModifierLayer>();
        for (int i = 0; i < terrains.Length; i++)
        {
            if (terrains[i].enabled)
            {
                _terrains.Add(terrains[i]);
                _terrains[i].Width = _width;
                _terrains[i].Height = _height;
            }
        }

        switch (_layerInteraction)
        {
            case LayerInteraction.LERPED:
                _terrain.terrainData = GenerateTerrainOctaves(_terrain.terrainData);
                break;
            case LayerInteraction.ADDITIVE:
            default:
                _terrain.terrainData = GenerateTerrainAdditive(_terrain.terrainData);
                break;
        }
    }

    TerrainData GenerateTerrainAdditive(TerrainData terrainData)
    {
        terrainData.heightmapResolution = _width + 1;

        _depth = 0;
        float[,] heights = new float[_width, _height];
        for (int i = 0; i < _terrains.Count; i++)
        {
            float[,] added = _terrains[i].GenerateHeights();
            _depth += _terrains[i].Depth;
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    heights[x, z] += added[x, z];
                }
            }
        }

        terrainData.size = new Vector3(_width, _depth, _height);

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    TerrainData GenerateTerrainOctaves(TerrainData terrainData)
    {
        float min = float.MaxValue;
        float max = float.MinValue;


        terrainData.heightmapResolution = _width + 1;

        _depth = 0;
        float[,] heights = new float[_width, _height];
        for (int i = 0; i < _terrains.Count; i++)
        {
            float[,] added = _terrains[i].GenerateHeights();
            _depth += _terrains[i].Depth;
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _height; z++)
                {
                    heights[x, z] += added[x, z];
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                if (heights[x, z] < min)
                {
                    min = heights[x, z];
                }
                else if (heights[x, z] > max)
                {
                    max = heights[x, z];
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                heights[x, z] = Mathf.InverseLerp(min, max, heights[x, z]);
            }
        }

        terrainData.size = new Vector3(_width, _depth, _height);

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    private void OnValidate()
    {
        if (_width < 10)
        {
            _width = 10;
        }

        if (_height < 10)
        {
            _height = 10;
        }
        _dataIsUnchanged = false;
    }

    public void DataIsChanged()
    {
        _dataIsUnchanged = false;
    }
}