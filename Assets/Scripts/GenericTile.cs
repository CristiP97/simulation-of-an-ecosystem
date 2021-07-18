using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTile : MonoBehaviour
{
    protected MeshRenderer objectRender;
    public int distance = -1;

    public virtual void Start()
    {
        objectRender = gameObject.transform.GetComponent<MeshRenderer>();
    }
}
