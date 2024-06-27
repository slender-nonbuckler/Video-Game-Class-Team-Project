using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDriver : MonoBehaviour {
    [Header("References")]
    [SerializeField] private CarController carController;
    public void SetCarController(CarController controller)
    {
        carController = controller;
    }
    void Update() {
        Vector2 inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (carController != null) {
            //Debug.Log("Setting Car Inputs: " + inputs);
            carController.SetInputs(inputs);
        }
    }
}
