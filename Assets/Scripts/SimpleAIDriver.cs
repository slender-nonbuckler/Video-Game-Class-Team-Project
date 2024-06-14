using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAIDriver : MonoBehaviour
{

    // Cole Donovan
    // Basic AI Script that handles point to point driving

    [SerializeField] private Transform[] targetPositionTransforms;

    private CarController carController;
    private Vector3 currentTargetPosition;
    private int targetNumber;

    public bool isRaceActive = false;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        targetNumber = 0;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            isRaceActive = true;
        }
    }

    private void FixedUpdate()
    {
        if (isRaceActive)
        {
            ControlAI();
        }
    }

    private void ControlAI()
    {
        SetTargetPosition(targetPositionTransforms[targetNumber].position);
        float bufferDistance = 8.5f;
        float distance = Vector3.Distance(transform.position, currentTargetPosition);


        Vector2 newInputs = new Vector2(0f, 1f);

        if (distance > bufferDistance)
        {
            Vector3 directionToMove = (currentTargetPosition - transform.position).normalized;
            float product = Vector3.Dot(transform.forward, directionToMove);

            if (product > 0)
            {
                newInputs.y = 1f;
            }

            float turnAngle = Vector3.SignedAngle(transform.forward, directionToMove, Vector3.up);
            Debug.Log(turnAngle);
            if (turnAngle > 5)
            {
                newInputs.x = 1f;
                newInputs.y = turnAngle / 180f;
            }
            else if (turnAngle < -5)
            {
                newInputs.x = -1f;
                newInputs.y = turnAngle / -180f;
            }
            else
            {
                newInputs.x = turnAngle / 180f;
            }
        }
        else
        {
            if (targetNumber == targetPositionTransforms.Length - 1)
            {
                newInputs.x = 0f;
                newInputs.y = 0f;
                isRaceActive = false;
            }
            else
            {
                targetNumber++;
            }
        }

        carController.SetInputs(newInputs);
    }

    public void SetTargetPosition(Vector3 target)
    {
        this.currentTargetPosition = target;
    }
}
