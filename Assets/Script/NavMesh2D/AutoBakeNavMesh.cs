using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class AutoBakeNavMesh : MonoBehaviour
{
    void Start()
    {
        NavMeshSurface surface = GetComponent<NavMeshSurface>();

        if (surface != null)
        {
            // Vygeneruje NavMesh okamžitì pøi startu scény
            surface.BuildNavMesh();
        }
    }
}
