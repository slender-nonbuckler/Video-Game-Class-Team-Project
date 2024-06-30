using System;
using UnityEngine;

/**
 * Marks a checkpoint a racer must pass when completing a race.
 *
 * Raises an event whenever a car enters its trigger volume, this class is not responsible for tracking race progress.
 */
public class Checkpoint : MonoBehaviour {
    [SerializeField] private int _id;
    public int id {
        get => _id;
        private set => _id = value;
    }

    public event EventHandler OnPassCheckpoint;
    
    private void OnTriggerEnter(Collider other) {
        if (other.attachedRigidbody.CompareTag("Car")) {
            OnPassCheckpoint?.Invoke(this, EventArgs.Empty);   
        }
    }
}