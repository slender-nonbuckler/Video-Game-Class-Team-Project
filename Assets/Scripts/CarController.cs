using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {
    [Header("References")] [SerializeField]
    private Rigidbody carRigidbody;

    [SerializeField] private TireComponent[] tireComponents;
    [SerializeField] private TireComponent[] steerableTireComponents;
    [SerializeField] private TireComponent[] drivetrainTireComponents;
    [SerializeField] private Transform centerOfMass;

    [Header("Engine Setting")] [SerializeField]
    private AnimationCurve torqueCurve;

    [SerializeField] private float topSpeed;
    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float selfRightingStrength = 0.5f;
    [SerializeField] private float selfRightingDamping = 0.5f;
    [SerializeField] private float airSteeringStrength = 0.5f;


    [Header("Tire Settings")] [SerializeField]
    private float tireRadius;

    [SerializeField] private float tireMass;
    [SerializeField] private float tireRollingDrag;
    [SerializeField] private AnimationCurve tireGripCurve;


    [Header("Suspension Settings")] [SerializeField]
    private float restDistance;

    [SerializeField] private float strength;
    [SerializeField] private float damping;


    [Header("Force Visual Settings")] [SerializeField]
    public bool isShowingRollForce = false;

    [SerializeField] public bool isShowingGripForce = false;
    [SerializeField] public bool isShowingSuspensionForce = false;
    [SerializeField] public bool isShowingTotalForces = false;
    [SerializeField] public bool isShowingWheelContact = false;

    private Vector2 inputs = Vector2.zero;
    private bool isResetDown;
    private float startHeight;
    private String currentGameObjectTag;

    private float resetCooldown = 2f;
    private float resetCooloffTime = 0f;


    //Getters for car selection information display
    public float TopSpeed {
        get {
            Debug.Log($"Getting TopSpeed: {topSpeed}");
            return topSpeed;
        }
    }

    public float MaxSteeringAngle => maxSteeringAngle;
    public float TireRadius => tireRadius;
    public float TireMass => tireMass;
    public float TireRollingDrag => tireRollingDrag;
    public float RestDistance => restDistance;
    public float Strength => strength;
    public float Damping => damping;

    void Start()
    {
        currentGameObjectTag = gameObject.tag;
        SyncTireComponentSettings();

        if (centerOfMass != null) {
            carRigidbody.centerOfMass = centerOfMass.localPosition;
        }

        startHeight = transform.position.y;
    }

    void Update()
    {
        foreach (TireComponent tireComponent in tireComponents)
        {
            tireComponent.SetInputs(inputs);
        }

        if (isResetDown) {
            Reset();
        }

        UpdateForceVisualSettings();
    }

    void FixedUpdate() {
        ApplySelfRightingForce();
        if (getIsGrounded() == false) {
            ApplyAirSteering();
        }
    }

    public void SetInputs(Vector2 inputs, bool isResetDown) {
        this.inputs = inputs;
        this.isResetDown = isResetDown;
    }

    private bool getIsGrounded() {
        foreach (TireComponent tireComponent in tireComponents) {
            if (tireComponent.IsGrounded()) {
                return true;
            }
        }

        return false;
    }

    private void Reset() {
        if (resetCooloffTime > Time.time) {
            return;
        }

        resetCooloffTime = Time.time + resetCooldown;
        
        transform.position = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z);
        transform.rotation = Quaternion.LookRotation(transform.forward);
        carRigidbody.angularVelocity = Vector3.zero;
        carRigidbody.velocity = Vector3.zero;
    }

    private void UpdateForceVisualSettings() {
        foreach (TireComponent tireComponent in tireComponents) {
            tireComponent.isShowingGripForce = isShowingGripForce;
            tireComponent.isShowingRollForce = isShowingRollForce;
            tireComponent.isShowingSuspensionForce = isShowingSuspensionForce;
            tireComponent.isShowingTotalForces = isShowingTotalForces;
            tireComponent.isShowingWheelContact = isShowingWheelContact;
        }
    }

    private void SyncTireComponentSettings() {
        foreach (TireComponent tireComponent in tireComponents) {
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

        foreach (TireComponent tireComponent in steerableTireComponents) {
            tireComponent.isSteerable = true;
        }

        foreach (TireComponent tireComponent in drivetrainTireComponents) {
            tireComponent.isPartOfDrivetrain = true;
        }
    }

    private Vector3 prevAngularVel = Vector3.zero;

    private void ApplySelfRightingForce() {
        var springTorque = selfRightingStrength * Vector3.Cross(carRigidbody.transform.up, Vector3.up);
        var dampTorque = selfRightingDamping * -carRigidbody.angularVelocity;
        carRigidbody.AddTorque(springTorque + dampTorque, ForceMode.Acceleration);
    }

    private void ApplyAirSteering() {
        Vector3 direction = Vector3.up;
        Vector3 torque = direction * airSteeringStrength * inputs.x;
        carRigidbody.AddTorque(torque, ForceMode.Acceleration);
    }


    //Helper methods for powerups

    private float originalTopSpeed;
    private List<PowerupManager.PowerupInfo> activePowerups = new List<PowerupManager.PowerupInfo>();

    public void AddActivePowerup(PowerupManager.PowerupInfo powerup) {
        activePowerups.Add(powerup);
        StartCoroutine(RemovePowerupAfterDuration(powerup));
    }

    private IEnumerator RemovePowerupAfterDuration(PowerupManager.PowerupInfo powerup) {
        yield return new WaitForSeconds(powerup.duration);
        if (powerup.removeEffect != null) {
            powerup.removeEffect(this);
            Debug.Log($"Removed powerup: {powerup.name}");
        }

        activePowerups.Remove(powerup);
    }

    public void SetTemporaryTopSpeed(float newTopSpeed) {
        Debug.Log($"Setting temporary top speed: {newTopSpeed}");
        originalTopSpeed = topSpeed;
        topSpeed = newTopSpeed;
        UpdateTireComponentSpeeds(newTopSpeed);
    }

    public void ResetTopSpeed() {
        Debug.Log($"Resetting top speed to: {originalTopSpeed}");
        topSpeed = originalTopSpeed;
        UpdateTireComponentSpeeds(originalTopSpeed);
    }

    private void UpdateTireComponentSpeeds(float speed) {
        Debug.Log($"Updating tire component speeds to: {speed}");
        foreach (TireComponent tireComponent in tireComponents) {
            tireComponent.maxSpeed = speed;
            Debug.Log($"Tire component speed set to: {tireComponent.maxSpeed}");
        }
    }

    public void ResetAllPowerups() {
        foreach (var powerup in activePowerups) {
            if (powerup.removeEffect != null) {
                powerup.removeEffect(this);
            }
        }

        activePowerups.Clear();
    }

    //Helper methods for banana obstacle
    private bool isRotating = false;

    // Add this method to handle car rotation
    public void RotateCar() {
        if (!isRotating) {
            StartCoroutine(RotateCarCoroutine());
        }
    }

    private IEnumerator RotateCarCoroutine() {
        isRotating = true;
        float rotationDuration = 3f; // Duration for three circles
        float rotationSpeed = 360f; // Degrees per second (one circle per second)
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration) {
            float rotationAmount = rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, rotationAmount);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isRotating = false;
    }

    //Helper methods for arresting obstacle
    private float currentSpeed; // This will be modified during gameplay
    private Collider carCollider;


    public void ApplySpeedReduction(float reductionFactor, float minSpeed) {
        foreach (TireComponent tireComponent in tireComponents) {
            // Access the Rigidbody attached to the tire component
            Rigidbody tireRigidbody = tireComponent.rigidbodyAttachedTo;

            if (tireRigidbody != null) {
                // Get current velocity and reduce it
                Vector3 currentVelocity = tireRigidbody.velocity;
                Vector3 reducedVelocity = currentVelocity * reductionFactor;

                // Ensure reduced speed doesn't fall below minimum speed
                reducedVelocity = Vector3.ClampMagnitude(reducedVelocity, minSpeed);

                // Apply the reduced velocity back to the Rigidbody
                tireRigidbody.velocity = reducedVelocity;

                Debug.Log($"Tire component speed set to: {reducedVelocity.magnitude}");
            }
            else {
                Debug.LogWarning("No Rigidbody attached to TireComponent.");
            }
        }
    }

    public void RemoveSpeedReduction() {
        foreach (TireComponent tireComponent in tireComponents) {
            // Access the Rigidbody attached to the tire component
            Rigidbody tireRigidbody = tireComponent.rigidbodyAttachedTo;

            if (tireRigidbody != null) {
                // Restore the original speed (or top speed)
                tireRigidbody.velocity = tireRigidbody.velocity.normalized * tireComponent.maxSpeed;

                Debug.Log($"Tire component speed reset to: {tireComponent.maxSpeed}");
            }
            else {
                Debug.LogWarning("No Rigidbody attached to TireComponent.");
            }
        }
    }
}