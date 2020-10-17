#define DEBUG_GATE_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class GateManager : SingletonManager<GateManager>
{
	/* State vars */

	// Array of list of registered gates, indexed by color as int
	private List<Gate>[] m_GatesByColor;

	protected override void Init()
	{
		int colorCount = EnumUtil.GetCount<GameColor>();
		m_GatesByColor = new List<Gate>[colorCount];

		for (int i = 0; i < colorCount; ++i)
		{
			m_GatesByColor[i] = new List<Gate>();
		}
	}

	public void RegisterGate(Gate gate)
	{
		m_GatesByColor[(int) gate.Color].Add(gate);
#if DEBUG_GATE_MANAGER
		Debug.LogFormat(gate, "[GateManager] Registered gate {0} with color {1}", gate, gate.Color);
#endif
	}

	public void UnregisterGate(Gate gate)
	{
		m_GatesByColor[(int) gate.Color].Remove(gate);
	}

	public void SetupGates()
	{
		int colorCount = EnumUtil.GetCount<GameColor>();
		for (int i = 0; i < colorCount; ++i)
		{
			foreach (var gate in m_GatesByColor[i])
			{
				gate.Setup();
			}
		}
	}

	public void ToggleGatesByColor(GameColor color)
	{
#if DEBUG_GATE_MANAGER
		Debug.LogFormat(this, "[GateManager] Toggle gates by color: {0}", color);
#endif
		foreach (var gate in m_GatesByColor[(int)color]) {
			gate.Toggle();
		}
	}
}
