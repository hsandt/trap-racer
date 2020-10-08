#define DEBUG_GATE_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

// SEO: before Gate
public class GateManager : SingletonManager<GateManager>
{
	/* State vars */

	// Array of list of registered gates, indexed by color as int
	private List<Gate>[] m_Gates;

	protected override void Init()
	{
		int colorCount = EnumUtil.GetCount<GameColor>();
		m_Gates = new List<Gate>[colorCount];

		for (int i = 0; i < colorCount; ++i)
		{
			m_Gates[i] = new List<Gate>();
		}
	}

	public void RegisterGate(Gate gate)
	{
		m_Gates[(int)gate.Color].Add(gate);
#if DEBUG_GATE_MANAGER
		Debug.LogFormat(gate, "[GateManager] Registered gate {0} with color {1}", gate, gate.Color);
#endif
	}

	public void UnregisterGate(Gate gate)
	{
		m_Gates[(int)gate.Color].Remove(gate);
	}

	// Update is called once per frame
	public void ToggleGatesByColor(GameColor color)
	{
#if DEBUG_GATE_MANAGER
		Debug.LogFormat(this, "[GateManager] Toggle gates by color: {0}", color);
#endif
		foreach (var gate in m_Gates[(int)color]) {
			gate.Toggle();
		}
	}
}
