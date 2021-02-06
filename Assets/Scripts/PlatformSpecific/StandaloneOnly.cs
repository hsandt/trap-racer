using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandaloneOnly : MonoBehaviour
{
#if UNITY_WEBGL
    void Start()
    {
        gameObject.SetActive(false);
    }
#endif
}
