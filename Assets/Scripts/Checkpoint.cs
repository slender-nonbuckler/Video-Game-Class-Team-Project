using System;
using UnityEngine;

/**
 * Marks a checkpoint a racer must pass when completing a race.
 *
 * Raises an event whenever a car enters its trigger volume, this class is not responsible for tracking race progress.
 */
public class Checkpoint : MonoBehaviour {
    [SerializeField] private int _id;
    [SerializeField] public Transform resetTransform;
    
    public int id {
        get => _id;
        set => _id = value;
    }

    public event EventHandler<RaceId> OnPassCheckpoint;
    
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("TriggerColliders")) {
            return;
        }
        
        RaceId raceId = other.attachedRigidbody.GetComponent<RaceId>();
        if (!raceId) {
            return;
        }
        
        OnPassCheckpoint?.Invoke(this, raceId);
    }

    private void OnDrawGizmos() {
        Collider trigger = GetComponent<Collider>();
        if (!trigger) {
            return;
        }

        Bounds bounds = trigger.bounds;
        Color triggerColor = Color.green;
        triggerColor.a = 0.1f;
        
        Gizmos.color = triggerColor;
        Gizmos.DrawCube(bounds.center, bounds.size);
    }
}