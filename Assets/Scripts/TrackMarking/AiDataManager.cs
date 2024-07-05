using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiDataManager : MonoBehaviour {
    public const string TAG = "AiDataManager";

    private HashSet<Transform> waypoints = new HashSet<Transform>();

    void Awake() {
        if (!CompareTag(TAG)) {
            tag = TAG;
        }

        waypoints = new HashSet<Transform>();
    }

    public bool AddWaypoint(AiWaypoint waypoint) {
        Debug.Log($"Waypoint count: {waypoints.Count}");
        return waypoints.Add(waypoint.transform);
    }
    
    public bool RemoveWaypoint(AiWaypoint waypoint) {
        return waypoints.Remove(waypoint.transform);
    }

    public List<Transform> GetWaypoints() {
        return waypoints.ToList();
    }
}