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
public class PlayerController : MonoBehaviour
{
    // Transform der Kamera
    [SerializeField] private Transform cam;

    // Maximale Geschwindigkeit
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;

    // Maximale Beschleunigung (Auf Boden & in Luft)
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f, maxAirAcceleration = 5f;

    // Maximaler Winkel einer Oberfläche, um als "Boden" zu zählen
    [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 70f;

    // Maximale Geschwindigkeit, mit der man am Boden gesnapped werden kann
    [SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 80f;

    // Maximale Distanz, die der Boden beim Snap entfernt sein darf
    [SerializeField, Min(0f)] private float probeDistance = 1f;

    // Sprunghöhe
    [SerializeField, Range(0f, 100f)] private float jumpHeight = 2f;

    // Maximale Anzahl an Sprüngen in der Luft
    [SerializeField, Range(0, 5)] private int maxAirJumps = 2;

    // Rigidbody des Spielers
    private Rigidbody rb;

    // Controller Input Aktionen (Jump, Move, etc.)
    private PlayerInputActions inputActions;

    // Momentane Werte des linken Joysticks
    private Vector2 leftStickValue;

    // Momentane Gescwindigkeit & Zielgeschwindigkeit
    private Vector3 _velocity, _desiredVelocity;

    // Sprung wurde initiiert
    private bool _desiredJump;

    // Anzahl an verfügbaren Sprüngen
    private int _jumpPhase;

    // Anzahl an Kontaktpunkten, die als Boden/Steigungen zählen
    private int _groundContactCount, _steepContactCount;

    // Spieler befindet sich auf Boden, wenn Kontaktpunkte > 0
    private bool OnGround => _groundContactCount > 0;

    // Spieler befindet sich auf Steigung wenn Kontaktpunkte > 0
    private bool OnSteep => _steepContactCount > 0;

    // Zeitschritte, seit der Spieler das letzte mal den Boden berührt hat
    private int _stepsSinceLastGrounded;

    // Zeitschritte, seit der Spieler gesprungen hat
    private int _stepsSinceLastJump;

    // Maximaler Winkel in Radianten
    private float _minGroundDotProduct;

    // Normale des Kontaktpunktes des Bodens/Steigung
    private Vector3 _contactNormal, _steepNormal;

    private void Awake()
    {
        OnValidate();
        rb = GetComponent<Rigidbody>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => leftStickValue = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => leftStickValue = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => _desiredJump = true;

        inputActions.Player.Glide.performed += ctx => { };
        inputActions.Player.Glide.canceled += ctx => { };
    }

    private void Update()
    {
        // Ist eine Kamera gegeben, wandle gegebene lokale Koordinaten der Kamera (leftStickValue) in 
        // World Space um, damit der Spieler sich abhängig von der Kameraorientierung bewegt
        if (cam)
        {
            // Projiziere forward und right Vektoren der Kamera auf XZ-Ebene, nutze diese für
            // _desiredVelocity, damit Kamera-Rotierung nicht Geschwindigkeit des Spielers beeinflusst
            Vector3 forward = cam.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cam.right;
            right.y = 0f;
            right.Normalize();
            _desiredVelocity = (forward * leftStickValue.y + right * leftStickValue.x) * maxSpeed;
        }
        else
        {
            _desiredVelocity = new Vector3(leftStickValue.x, 0f, leftStickValue.y) * maxSpeed;
        }
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();

        if (_desiredJump)
        {
            _desiredJump = false;
            Jump();
        }

        rb.velocity = _velocity;
        ClearState();
    }

    /// <summary>
    /// Initialisieren der Daten am Anfang von FixedUpdate
    /// </summary>
    private void UpdateState()
    {
        _stepsSinceLastGrounded++;
        _stepsSinceLastJump++;
        _velocity = rb.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            _stepsSinceLastGrounded = 0;
            if (_stepsSinceLastJump > 1)
            {
                _jumpPhase = 0;
            }

            if (_groundContactCount > 1)
            {
                _contactNormal.Normalize();
            }
        }
        else
        {
            _contactNormal = Vector3.up;
        }
    }

    /// <summary>
    /// Zurücksetzen der Daten am Ende von FixedUpdate
    /// </summary>
    private void ClearState()
    {
        _groundContactCount = _steepContactCount = 0;
        _contactNormal = _steepNormal = Vector3.zero;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }

    /// <summary>
    /// Berechnet die Beschleunigung des Spielers auf der Fläche, in der dieser sich bewegen will
    /// </summary>
    private void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);

        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    /// <summary>
    /// Überprüft, ob ein Spieler an Boden "gesnapped" werden soll oder über der Kante hinaus fliegt
    /// </summary>
    /// <returns></returns>
    bool SnapToGround()
    {
        // Ist der Spieler zu lange in der Luft gewesen?
        if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = _velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        // Ist unter dem Spieler boden?
        if (!Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, probeDistance))
        {
            return false;
        }

        if (hit.normal.y < _minGroundDotProduct)
        {
            return false;
        }

        // Wenn nichts falsch, passe Geschwindigkeit an, sodass der Spieler sich weiter den Boden entlang bewegt
        _groundContactCount = 1;
        _contactNormal = hit.normal;
        float dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f)
        {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    /// <summary>
    /// Befindet sich der Spieler nicht auf Boden, nutzt sonstige Steigungen zum erstellen "virtuellen Bodens"
    /// </summary>
    /// <returns></returns>
    private bool CheckSteepContacts()
    {
        if (_steepContactCount > 1)
        {
            _steepNormal.Normalize();
            if (_steepNormal.y >= _minGroundDotProduct)
            {
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Führt Sprung aus
    /// </summary>
    private void Jump()
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = _contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = _steepNormal;
            _jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps)
        {
            if (_jumpPhase == 0)
            {
                _jumpPhase = 1;
            }

            jumpDirection = _contactNormal;
        }
        else
        {
            return;
        }

        _stepsSinceLastJump = 0;
        _jumpPhase++;

        // Berechne mit angegebener Sprunghöhe die nötige Upwards-Velocity
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

        // +Vector3.up damit WallJump nach oben gerichtet ist
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection);

        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        _velocity += jumpDirection * jumpSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollsion(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollsion(collision);
    }

    /// <summary>
    /// Überprüft mithilfe der Kollision, ob diese als Boden zählt und damit der Spieler grounded ist
    /// </summary>
    /// <param name="collision">Daten der Kollision</param>
    void EvaluateCollsion(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= _minGroundDotProduct)
            {
                _groundContactCount++;
                _contactNormal += normal;
            }
            else if (normal.y >= -0.01f)
            {
                _steepContactCount++;
                _steepNormal += normal;
            }
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    /// <summary>
    /// Editor only, wird der maxGroundAngle geändert, führe Kosinus-rechnung aus
    /// </summary>
    private void OnValidate()
    {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    public float MaxSpeed
    {
        get => maxSpeed;
        set => maxSpeed = value;
    }

    public float MaxAcceleration
    {
        get => maxAcceleration;
        set
        {
            maxAcceleration = value;
            maxAirAcceleration = maxAcceleration / 2.0f;
        }
    }

    public int JumpPhase
    {
        get => _jumpPhase;
        set => _jumpPhase = value;
    }
}