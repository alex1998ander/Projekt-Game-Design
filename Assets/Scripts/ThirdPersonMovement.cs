using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Kontrolliert die Bewegung des Spielers
/// </summary>
public class ThirdPersonMovement : MonoBehaviour
{
    // Geschwindigkeit des Spielers
    public float speed = 15f;

    // Zeit zwischen zwei Drehzuständen des Spielers
    public float turnSmoothTime = 0.1f;

    // Transform der Kamera
    public Transform cam;

    // RigidBody des Spielers
    private Rigidbody rb;

    // Controller Input Aktionen (Jump, Move, etc.)
    private PlayerInputActions inputActions;

    // Momentane Werte der Joysticks
    private Vector2 leftStickValue;
    private Vector2 rightStickValue;

    private float turnSmoothVelocity;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody>();

        inputActions.Player.Jump.performed += ctx => rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        inputActions.Player.Move.performed += ctx => leftStickValue = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => leftStickValue = Vector2.zero;
        inputActions.Player.Look.performed += ctx => rightStickValue = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => rightStickValue = Vector2.zero;
    }

    void FixedUpdate()
    {
        Vector3 movementDirection = new Vector3(leftStickValue.x, 0, leftStickValue.y).normalized;

        // Rotation berechnen, damit Spieler in Bewegungsrichtung gedreht ist
        if (leftStickValue != Vector2.zero)
        {
            // Berechne Winkel des Spielers, der in Bewegungsrichtung in der XZ-Ebene gucken soll
            float targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg +
                                cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 forwardDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            rb.AddForce(forwardDirection.normalized * speed, ForceMode.Force);
        }
    }

    /// <summary>
    /// Charaktersprung beim Button-Press
    /// </summary>
    /// <param name="context"></param>
    // public void Jump(InputAction.CallbackContext context)
    // {
    //     Debug.Log("Jump! ");
    //     controller.Move(Vector3.up * Time.deltaTime);
    // }
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
}