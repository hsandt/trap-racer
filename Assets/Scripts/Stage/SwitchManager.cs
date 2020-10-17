#define DEBUG_SWITCH_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class SwitchManager : SingletonManager<SwitchManager>
{
    /* State vars */

    // Array of list of registered switchs
    private List<Switch> m_Switchs = new List<Switch>();

    public void RegisterSwitch(Switch switchDevice)
    {
        m_Switchs.Add(switchDevice);
#if DEBUG_SWITCH_MANAGER
        Debug.LogFormat(switchDevice, "[SwitchManager] Registered switch {0}", switchDevice);
#endif
    }

    public void UnregisterSwitch(Switch switchDevice)
    {
        m_Switchs.Remove(switchDevice);
    }

    public void SetupSwitches()
    {
        foreach (var switchDevice in m_Switchs)
        {
            switchDevice.Setup();
        }
    }
}
