using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    private Vector3[] vertex;

    public Voxel(Vector3[] vertex)
    {
        this.vertex = vertex;
    }

    public Vector3[] getVertex()
    {
        return this.vertex;
    }
}
