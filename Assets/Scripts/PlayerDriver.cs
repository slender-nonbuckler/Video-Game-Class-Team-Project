using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriver : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CarController carController;

    void Update() {
        Vector2 inputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (carController != null) {
            //Debug.Log("Setting Car Inputs: " + inputs);
            carController.SetInputs(inputs);
        }
    }
}
