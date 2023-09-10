using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UpgradeManager
{
    // upgrades
    private static readonly List<WeaponUpgrade> WeaponUpgrades = new();
    private static readonly List<AbilityUpgrade> AbilityUpgrades = new();

    // stat upgrades
    public static readonly StatUpgrade MaxHealthIncrease = new StatUpgrade(10, 1, 3);
    public static readonly StatUpgrade BulletDamageIncrease = new StatUpgrade(10, 1, 3);
    public static readonly StatUpgrade PlayerMovementSpeedIncrease = new StatUpgrade(10, 1, 3);
    public static readonly StatUpgrade BulletKnockbackIncrease = new StatUpgrade(10, 1, 3);

    //private static int _nextReplacementIndex;

    private static WeaponUpgrade[] _currentWeaponUpgradeSelection;
    private static AbilityUpgrade[] _currentAbilityUpgradeSelection;

    private static readonly List<WeaponUpgrade> WeaponUpgradePool = new()
    {
        new UpgradeHitman(),
        new UpgradeBuckshot(),
        new UpgradeBurst(),
        new UpgradeBounce(),
        new UpgradeCarefulPlanning(),
        new UpgradeTank(),
        //new UpgradeExplosiveBullet(),
        new UpgradeHoming(),
        new UpgradeBigBullet(),
        new UpgradeMentalMeltdown(),
        new UpgradeDemonicPact(),
        //new UpgradeDrill(),
        new UpgradeGlassCannon()
    };

    private static readonly List<AbilityUpgrade> AbilityUpgradePool = new()
    {
        new UpgradeHealingField(),
        new UpgradePhoenix()
    };

    public static WeaponUpgrade[] GenerateNewRandomWeaponUpgradeSelection(int count)
    {
        System.Random rnd = new System.Random();
        _currentWeaponUpgradeSelection = WeaponUpgradePool.OrderBy(x => rnd.Next()).Take(count).ToArray();

        return _currentWeaponUpgradeSelection;
    }

    public static AbilityUpgrade[] GenerateNewRandomAbilityUpgradeSelection(int count)
    {
        System.Random rnd = new System.Random();
        _currentAbilityUpgradeSelection = AbilityUpgradePool.OrderBy(x => rnd.Next()).Take(count).ToArray();

        return _currentAbilityUpgradeSelection;
    }

    /// <summary>
    /// Binds an upgrade from the current upgrade selection to the upgrade inventory to the upgrade slot of the current oldest upgrade.
    /// </summary>
    /// <param name="selectionIdx">Index of the new upgrade in the upgrade selection</param>
    public static void BindWeaponUpgrade(int selectionIdx)
    {
        WeaponUpgrade newUpgrade = _currentWeaponUpgradeSelection[selectionIdx];

        // Replace upgrade
        WeaponUpgrades.Add(newUpgrade);

        // Remove new upgrade from upgrade pool
        WeaponUpgradePool.Remove(newUpgrade);
    }

    public static void BindAbilityUpgrade(int selectionIdx)
    {
        AbilityUpgrade newUpgrade = _currentAbilityUpgradeSelection[selectionIdx];

        // Replace upgrade
        AbilityUpgrades.Add(newUpgrade);

        // Remove new upgrade from upgrade pool
        AbilityUpgradePool.Remove(newUpgrade);
    }

    /// <summary>
    /// Returns the bound upgrade at the passed index.
    /// </summary>
    /// <param name="index">Upgrade index</param>
    /// <returns>Upgrade at index</returns>
    public static Upgrade GetWeaponUpgradeAtIndex(int index)
    {
        if (index < WeaponUpgrades.Count)
        {
            return WeaponUpgrades[index];
        }

        return null;
    }

    public static Upgrade GetAbilityUpgradeAtIndex(int index)
    {
        if (index < AbilityUpgrades.Count)
        {
            return AbilityUpgrades[index];
        }

        return null;
    }

    /// <summary>
    /// Clears all applied upgrades
    /// </summary>
    public static void ClearUpgrades()
    {
        WeaponUpgrades.Clear();
        AbilityUpgrades.Clear();
    }

    /// <summary>
    /// Calculates the multiplier from the passed values. It is ensured that no negative multipliers occur.
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

        float multiplier = 1f + WeaponUpgrades.Sum(upgrade => attributeSelector(upgrade)) + AbilityUpgrades.Sum(upgrade => attributeSelector(upgrade));

        return Mathf.Max(0, multiplier);
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
    /// Calculates the bullet speed multiplier of all upgrades.
    /// </summary>
    /// <returns>Common bullet speed multiplier</returns>
    public static float GetBulletSpeedMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BulletSpeed);
    }

    /// <summary>
    /// Calculates the bullet count adjustment of all upgrades.
    /// </summary>
    /// <returns>Common bullet count adjustment</returns>
    public static int GetBulletCountAdjustment()
    {
        int bulletCountAdjustment = 0;

        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
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
    /// Calculates the bullet size multiplier of all upgrades.
    /// </summary>
    /// <returns>Common bullet size multiplier</returns>
    public static float GetBulletSizeMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BulletSize);
    }

    /// <summary>
    /// Calculates the fire delay multiplier of all upgrades.
    /// </summary>
    /// <returns>Common fire delay multiplier</returns>
    public static float GetFireDelayMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.FireDelay);
    }

    /// <summary>
    /// Calculates the block delay multiplier of all upgrades.
    /// </summary>
    /// <returns>Common block delay multiplier</returns>
    public static float GetBlockDelayMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.BlockDelay);
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
    /// Calculates the movement speed multiplier of all upgrades.
    /// </summary>
    /// <returns>Common movement speed multiplier</returns>
    public static float GetPlayerMovementSpeedMultiplier()
    {
        return GetAttributeMultiplier(upgrade => upgrade.PlayerMovementSpeed);
    }

    /// <summary>
    /// Executes the player initialization functions of all assigned upgrades
    /// </summary>
    /// <param name="upgradeablePlayer">Player reference</param>
    public static void Init(IUpgradeablePlayer upgradeablePlayer)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.Init(upgradeablePlayer);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.Init(upgradeablePlayer);
        }
    }

    /// <summary>
    /// Executes the bullet initialization functions of all assigned upgrades
    /// </summary>
    /// <param name="upgradeableBullet">Bullet reference</param>
    public static void Init(IUpgradeableBullet upgradeableBullet)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.Init(upgradeableBullet);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.Init(upgradeableBullet);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player fires
    /// </summary>
    /// <param name="upgradeablePlayer">Player reference</param>
    public static void OnFire(IUpgradeablePlayer upgradeablePlayer)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.OnFire(upgradeablePlayer);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.OnFire(upgradeablePlayer);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player blocks
    /// </summary>
    /// <param name="upgradeablePlayer">Player reference</param>
    public static void OnBlock(IUpgradeablePlayer upgradeablePlayer)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.OnBlock(upgradeablePlayer);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.OnBlock(upgradeablePlayer);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades every frame while the bullet is flying
    /// </summary>
    /// <param name="upgradeableBullet">Bullet reference</param>
    public static void BulletUpdate(IUpgradeableBullet upgradeableBullet)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.BulletUpdate(upgradeableBullet);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.BulletUpdate(upgradeableBullet);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades for the player every frame 
    /// </summary>
    /// <param name="upgradeablePlayer">Player reference</param>
    public static void PlayerUpdate(IUpgradeablePlayer upgradeablePlayer)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.PlayerUpdate(upgradeablePlayer);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.PlayerUpdate(upgradeablePlayer);
        }
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the bullet hits something
    /// </summary>
    /// <param name="upgradeableBullet">Bullet reference</param>
    /// <param name="collision">Collision information</param>
    /// <returns>Bool, whether the bullet should survive afterwards</returns>
    public static bool OnBulletImpact(IUpgradeableBullet upgradeableBullet, Collision2D collision)
    {
        // binary unconditional logical OR ('|' not '||') needed to evaluate every operand (no short-circuiting)
        bool bulletSurvives = false;

        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            bulletSurvives |= upgrade.OnBulletImpact(upgradeableBullet, collision);
        }
        
        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            bulletSurvives |= upgrade.OnBulletImpact(upgradeableBullet, collision);
        }

        return bulletSurvives;
    }

    /// <summary>
    /// Executes the functionalities of all assigned upgrades when the player dies
    /// </summary>
    /// <param name="upgradeablePlayer">Player reference</param>
    public static void OnPlayerDeath(IUpgradeablePlayer upgradeablePlayer)
    {
        foreach (WeaponUpgrade upgrade in WeaponUpgrades)
        {
            upgrade.OnPlayerDeath(upgradeablePlayer);
        }

        foreach (AbilityUpgrade upgrade in AbilityUpgrades)
        {
            upgrade.OnPlayerDeath(upgradeablePlayer);
        }
    }
}