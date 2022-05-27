using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoAnimRootMotion : MonoBehaviour
{
    void Update()
    {
        transform.localPosition = new Vector3( 0, 0, -transform.GetChild( 2 ).localPosition.z );
    }
}
