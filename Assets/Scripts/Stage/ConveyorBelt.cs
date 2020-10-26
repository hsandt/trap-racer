using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField, Tooltip("Extra speed provided by the belt when running on it. If negative, slows runner down.")]
    private float extraSpeed = 5f;
    public float ExtraSpeed => extraSpeed;
}
