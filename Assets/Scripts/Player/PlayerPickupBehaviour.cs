using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Steuert das Verhalten des Spielers, sammelt dieser ein Pickup ein
/// </summary>
public class PlayerPickupBehaviour : MonoBehaviour
{
    // Wert, mit dem maximale Geschwindigkeit erhöht wird
    [SerializeField] private float maxSpeedUpGain = 10f;

    // Wert, mit dem der Spieler kurzzeitig bescheunigt wird
    [SerializeField] private float speedBoostGain = 25f;

    // Rigidbody des Spielers
    private Rigidbody rb;

    // Controller-Skript, dass die Spielerbewegung steuert
    private PlayerController pc;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<PlayerController>();

        Eventsystem.current.OnPickupMaxSpeedUp += OnPickupMaxSpeedUp;
        Eventsystem.current.OnPickupSpeedBoost += OnPickupSpeedBoost;
        Eventsystem.current.OnPickupExtraAirJump += OnPickupExtraAirJump;
    }

    private void OnPickupMaxSpeedUp()
    {
        pc.MaxSpeed += maxSpeedUpGain;
    }

    private void OnPickupSpeedBoost()
    {
        StartCoroutine(SpeedBoostForSeconds(2f));
    }

    private IEnumerator SpeedBoostForSeconds(float seconds)
    {
        pc.MaxSpeed += speedBoostGain;
        pc.MaxAcceleration += speedBoostGain;
        yield return new WaitForSeconds(seconds);
        pc.MaxSpeed -= speedBoostGain;
        pc.MaxAcceleration -= speedBoostGain;
    }

    private void OnPickupExtraAirJump()
    {
        pc.JumpPhase--;
    }

    private void OnDestroy()
    {
        Eventsystem.current.OnPickupMaxSpeedUp -= OnPickupMaxSpeedUp;
        Eventsystem.current.OnPickupSpeedBoost -= OnPickupSpeedBoost;
        Eventsystem.current.OnPickupExtraAirJump -= OnPickupExtraAirJump;
    }
}