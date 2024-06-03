using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDriver : MonoBehaviour{
    [Header("References")]
    [SerializeField] private CarController carController;
    private Vector2 inputs = Vector2.up * 0.5f;

    void Update() {
        UpdateInputs();
        if (carController != null) {
            carController.SetInputs(inputs);
        }
    }

    private void UpdateInputs() {
        Vector3 randomVector = Random.onUnitSphere;
        Vector2 changeInInputs = new Vector2(randomVector.x, randomVector.y);
        inputs = inputs + changeInInputs * Time.deltaTime;
        inputs.x = Mathf.Clamp(inputs.x, -1f, 1f);
        inputs.y = Mathf.Clamp(inputs.y, -1f, 1f);
    }
}
