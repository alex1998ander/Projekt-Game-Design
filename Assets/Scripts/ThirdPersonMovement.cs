using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// Kontrolliert die Bewegung des Spielers
/// </summary>
public class ThirdPersonMovement : MonoBehaviour
{
    public float movementSpeed = 50f; // Geschwindigkeit des Spielers
    public float jumpForce = 40f; // Sprungstärke
    public float jumpFallMultiplier = 2.5f; // Multiplikator, der Gravitation nach vollen Sprung verstärkt
    public float jumpLowFallMultiplier = 2f; // Multiplikator, der Gravitation nach kleinen Sprung verstärkt
    public float turnSmoothTime = 0.2f; // Zeit zwischen zwei Drehzuständen des Spielers
    public float glideDescentVelocity = -0.5f; // Fallgeschwindigkeit des Spielers beim Glide
    public Transform cam; // Transform der Kamera
    private Rigidbody rb; // RigidBody des Spielers
    private PlayerInputActions inputActions; // Controller Input Aktionen (Jump, Move, etc.)
    private Vector2 leftStickValue; // Momentane Werte des linken Joysticks
    private Vector2 rightStickValue; // Momentane Werte des rechten Joysticks
    private float turnSmoothVelocity; // Momentane weiche Drehgeschwindigkeit
    private float descentVelocityDifference; // Differenz momentaner Fallgeschwindigkeit und Glide-Fallgeschwindigkeit
    private bool isGliding = false;
    private bool isGrounded = false;
    private bool isJumping = false;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();

        inputActions.Player.Move.performed += ctx => leftStickValue = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => leftStickValue = Vector2.zero;
        inputActions.Player.Look.performed += ctx => rightStickValue = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => rightStickValue = Vector2.zero;

        inputActions.Player.Jump.performed += ctx =>
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isJumping = true;
            }
        };
        inputActions.Player.Jump.canceled += ctx => isJumping = false;

        inputActions.Player.Glide.performed += ctx =>
        {
            descentVelocityDifference = Mathf.Abs(rb.velocity.y - glideDescentVelocity);
            // rb.useGravity = false;
            isGliding = true;
        };
        inputActions.Player.Glide.canceled += ctx =>
        {
            // rb.useGravity = true;
            isGliding = false;
        };
    }

    private void FixedUpdate()
    {
        Vector3 leftStickTo3DSpace = new Vector3(leftStickValue.x, 0, leftStickValue.y).normalized;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, out var hit, 0.5f);
        Debug.DrawLine(transform.position, hit.point, Color.green);

        // Bewege Spieler nur, gibt es Spieler-Input
        if (leftStickValue != Vector2.zero)
        {
            // Berechne Winkel des Spielers, der in Bewegungsrichtung in der XZ-Ebene gucken soll
            float targetAngle = Mathf.Atan2(leftStickTo3DSpace.x, leftStickTo3DSpace.z) * Mathf.Rad2Deg +
                                cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            rb.MoveRotation(Quaternion.Euler(0f, angle, 0f));

            // Bewege Spieler in Richtung, wo die Kamera hinguckt
            Vector3 forwardDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 movementDirection = CalculateMovementDirection(forwardDirection.normalized);
            rb.AddForce(forwardDirection.normalized * movementSpeed, ForceMode.Force);

            Debug.DrawLine(transform.position, transform.position + forwardDirection.normalized, Color.cyan);
        }

        CalculateFallingVelocity();
    }

    /// <summary>
    /// Passt die Fallgeschwindigkeit des Spielers an
    /// </summary>
    private void CalculateFallingVelocity()
    {
        // Wenn Glide initialisiert wurde und der Spieler schneller fällt als die Glide-Fallgeschwindigkeit,
        // verlangsame den Spieler in den nächsten Updates Schritt für Schritt
        if (isGliding)
        {
            if (rb.velocity.y < glideDescentVelocity)
            {
                Vector3 ascendVelocity = new Vector3(0f, descentVelocityDifference / 10f, 0f);
                rb.AddForce(ascendVelocity, ForceMode.VelocityChange);
            }
        }
        else
        {
            if (rb.velocity.y < 0)
            {
                // rb.velocity += Vector3.up * (Physics.gravity.y * (jumpFallMultiplier - 1) * Time.fixedDeltaTime);
                rb.AddForce(Vector3.up * (Physics.gravity.y * (jumpFallMultiplier - 1) * Time.fixedDeltaTime),
                    ForceMode.VelocityChange);
            }
            else if (rb.velocity.y > 0 && !isJumping)
            {
                // rb.velocity += Vector3.up * (Physics.gravity.y * (jumpLowFallMultiplier - 1) * Time.fixedDeltaTime);
                rb.AddForce(Vector3.up * (Physics.gravity.y * (jumpLowFallMultiplier - 1) * Time.fixedDeltaTime),
                    ForceMode.VelocityChange);
            }
        }
    }

    private Vector3 CalculateMovementDirection(Vector3 forward)
    {
        if (!isGrounded)
        {
            Debug.DrawLine(transform.position, transform.position + forward, Color.cyan);
            return forward;
        }

        Physics.Raycast(transform.position + forward + Vector3.up * 2, Vector3.down, out var hit, Mathf.Infinity);
        return (hit.point - transform.position).normalized;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
}