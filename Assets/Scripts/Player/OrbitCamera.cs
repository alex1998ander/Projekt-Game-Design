using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Realisiert eine sich mit dem Spieler bewegende Kamera
/// </summary>
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    // Transform des Objektes, das die Kamera fokussieren soll
    [SerializeField] private Transform focus = default;

    // Distanz der Kamera vom Objekt
    [SerializeField, Range(1f, 20f)] private float distance = 15f;

    // Radius um Fokuspunkt, um weiche Kamerabewegung zu erlauben
    [SerializeField, Min(0f)] private float focusRadius = 1f;

    // Nach jedem Zeitschritt wird die Distanz zwischen Fokussierten Punkt und Fokus um focusCentering % verringert
    // z.B. bei fC = 0.5; Distanz von 1.00, dann 0.5, dann 0.25, 0.125, ...
    // z.B. bei fC = 0.75; Distanz von 1.0, dann 0.25, 0.0625, 0.015625, ...
    [SerializeField, Range(0f, 1f)] private float focusCentering = 0.5f;

    // Rotationsgeschwindigkeit der Kamera in ° pro Sekunde
    [SerializeField, Range(1f, 360f)] private float rotationSpeed = 90f;

    // Minimaler und maximaler vertikaler Winkel (Damit man nicht die Kamera über Kopf drehen kann)
    [SerializeField, Range(-89f, 89f)] private float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    // Winkelraum, den die Kamera zur horizontalen Drehung zur Verfügung hat
    [SerializeField, Range(0f, 180f)] private float horizontalAngleRange = 45f;

    // Delay in Sekunden, wann die automatische horizontale Winkelanpassung der Kamera starten soll
    [SerializeField, Min(0f)] private float alignDelay = 5f;

    // Kontrolliert Rotationsgeschwindigkeit als Upper Limit, d.h. muss sich die Kamera um alignSmoothRange°
    // oder mehr drehen, rotiert die Kamera mit der vollen Rotationsgeschwindigkeit, davor weniger stark
    [SerializeField, Range(0f, 90f)] private float alignSmoothRange = 45f;

    // Gibt an, welche Objekte beim Boxcast der Kamera nicht berücksichtigt werden sollen
    [SerializeField] private LayerMask obstructionMask = -1;

    // Controller Input Aktionen (Look)
    private PlayerInputActions _inputActions;

    // Momentane Werte des rechten Joysticks
    private Vector2 _rightStickValue;

    // Fokussierter Punkt der Kamera (jetztiger und vorheriger)
    private Vector3 _focusPoint, _previousFocusPoint;

    // Winkel der Kamera zur X- und Y-Achse
    private Vector2 _orbitAngles = new Vector2(45f, 0f);

    // Zeitpunkt der letzten manuellen Drehung der Kamera
    private float _lastManualRotationTime;

    // Kamera-Komponente
    private Camera _regularCamera;

    // Enthält alle Daten der Near Plane der Kamera als Box dargestellt
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = _regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * _regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }


    private void Awake()
    {
        _regularCamera = GetComponent<Camera>();
        _focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(_orbitAngles);

        _inputActions = new PlayerInputActions();
        _inputActions.Player.Look.performed += ctx => _rightStickValue = ctx.ReadValue<Vector2>();
        _inputActions.Player.Look.canceled += ctx => _rightStickValue = Vector2.zero;
    }

    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstraintAngles();
            lookRotation = Quaternion.Euler(_orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }

        Vector3 lookDirection = lookRotation * Vector3.forward; // Quaternion Trick, don't ask me how it works
        Vector3 lookPosition = _focusPoint - lookDirection * distance;

        // Ermittle den idealen Fokus-Punkt, von dem der Boxcast gestartet werden soll
        Vector3 rectOffset = lookDirection * _regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        // Boxcast vom Fokus-Punkt entgegen lookDirection (Also von Spielfigur "zu Bildschirm"),
        // gab es einen Treffer, verschiebe Kamera entsprechend nach vorne, damit Kamera anicht innerhalb Geometrie ist
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation,
                castDistance, obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    /// <summary>
    /// Aktualisiert den Punkt, den die Kamera fokussiert
    /// </summary>
    private void UpdateFocusPoint()
    {
        _previousFocusPoint = _focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, _focusPoint);

            // Berechne Faktor, mit dem Distanz zwischen targetPoint und _focus verringert wird
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }

            // Ist der targetPoint außerhalb des erlaubten Radius, passe t an
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }

            // Führe Verschiebung des focusPoints nach t aus
            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
        else
        {
            _focusPoint = targetPoint;
        }
    }

    /// <summary>
    /// Liest rechten Joystick input, passt Kamerawinkel an
    /// </summary>
    /// <returns>True wenn Änderungen durchgeführt wurde, ansonsten false</returns>
    private bool ManualRotation()
    {
        // y und x ausgetauscht, weil der vertikale Wert des sticks den X-Achsen Winkel beeinflussen soll und anders herum
        Vector2 input = new Vector2(-_rightStickValue.y, _rightStickValue.x);
        if (input != Vector2.zero)
        {
            _orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            _lastManualRotationTime = Time.unscaledTime;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Bewegt sich der Spieler in eine Richtung, rotiert diese Funktion automatisch die Kamera
    /// in Richtung der Spielerbewegung. Führt Dämpfung der Kamerarotation bei langsameren Bewegungen aus.
    /// </summary>
    /// <returns>True, wenn die Kamerarotierung automatisch angepasst wurde, ansonsten false</returns>
    private bool AutomaticRotation()
    {
        if (Time.unscaledTime - _lastManualRotationTime < alignDelay)
        {
            return false;
        }

        // Berechne Bewegungsvektor von alten Fokuspunkt zu neuen, return false wenn Vektor quasi 0, also kaum Bewegung
        // 2D-Vektor in der XZ-Ebene, weil nur horizontale Rotierung relevant ist
        Vector2 movement = new Vector2(
            _focusPoint.x - _previousFocusPoint.x,
            _focusPoint.z - _previousFocusPoint.z
        );
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));

        // Berechne den minimalen Winkel (Delta-Winkel) zwischen momentaner Kamerarotierung und Zielrotierung
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));

        // Berechne maximal mögliche Rotationsänderung, dämpfe diese mithilfe der Länge des Bewegungsvektors
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

        // Ist der Delta-Winkel in der alignSmoothRange, passe Rotationsänderung an, damit die Kamera bei kleinen
        // Rotierungen sich langsamer dreht
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        // Fall, dass sich der Fokus in Richtung der Kamera bewegt
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }

        _orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    /// <summary>
    /// Limitiert Kamera-Winkel
    /// </summary>
    private void ConstraintAngles()
    {
        Debug.Log("angle: " + _orbitAngles.y);
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        _orbitAngles.y = Mathf.Clamp(_orbitAngles.y, -horizontalAngleRange / 2f, horizontalAngleRange / 2f);
        // if (_orbitAngles.y < 0f)
        // {
        //     _orbitAngles.y += 360f;
        // }
        // else if (_orbitAngles.y >= 360f)
        // {
        //     _orbitAngles.y -= 360f;
        // }
    }

    /// <summary>
    /// Berechnet den horizontalen Winkel der Kamera, der zur momentanen Spielerbewegung passt
    /// </summary>
    /// <param name="direction">Richtung des Spielers</param>
    /// <returns>Berechneter Winkel</returns>
    private static float GetAngle(Vector2 direction)
    {
        // Y-Komponente des Vektors ist gleich der Kosinus des benötigten Winkels 
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;

        // Wenn X-Komponente negativ, ist es eine Drehung gegen Uhrzeigersinn, die korrigiert werden muss
        return direction.x < 0f ? 360f - angle : angle;
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }

    /// <summary>
    /// Editor only, maxVerticalAngle soll nicht kleiner als min sein
    /// </summary>
    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }
}