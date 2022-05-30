using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	public static Player Instance;
	public static Camera Camera;

	private void Awake()
	{
		Instance = this;
		Camera = GetComponentInChildren<Camera>();
	}
}
