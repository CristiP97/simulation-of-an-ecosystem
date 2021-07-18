using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap
{
    private int width;
    private int height;
    private int cellSize;
    private Vector3 mapOffset;
    private int[,] map;

    public GridMap(int _width, int _height, int _cellSize, Vector3 _mapOffset)
    {
        width = _width;
        height = _height;
        cellSize = _cellSize;
        mapOffset = _mapOffset;

        map = new int[width, height];
        InitialiseMap();
    }

    private void InitialiseMap()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                map[x, y] = 0;
            }
        }
    }

    

    public Vector3 GetMapOffset()
    {
        return mapOffset;
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y>=0 && x < width && y < height)
        {
            map[x, y] = value;
        }
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return map[x, y];
        }

        return -1;
    }
}
