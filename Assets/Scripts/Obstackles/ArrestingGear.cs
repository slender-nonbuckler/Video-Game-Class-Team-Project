using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrestingGear : MonoBehaviour {
    public float speedReductionFactor = 0.3f; // Speed reduced to 3/10th
    public float minimumSpeed = 5f; // Minimum speed threshold
    public float effectDuration = 2f; // Duration for the effect to last


    private void OnTriggerEnter(Collider other) {
        CarController carController = other.GetComponentInParent<CarController>();
        if (carController != null) {
            // Apply the speed reduction effect to the car
            carController.ApplySpeedReduction(speedReductionFactor, minimumSpeed);
            StartCoroutine(RemoveEffectAfterDuration(carController));
        }
    }

    private IEnumerator RemoveEffectAfterDuration(CarController carController) {
        // Wait for the effect duration to pass
        yield return new WaitForSeconds(effectDuration);


        // Remove the speed reduction effect
        carController.RemoveSpeedReduction();
    }
}