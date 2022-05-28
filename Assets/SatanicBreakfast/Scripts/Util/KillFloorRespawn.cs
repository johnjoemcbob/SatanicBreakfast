using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFloorRespawn : MonoBehaviour
{
    public float KillY = -10;

    private Vector3 RespawnPos;

    void Start()
    {
        RespawnPos = transform.position;
    }

    void Update()
    {
        if ( transform.position.y <= KillY )
		{
            transform.position = RespawnPos;
		}
    }
}
