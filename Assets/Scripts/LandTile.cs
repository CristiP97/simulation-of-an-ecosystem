using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandTile : GenericTile
{
    public Gradient landGradient;
    
    //private int maxDistance;
    private int minDistance;
    private static int maxDistance;

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
            objectRender.material.color = landGradient.Evaluate(value);
        } else
        {
            Debug.LogWarning("Didn't find the mesh renderer of the land tile!");
        }
        
    }
}
