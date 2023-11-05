using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UpgradeManager
{
    // upgrades
    private static readonly List<Upgrade> Upgrades = new();

    // stat upgrades
    public static readonly StatUpgrade MaxHealthIncrease = new StatUpgrade("Max Health", 20f, 5f, 3);
    public static readonly StatUpgrade BulletDamageIncrease = new StatUpgrade("Bullet Damage", 10, 5, 3);
    public static readonly StatUpgrade PlayerMovementSpeedIncrease = new StatUpgrade("Movement Speed", 1f, 0.5f, 1);
    public static readonly StatUpgrade BulletKnockbackIncrease = new StatUpgrade("Bullet Knockback", 10, 1, 3);

    private static Upgrade[] _currentUpgradeSelection;

    public static readonly List<Upgrade> DefaultUpgradePool = new()
    {
        new UpgradeBigBullet(),
        new UpgradeBounce(),
        new UpgradeBuckshot(),
        new UpgradeBurst(),
        new UpgradeCarefulPlanning(),
        new UpgradeDemonicPact(),
        //new UpgradeExplosiveBullet(),
        new UpgradeGlassCannon(),
        new UpgradeHealingField(),
        new UpgradeHitman(),
        new UpgradeHoming(),
        new UpgradeMentalMeltdown(),
        new UpgradeMinigun(),
        new UpgradePiercing(),
        new UpgradePhoenix(),
        new UpgradeShockwave(),
        new UpgradeSinusoidalShots(),
        new UpgradeSmartPistol(),
        new UpgradeStickyFingers(),
        new UpgradeSplitShot(),
        new UpgradeStimpack(),
        new UpgradeTank(),
        new UpgradeTimefreeze(),
    };

    private static readonly List<Upgrade> UpgradePool = new();

    public static Upgrade[] GenerateNewRandomUpgradeSelection(int count)
    {
        System.Random rnd = new System.Random();
        _currentUpgradeSelection = UpgradePool.OrderBy(x => rnd.Next()).Take(count).ToArray();

        return _currentUpgradeSelection;
    }

    /// <summary>
    /// Binds an upgrade from the default weapon upgrade pool.
    /// ONLY USE IN SANDBOX!
    /// </summary>
    /// <param name="weaponIndex">Index of the new upgrade in the default weapon upgrade pool</param>
    public static void BindUpgrade_Sandbox(int weaponIndex)
    {
        Upgrade newUpgrade = DefaultUpgradePool[weaponIndex];

        // Replace upgrade
        Upgrades.Add(newUpgrade);
    }

    /// <summary>
    /// Binds an upgrade from the current upgrade selection.
    /// </summary>
    /// <param name="selectionIdx">Index of the new upgrade in the upgrade selection</param>
    public static void BindUpgrade(int selectionIdx)
    {
        Upgrade newUpgrade = _currentUpgradeSelection[selectionIdx];

        // Replace upgrade
        Upgrades.Add(newUpgrade);

        // Remove new upgrade from upgrade pool
        UpgradePool.Remove(newUpgrade);
    }

    /// <summary>
    /// Returns the bound upgrade at the passed index.
    /// </summary>
    /// <param name="index">Upgrade index</param>
    /// <returns>Upgrade at index</returns>
    public static Upgrade GetWeaponUpgradeAtIndex(int index)
    {
        if (index < Upgrades.Count)
        {
            return Upgrades[index];
        }

        return null;
    }

    /// <summary>
    /// Clears all applied upgrades
    /// </summary>
    public static void PrepareUpgrades()
    {
        Upgrades.Clear();
        UpgradePool.Clear();
        UpgradePool.AddRange(DefaultUpgradePool);

        MaxHealthIncrease.Reset();
        BulletDamageIncrease.Reset();
        PlayerMovementSpeedIncrease.Reset();
        BulletKnockbackIncrease.Reset();
    }

    /// <summary>
    /// Calculates the multiplier from the passed values.
    /// </summary>
    /// <param name="attributeSelector">Upgrade attribute</param>
    /// <returns>Calculated Multiplier</returns>
    private static float GetAttributeMultiplier(Func<Upgrade, float> attributeSelector)
    {
        // float multiplier = 1f;
        //
        // foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        // {
        //     multiplier += attributeSelector(upgrade);
        // }
        //
        // foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        // {
        //     multiplier += attributeSelector(upgrade);
        // }

        return Upgrades.Select(upgrade => attributeSelector(upgrade) + 1f).DefaultIfEmpty(1f)
            .Aggregate((acc, attribute) => acc * attribute);
    }

    /// <summary>
    /// Calculates the ability delay multiplier of all upgrades.
    /// </summary>
    /// <returns>Common ability delay multiplier</returns>
    public static float GetAbilityDelayMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.AbilityDelay);
    }

    /// <summary>
    /// Calculates the bullet count adjustment of all upgrades.
    /// </summary>
    /// <returns>Common bullet count adjustment</returns>
    public static int GetBulletCountAdjustment()
    {
        int bulletCountAdjustment = 0;

        foreach (Upgrade upgrade in Upgrades)
        {
            bulletCountAdjustment += upgrade.BulletCount;
        }

        return bulletCountAdjustment;
    }

    /// <summary>
    /// Calculates the bullet damage multiplier of all upgrades.
    /// </summary>
    /// <returns>Common bullet damage multiplier</returns>
    public static float GetBulletDamageMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BulletDamage);
    }

    /// <summary>
    /// Calculates the bullet range multiplier of all upgrades.
    /// </summary>
    /// <returns>Common bullet range multiplier</returns>
    public static float GetBulletRangeMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BulletRange);
    }

    /// <summary>
    /// Calculates the bullet size multiplier of all upgrades.
    /// </summary>
    /// <returns>Common bullet size multiplier</returns>
    public static float GetBulletSizeMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BulletSize);
    }

    /// <summary>
    /// Calculates the bullet speed multiplier of all upgrades.
    /// </summary>
    /// <returns>Common bullet speed multiplier</returns>
    public static float GetBulletSpeedMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BulletSpeed);
    }

    /// <summary>
    /// Calculates the fire delay multiplier of all upgrades.
    /// </summary>
    /// <returns>Common fire delay multiplier</returns>
    public static float GetFireCooldownMultiplier()
    {
        return Mathf.Min(GetAttributeMultiplier(upgrade => upgrade.FireCooldown), Configuration.WeaponFireCooldownMax);
    }

    /// <summary>
    /// Calculates the health multiplier of all upgrades.
    /// </summary>
    /// <returns>Common health multiplier</returns>
    public static float GetHealthMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.Health);
    }

    /// <summary>
    /// Calculates the magazine size multiplier of all upgrades.
    /// </summary>
    /// <returns>Common magazine size multiplier</returns>
    public static float GetMagazineSizeMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.MagazineSize);
    }

    /// <summary>
    /// Calculates the movement speed multiplier of all upgrades.
    /// </summary>
    /// <returns>Common movement speed multiplier</returns>
    public static float GetPlayerMovementSpeedMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.PlayerMovementSpeed);
    }

    /// <summary>
    /// Calculates the reload time multiplier of all upgrades.
    /// </summary>
    /// <returns>Common reload time multiplier</returns>
    public static float GetReloadTimeMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.ReloadTime);
    }

    /// <summary>
    /// Calculates the reload time multiplier of all upgrades.
    /// </summary>
    /// <returns>Common reload time multiplier</returns>
    public static float GetWeaponSprayMultiplier()
    {
        return Mathf.Min(GetAttributeMultiplier(upgrade => upgrade.WeaponSpray), Configuration.WeaponSprayMax);
    }

    /// <summary>
    /// Checks if an upgrade prevents dashing.
    /// </summary>
    /// <returns>Bool, whether dashing is prevented</returns>
    public static bool IsDashPrevented()
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            if (upgrade.PreventDash)
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Executes the player initialization functions of all assigned upgrades
    /// </summary>
    /// <param name="playerController">Player reference</param>
    public static void Init(PlayerController playerController)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.Init(playerController);
        }
    }

    /// <summary>
    /// Executes the bullet initialization functions of all assigned upgrades
    /// </summary>
    /// <param name="playerBullet">Bullet reference</param>
    public static void Init(PlayerBullet playerBullet)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.Init(playerBullet);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player fires
    /// </summary>
    /// <param name="playerController">Player reference</param>
    /// <param name="playerWeapon">Player reference</param>
    public static void OnFire(PlayerController playerController, PlayerWeapon playerWeapon, Vector2 fireDirectionOverwrite = default)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.OnFire(playerController, playerWeapon, fireDirectionOverwrite);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player reloads
    /// </summary>
    /// <param name="playerController">Player reference</param>
    /// <param name="playerWeapon">Player reference</param>
    public static void OnReload(PlayerController playerController, PlayerWeapon playerWeapon)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.OnReload(playerController, playerWeapon);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the players magazine has been emptied
    /// </summary>
    /// <param name="playerController">Player reference</param>
    /// <param name="playerWeapon">Player weapon reference</param>
    public static void OnMagazineEmptied(PlayerController playerController, PlayerWeapon playerWeapon)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.OnMagazineEmptied(playerController, playerWeapon);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the bullet is fired.
    /// This should only be executed when the bullet is instantiated by the weapon.
    /// </summary>
    /// <param name="playerBullet">Player reference</param>
    public static void OnFire(PlayerBullet playerBullet)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.OnFire(playerBullet);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player uses their ability
    /// </summary>
    /// <param name="playerController">Player reference</param>
    /// <param name="playerWeapon">Player weapon reference</param>
    public static void OnAbility(PlayerController playerController, PlayerWeapon playerWeapon)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.OnAbility(playerController, playerWeapon);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades every frame while the bullet is flying
    /// </summary>
    /// <param name="playerBullet">Bullet reference</param>
    public static void BulletUpdate(PlayerBullet playerBullet)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.BulletUpdate(playerBullet);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades for the player every frame 
    /// </summary>
    /// <param name="playerController">Player reference</param>
    public static void PlayerUpdate(PlayerController playerController)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.PlayerUpdate(playerController);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the bullet trigger zone overlaps something
    /// </summary>
    /// <param name="playerBullet">Bullet reference</param>
    /// <param name="other">Collider information</param>
    /// <returns>Bool, whether the bullet should survive afterwards</returns>
    public static bool OnBulletTrigger(PlayerBullet playerBullet, Collider2D other)
    {
        // binary unconditional logical OR ('|' not '||') needed to evaluate every operand (no short-circuiting)
        bool bulletSurvives = false;

        foreach (Upgrade upgrade in Upgrades)
        {
            bulletSurvives |= upgrade.OnBulletTrigger(playerBullet, other);
        }

        return bulletSurvives;
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the bullet collides with something
    /// </summary>
    /// <param name="playerBullet">Bullet reference</param>
    /// <param name="collision">Collision information</param>
    /// <returns>Bool, whether the bullet should survive afterwards</returns>
    public static bool OnBulletCollision(PlayerBullet playerBullet, Collision2D collision)
    {
        // binary unconditional logical OR ('|' not '||') needed to evaluate every operand (no short-circuiting)
        bool bulletSurvives = false;

        foreach (Upgrade upgrade in Upgrades)
        {
            bulletSurvives |= upgrade.OnBulletCollision(playerBullet, collision);
        }

        return bulletSurvives;
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player dies
    /// </summary>
    /// <param name="playerController">Player reference</param>
    public static void OnPlayerDeath(PlayerController playerController)
    {
        foreach (Upgrade upgrade in Upgrades)
        {
            upgrade.OnPlayerDeath(playerController);
        }
    }
}