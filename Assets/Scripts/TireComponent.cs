using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireComponent : MonoBehaviour {
    [Header("References")]
    [SerializeField] public Rigidbody rigidbodyAttachedTo;
    [SerializeField] private LayerMask drivableLayers;
    [SerializeField] private Transform tireVisual;
    [SerializeField] public float tireRadius;

    
    [Header("Steering Settings")]
    [SerializeField] public bool isSteerable;
    [SerializeField] public float maxSteeringAngle;

    [Header("Suspension Settings")]
    [SerializeField] public float restDistance;
    [SerializeField] public float strength; 
    [SerializeField] public float damping;

    [Header("Grip Settings")]
    [SerializeField] public AnimationCurve gripCurve;
    [SerializeField] public float mass = 0.1f;

    [Header("Roll Settings")]
    [SerializeField] public bool isPartOfDrivetrain;
    [SerializeField] public AnimationCurve torqueCurve;
    [SerializeField] public float maxSpeed = 30f;
    [SerializeField] public float rollDrag = 1f;

    [Header("Force Visual Settings")]
    [SerializeField] public bool isShowingRollForce = false;
    [SerializeField] public bool isShowingGripForce = false;
    [SerializeField] public bool isShowingSuspensionForce = false;
    [SerializeField] public bool isShowingTotalForces = false;
    [SerializeField] public bool isShowingWheelContact = false;

    private float accelerationInput = 0f;
    private float steerInput = 0f;
    private bool isGrounded = false;

    void Update() {
    }

    void FixedUpdate() {
        Steer();
        ApplyTotalForces();
        UpdateTireVisuals();
    }

    public bool IsGrounded() {
        return isGrounded;
    }

    public void SetInputs(Vector2 inputs) {
        accelerationInput = Mathf.Clamp(inputs.y, -1f, 1f);
        steerInput = Mathf.Clamp(inputs.x, -1f, 1f);
    }

    public void SetAttachedRigidbody(Rigidbody rigidbody) {
        rigidbodyAttachedTo = rigidbody;
    }

    private void ApplyTotalForces() {
        RaycastHit raycastHit;
        
        if (Physics.Raycast(transform.position, -transform.up, out raycastHit, restDistance, drivableLayers)) {
            isGrounded = true;
            Vector3 totalForces = GetSuspensionForce(raycastHit) + GetGripForce(raycastHit) + GetRollForce(raycastHit); 
            rigidbodyAttachedTo.AddForceAtPosition(totalForces, transform.position);
            
            if (isShowingSuspensionForce) { Debug.DrawLine(raycastHit.point + GetSuspensionForce(raycastHit), raycastHit.point, Color.green); }
            if (isShowingRollForce) { Debug.DrawLine(raycastHit.point + GetRollForce(raycastHit), raycastHit.point, Color.blue); }
            if (isShowingGripForce) { Debug.DrawLine(raycastHit.point + GetGripForce(raycastHit), raycastHit.point, Color.red); }
            if (isShowingTotalForces) { Debug.DrawLine(raycastHit.point + totalForces, raycastHit.point, Color.cyan); }
            if (isShowingWheelContact) { Debug.DrawLine(transform.position, raycastHit.point, Color.yellow); }
        } else {
            isGrounded = false;
            if (isShowingWheelContact) { Debug.DrawLine(transform.position, transform.position + restDistance * -transform.up, Color.magenta); }
        }
    }

    private Vector3 GetSuspensionForce(RaycastHit raycastHit) {
        Vector3 worldVelocity = rigidbodyAttachedTo.GetPointVelocity(transform.position);
        float offset = restDistance - raycastHit.distance;
        Vector3 springDirection = transform.up;
        float springVelocity = Vector3.Dot(springDirection, worldVelocity);

        float force = (offset * strength) - (springVelocity * damping);
        return springDirection * force;
    }

    private Vector3 GetGripForce(RaycastHit raycastHit) {
        Vector3 worldVelocity = rigidbodyAttachedTo.GetPointVelocity(transform.position);
        float slideVelocity = Vector3.Dot(transform.right, worldVelocity);
        float slideRatio = slideVelocity / worldVelocity.magnitude;

        float gripFactor = gripCurve.Evaluate(Mathf.Abs(slideRatio));
        float desiredVelocityChange = -slideVelocity * gripFactor;
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        return transform.right * (mass * desiredAcceleration);
    }

    private Vector3 GetRollForce(RaycastHit raycastHit) {
        Vector3 rollForce = Vector3.zero;

        if (isPartOfDrivetrain == false) {
            return rollForce;
        }

        Vector3 accelerationDirection = transform.forward;
        Vector3 rigidbodyForward = rigidbodyAttachedTo.gameObject.transform.forward;
        float rollVelocity = Vector3.Dot(rigidbodyForward, rigidbodyAttachedTo.velocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(rollVelocity) / maxSpeed);

        if (accelerationInput > 0.0f) {
            if (normalizedSpeed == 1f) {
                return Vector3.zero;
            }
            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * accelerationInput;

            rollForce = accelerationDirection * (availableTorque * maxSpeed);
        } else if (accelerationInput < 0.0f) {
            if (normalizedSpeed == 1f) {
                return Vector3.zero;
            }
            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * accelerationInput;

            rollForce = accelerationDirection * (availableTorque * maxSpeed);
        } else {
            float normalizedSignedSpeed = Mathf.Clamp(rollVelocity / maxSpeed, -1f, 1f);
            rollForce = -rigidbodyForward * (rollDrag * normalizedSignedSpeed);
        }

        return rollForce;
    }

    private void Steer() {
        if (isSteerable == false) {
            return;
        }

        Vector3 eulerAngles = transform.localRotation.eulerAngles;
        float steerAngle = steerInput * maxSteeringAngle;
        eulerAngles.y = steerAngle;
        transform.localRotation = Quaternion.Euler(eulerAngles);
    }

    private void UpdateTireVisuals() {
        if (!tireVisual) {
            return;
        }

        RaycastHit raycastHit;
        bool isHit = Physics.Raycast(transform.position, -transform.up, out raycastHit, restDistance, drivableLayers);

        UpdateTirePosition(raycastHit, isHit);
        RotateTireVisual(raycastHit, isHit);
    }

    private void RotateTireVisual(RaycastHit raycastHit, bool isHit) {
        Vector3 worldVelocity;
        if (isHit) {
            worldVelocity = rigidbodyAttachedTo.GetPointVelocity(transform.position);  
        } else {
            worldVelocity = rigidbodyAttachedTo.velocity;
        }

        Vector3 rotationAxis = Vector3.right;
        float rollVelocity = Vector3.Dot(transform.forward, worldVelocity);
        float degreesToRotate = 360 * GetRevolutions(tireRadius, rollVelocity) * Time.fixedDeltaTime;
        tireVisual.Rotate(degreesToRotate * rotationAxis);
    }

    private void UpdateTirePosition(RaycastHit raycastHit, bool isHit) {
        float extensionAmount;
        if (isHit) {
            extensionAmount = raycastHit.distance - tireRadius;
        } else {
            extensionAmount = restDistance - tireRadius;
        }

        Vector3 extensionDirection = -transform.up;
        tireVisual.position = transform.position + extensionDirection * extensionAmount;
    }

    private float GetRevolutions(float radius, float speed) {
        float circumference = 2 * Mathf.PI * radius;
        return speed / circumference;
    }
}
