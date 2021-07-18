using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTile : GenericTile
{
    public Gradient waterGradient;

    //private int maxDistance;
    private int minDistance;
    private static int maxDistance;
    private Vector2Int mapPosition;
    private bool setMapPosition;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        minDistance = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void SetMaxDistance(int _maxDistance)
    {
        maxDistance = _maxDistance;
    }

    public void SetColor()
    {
        if (objectRender != null)
        {
            float value = Mathf.InverseLerp(minDistance, maxDistance, distance);
            objectRender.material.color = waterGradient.Evaluate(value);
        }
    }

    public void SetMapPosition(Vector2Int _mapPosition)
    {
        if (!setMapPosition)
        {
            mapPosition = _mapPosition;
            setMapPosition = true;
        }
    }

    public Vector2Int GetMapPosition()
    {
        return mapPosition;
    }
}
