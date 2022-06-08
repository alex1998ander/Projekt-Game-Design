using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] private PickupType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (type)
            {
                case PickupType.MaxSpeedUp:
                {
                    Eventsystem.current.PickupMaxSpeedUp();
                    break;
                }
                case PickupType.SpeedBoost:
                {
                    Eventsystem.current.PickupSpeedBoost();
                    break;
                }
                case PickupType.ExtraAirJump:
                {
                    Eventsystem.current.PickupExtraAirJump();
                    break;
                }
            }

            Destroy(transform.root.gameObject);
        }
    }
}