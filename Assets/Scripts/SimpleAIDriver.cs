using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAIDriver : MonoBehaviour
{

    [SerializeField] private Transform[] targetPositionTransforms;

    private CarController carController;
    private Vector3 currentTargetPosition;
    private int targetNumber;

    private void Awake()
    {
        carController = GetComponent<CarController>();
        targetNumber = 0;
    }

    private void Update()
    {
        SetTargetPosition(targetPositionTransforms[targetNumber].position);
        float bufferDistance = 7f;
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

            if (turnAngle > 0)
            {
                newInputs.x = 1f;
            }
            else
            {
                newInputs.x = -1f;
            }
        }
        else
        {
            if(targetNumber == targetPositionTransforms.Length - 1)
            {
                newInputs.x = 0f;
                newInputs.y = 1f;
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
