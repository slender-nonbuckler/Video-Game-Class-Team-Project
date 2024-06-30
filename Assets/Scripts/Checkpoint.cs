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

    public event EventHandler<CarController> OnPassCheckpoint;
    
    private void OnTriggerEnter(Collider other) {
        if (!other.attachedRigidbody.CompareTag("Car")) {
             return;
        }

        CarController carController = other.attachedRigidbody.GetComponent<CarController>();
        if (!carController) {
            return;
        }
        
        OnPassCheckpoint?.Invoke(this, carController);
    }
}