using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eventsystem : MonoBehaviour
{
    public static Eventsystem current;

    private void Awake()
    {
        current = this;
    }

    // ----- Pickups -----

    public event Action OnPickupMaxSpeedUp;
    public event Action OnPickupSpeedBoost;
    public event Action OnPickupExtraAirJump;

    public void PickupMaxSpeedUp()
    {
        OnPickupMaxSpeedUp?.Invoke();
    }

    public void PickupSpeedBoost()
    {
        OnPickupSpeedBoost?.Invoke();
    }

    public void PickupExtraAirJump()
    {
        OnPickupExtraAirJump?.Invoke();
    }
}