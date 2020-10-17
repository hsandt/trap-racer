#define DEBUG_OBSTACLE_MANAGER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CommonsHelper;
using CommonsPattern;

public class ObstacleManager : SingletonManager<ObstacleManager>
{
	/* State vars */

	// Array of list of registered obstacles
	private List<Obstacle> m_Obstacles = new List<Obstacle>();

	public void RegisterObstacle(Obstacle obstacle)
	{
		m_Obstacles.Add(obstacle);
#if DEBUG_OBSTACLE_MANAGER
		Debug.LogFormat(obstacle, "[ObstacleManager] Registered obstacle {0}", obstacle);
#endif
	}

	public void UnregisterObstacle(Obstacle obstacle)
	{
		m_Obstacles.Remove(obstacle);
	}

	public void SetupObstacles()
	{
		foreach (var obstacle in m_Obstacles)
		{
			obstacle.Setup();
		}
	}
}
