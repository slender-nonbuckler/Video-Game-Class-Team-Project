using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private TireComponent[] tireComponents;
    [SerializeField] private TireComponent[] steerableTireComponents;
    [SerializeField] private TireComponent[] drivetrainTireComponents;
    [SerializeField] private Transform centerOfMass;

    [Header("Engine Setting")]
    [SerializeField] private AnimationCurve torqueCurve;
    [SerializeField] private float topSpeed;
    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float selfRightingStrength = 0.5f;
    [SerializeField] private float selfRightingDamping = 0.5f;
    [SerializeField] private float airSteeringStrength = 0.5f;


    [Header("Tire Settings")]
    [SerializeField] private float tireRadius;
    [SerializeField] private float tireMass;
    [SerializeField] private float tireRollingDrag;
    [SerializeField] private AnimationCurve tireGripCurve;


    [Header("Suspension Settings")]
    [SerializeField] private float restDistance;
    [SerializeField] private float strength;
    [SerializeField] private float damping;



    [Header("Force Visual Settings")]
    [SerializeField] public bool isShowingRollForce = false;
    [SerializeField] public bool isShowingGripForce = false;
    [SerializeField] public bool isShowingSuspensionForce = false;
    [SerializeField] public bool isShowingTotalForces = false;
    [SerializeField] public bool isShowingWheelContact = false;

    private Vector2 inputs = Vector2.zero;
    private float startHeight;


    //Getters for car selection information display
    public float TopSpeed => topSpeed;
    public float MaxSteeringAngle => maxSteeringAngle;
    public float TireRadius => tireRadius;
    public float TireMass => tireMass;
    public float TireRollingDrag => tireRollingDrag;
    public float RestDistance => restDistance;
    public float Strength => strength;
    public float Damping => damping;

    void Start()
    {
        SyncTireComponentSettings();

        if (centerOfMass != null)
        {
            carRigidbody.centerOfMass = centerOfMass.localPosition;
        }

        startHeight = transform.position.y;
    }

    void Update()
    {
        foreach (TireComponent tireComponent in tireComponents)
        {
            //Debug.Log("Setting Tire Inputs: " + inputs);
            tireComponent.SetInputs(inputs);
        }

        if (Input.GetButtonDown("Jump"))
        {
            Reset();
        }

        UpdateForceVisualSettings();
    }

    void FixedUpdate()
    {
        ApplySelfRightingForce();
        if (getIsGrounded() == false)
        {
            ApplyAirSteering();
        }
    }

    public void SetInputs(Vector2 inputs)
    {
        this.inputs = inputs;
    }

    private bool getIsGrounded()
    {
        foreach (TireComponent tireComponent in tireComponents)
        {
            if (tireComponent.IsGrounded())
            {
                return true;
            }
        }

        return false;
    }

    private void Reset()
    {
        transform.position = new Vector3(transform.position.x, startHeight, transform.position.z);
        transform.rotation = Quaternion.LookRotation(transform.forward);
    }

    private void UpdateForceVisualSettings()
    {
        foreach (TireComponent tireComponent in tireComponents)
        {
            tireComponent.isShowingGripForce = isShowingGripForce;
            tireComponent.isShowingRollForce = isShowingRollForce;
            tireComponent.isShowingSuspensionForce = isShowingSuspensionForce;
            tireComponent.isShowingTotalForces = isShowingTotalForces;
            tireComponent.isShowingWheelContact = isShowingWheelContact;
        }
    }

    private void SyncTireComponentSettings()
    {
        foreach (TireComponent tireComponent in tireComponents)
        {
            tireComponent.SetAttachedRigidbody(carRigidbody);
            tireComponent.maxSpeed = topSpeed;
            tireComponent.torqueCurve = torqueCurve;
            tireComponent.maxSteeringAngle = maxSteeringAngle;

            tireComponent.gripCurve = tireGripCurve;
            tireComponent.tireRadius = tireRadius;
            tireComponent.mass = tireMass;
            tireComponent.rollDrag = tireRollingDrag;

            tireComponent.restDistance = restDistance;
            tireComponent.strength = strength;
            tireComponent.damping = damping;
        }

        foreach (TireComponent tireComponent in steerableTireComponents)
        {
            tireComponent.isSteerable = true;
        }

        foreach (TireComponent tireComponent in drivetrainTireComponents)
        {
            tireComponent.isPartOfDrivetrain = true;
        }
    }

    private Vector3 prevAngularVel = Vector3.zero;
    private void ApplySelfRightingForce()
    {
        var springTorque = selfRightingStrength * Vector3.Cross(carRigidbody.transform.up, Vector3.up);
        var dampTorque = selfRightingDamping * -carRigidbody.angularVelocity;
        carRigidbody.AddTorque(springTorque + dampTorque, ForceMode.Acceleration);
    }

    private void ApplyAirSteering()
    {
        Vector3 direction = Vector3.up;
        Vector3 torque = direction * airSteeringStrength * inputs.x;
        carRigidbody.AddTorque(torque, ForceMode.Acceleration);
    }
}