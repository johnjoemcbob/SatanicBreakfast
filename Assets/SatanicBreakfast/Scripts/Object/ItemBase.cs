using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : MonoBehaviour
{
    public bool Held = false;

    public virtual void OnPickup()
	{

	}

    public virtual void OnDrop()
    {

    }
}
