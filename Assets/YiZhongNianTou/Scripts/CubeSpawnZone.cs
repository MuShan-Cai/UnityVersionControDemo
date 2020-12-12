﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnZone : SpawnZone
{

    [SerializeField]
    private bool surfaceOnly;
    public override Vector3 SpawnPoint {
        get {
            Vector3 p;
            p.x = Random.Range(-0.5f,0.5f);
            p.y = Random.Range(-0.5f,0.5f);
            p.z = Random.Range(-0.5f,0.5f);
            if(surfaceOnly)
            {
                int axis = Random.Range(0, 3);
                p[axis] = p[axis] < 0f ? -0.5f : 0.5f;
            }
            return transform.TransformPoint(p);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero,Vector3.one);
    }
}
