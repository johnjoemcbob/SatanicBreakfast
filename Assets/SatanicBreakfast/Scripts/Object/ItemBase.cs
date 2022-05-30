using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : MonoBehaviour
{
    public float Radius = 0.5f;
    public float GrabUpOffset = 0;

    public virtual void OnPickup()
	{

	}

    public virtual void OnDrop()
    {

    }
}
