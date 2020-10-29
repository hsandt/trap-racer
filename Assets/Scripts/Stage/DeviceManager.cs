//#define DEBUG_DEVICE_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class DeviceManager : SingletonManager<DeviceManager>
{
    /* State vars */

    /// List of registered devices to setup
    private readonly List<Device> m_Devices = new List<Device>();

    public void RegisterDevice(Device device)
    {
        m_Devices.Add(device);
#if DEBUG_DEVICE_MANAGER
        Debug.LogFormat(device, "[DeviceManager] Registered device {0}", device);
#endif
    }

    public void UnregisterDevice(Device device)
    {
        m_Devices.Remove(device);
    }

    public void SetupDevices()
    {
        foreach (var device in m_Devices)
        {
            device.Setup();
        }
    }
}
