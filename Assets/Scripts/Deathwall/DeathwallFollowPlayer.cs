using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kontrolliert die Bewegung der Deathwall, die den Spieler im Laufe des Spiels verfolgt.
/// </summary>
public class DeathwallFollowPlayer : MonoBehaviour
{
    // Transform des Spielers
    [SerializeField] private Transform playerTransform;

    // Minimale/Maximale Geschwindigkeit der Deathwall
    [SerializeField, Range(0f, 100f)] private float minSpeed = 10f, maxSpeed = 20f;

    // Distanz vom Spieler, ab der die Deathwall seine minimale bzw. maximale Geschwindigkeit hat
    [SerializeField, Range(0f, 100f)] private float minSpeedDistance = 20f, maxSpeedDistance = 40f;

    // Distanz, die die Deathwall vom Spieler nicht überschreiten kann
    [SerializeField, Range(0f, 100f)] private float maxDistanceFromPlayer = 80f;

    void FixedUpdate()
    {
        CopyPlayerPosition();

        float distanceWallToPlayer = Mathf.Abs(playerTransform.position.z - this.transform.position.z);
        FixMaxDistance(distanceWallToPlayer);
        CalculateNewPosition(distanceWallToPlayer);
    }

    private void CopyPlayerPosition()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 newPos = new Vector3(playerPos.x, playerPos.y, this.transform.position.z);
        this.transform.position = newPos;
    }

    /// <summary>
    /// Korrigiere die Position der Deathwall, ist diese zu weit vom Spieler entfernt.
    /// </summary>
    /// <param name="distance">Distanz zwischen Spieler und Deathwall.</param>
    private void FixMaxDistance(float distance)
    {
        if (distance > maxDistanceFromPlayer)
        {
            float newZ = playerTransform.position.z - maxDistanceFromPlayer;
            Vector3 pos = this.transform.position;
            this.transform.position = new Vector3(pos.x, pos.y, newZ);
        }
    }

    /// <summary>
    /// Berechne die neue Position der Deathwall, basierend auf die Distanz vom Spieler.
    /// </summary>
    /// <param name="distance">Distanz zwischen Spieler und Deathwall.</param>
    private void CalculateNewPosition(float distance)
    {
        float distanceFactor = Mathf.InverseLerp(minSpeedDistance, maxSpeedDistance, distance);
        float positionFactor = Mathf.Lerp(minSpeed, maxSpeed, distanceFactor);
        Vector3 newPosition = new Vector3(0, 0, positionFactor);
        this.transform.position += newPosition * Time.fixedDeltaTime;
    }

    private void OnValidate()
    {
        if (minSpeed > maxSpeed)
        {
            minSpeed = maxSpeed;
        }

        if (maxSpeedDistance > maxDistanceFromPlayer)
        {
            maxSpeedDistance = maxDistanceFromPlayer;
        }

        if (minSpeedDistance > maxSpeedDistance)
        {
            minSpeedDistance = maxSpeedDistance;
        }
    }
}