using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Device : MonoBehaviour, ISetupable
{
    public abstract void Setup();
    public abstract void Pause();
    public abstract void Resume();
}