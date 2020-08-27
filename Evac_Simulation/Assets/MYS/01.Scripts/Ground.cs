using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ground : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            navMeshSurface.BuildNavMesh();
        }
    }
}
