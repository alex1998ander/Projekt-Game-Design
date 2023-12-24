using UnityEngine;

public class UpgradeStickyFingers : Upgrade
{
    public override string Name => "Sticky Fingers";
    public override UpgradeIdentification UpgradeIdentification => UpgradeIdentification.StickyFingers;
    public override UpgradeType UpgradeType => UpgradeType.Weapon;
    public override string FlavorText => "";
    public override string Description => "";

    public override float FireCooldown => -0.5f;
    public override float WeaponSpray => 2f;

    public override void OnFire(PlayerController playerController, PlayerWeapon playerWeapon, Vector2 fireDirectionOverwrite = default)
    {
        PlayerData.stickyFingers = true;
        PlayerData.canDash = false;
        PlayerData.canReload = false;
    }

    public override void OnMagazineEmptied(PlayerController playerController, PlayerWeapon playerWeapon)
    {
        PlayerData.stickyFingers = false;
        PlayerData.canDash = true;
        PlayerData.canReload = true;
    }
}