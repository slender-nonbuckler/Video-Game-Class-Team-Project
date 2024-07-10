using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Common;
using UnityEngine;

public class AiDataManager : MonoBehaviour {
    public const string TAG = "AiDataManager";

    [SerializeField] private Bounds hashGridBounds = new Bounds();
    [SerializeField] private Vector2Int hashGridDimensions = new Vector2Int(10, 10);
    
    private HashSet<Transform> waypoints = new HashSet<Transform>();
    private SpatialHashgrid hashGrid;
    private Stack<Transform> toAddToHashgrid = new Stack<Transform>();
    private Stack<Transform> toRemoveFromHashgrid = new Stack<Transform>();

    void Awake() {
        if (!CompareTag(TAG)) {
            tag = TAG;
        }
        
        hashGrid = new SpatialHashgrid(hashGridBounds, hashGridDimensions);
    }

    void Update() {
        while (toAddToHashgrid.Count > 0) {
            Transform transformToAdd = toAddToHashgrid.Pop();
            if (!transformToAdd) {
                continue;
            }
            hashGrid.AddItem(transformToAdd);
        }
        
        while (toRemoveFromHashgrid.Count > 0) {
            Transform transformToRemove = toRemoveFromHashgrid.Pop();
            if (!transformToRemove) {
                continue;
            }
            hashGrid.RemoveItem(transformToRemove);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(hashGridBounds.center, hashGridBounds.size);
    }

    public void AddWaypoint(AiWaypoint waypoint) {
        waypoints.Add(waypoint.transform);
        toAddToHashgrid.Push(waypoint.transform);
    }
    
    public void RemoveWaypoint(AiWaypoint waypoint) {
        waypoints.Remove(waypoint.transform);
        toRemoveFromHashgrid.Push(waypoint.transform);
    }

    public List<Transform> GetAllWaypoints() {
        return waypoints.ToList();
    }

    public HashSet<Transform> GetWaypointsNearby(Vector3 position, float radius) {
        return hashGrid.GetNearby(position, radius);
    }
}