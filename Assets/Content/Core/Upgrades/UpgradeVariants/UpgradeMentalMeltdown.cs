﻿using UnityEngine;

public class UpgradeMentalMeltdown : Upgrade
{
    public override string Name => "Mental Meltdown";
    public override string Description => "Your bullets possess the power to crash your enemies' brains, leaving them searching for a Ctrl+Alt+Delete button to reboot their shattered thoughts.";
    public override string HelpfulDescription => "Bullets stun the opponent";

    public override bool OnBulletImpact(IUpgradeableBullet upgradeableBullet, Collision2D collision)
    {
        return upgradeableBullet.ExecuteMentalMeltdown_OnBulletImpact(collision);
    }
}