﻿public class UpgradeStimpack : Upgrade
{
    public override string Name => "Stimpack";
    public override string HelpfulDescription => "";

    public override void OnAbility(PlayerController playerController)
    {
        playerController.StartCoroutine(Util.OnOffCoroutine(
            () => BulletDamage = Configuration.Stimpack_DamageMultiplier,
            () => BulletDamage = 0f,
            Configuration.Stimpack_Duration)
        );
    }
}