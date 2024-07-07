using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
This class is for the obstackle type of banana.
When the car hit it, the car will rotate for three circles
it will appear again after 10s any car hit it.
*/
public class Banana : MonoBehaviour {
    private Renderer bananaRenderer;
    private Collider bananaCollider;

    private void Start() {
        // Cache the renderer and collider components
        bananaRenderer = GetComponent<Renderer>();
        bananaCollider = GetComponent<Collider>();

        if (bananaRenderer == null) {
            bananaRenderer = GetComponentInChildren<Renderer>();
        }

        if (bananaCollider == null) {
            bananaCollider = GetComponentInChildren<Collider>();
        }

        if (bananaRenderer == null || bananaCollider == null) {
            Debug.LogError("Banana: Missing Renderer or Collider component!");
        }
    }

    private void OnTriggerEnter(Collider other) {
        CarController carController = other.GetComponentInParent<CarController>();
        if (carController != null) {
            // Apply the rotating effect to the car
            carController.RotateCar();

            // Start the coroutine to handle disappearance and reappearance
            StartCoroutine(HandleBananaHit());
        }
    }

    private IEnumerator HandleBananaHit() {
        // Disable the banana's renderer and collider
        if (bananaRenderer) {
            bananaRenderer.enabled = false;
        }

        if (bananaCollider) {
            bananaCollider.enabled = false;
        }

        // Wait for 10 seconds
        yield return new WaitForSeconds(10f);

        // Re-enable the banana's renderer and collider
        if (bananaRenderer) {
            bananaRenderer.enabled = true;
        }

        if (bananaCollider) {
            bananaCollider.enabled = true;
        }
    }
}