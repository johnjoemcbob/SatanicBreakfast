using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AINavTest : MonoBehaviour
{
    public Transform Target;

    void Start()
    {
    }

    void Update()
    {
        GetComponent<NavMeshAgent>().SetDestination( Target.position );
    }
}
