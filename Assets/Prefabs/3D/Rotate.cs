using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
	Rigidbody rb;
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.angularVelocity = new Vector3(0, 2f, 0);
	}

}