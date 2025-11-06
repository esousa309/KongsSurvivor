
using UnityEngine;

public enum UpgradeType
{
    MoveSpeed,
    Damage,
    FireRate,
    MaxHealth,
    PickupRange
}

[System.Serializable]
public class Upgrade
{
    public string id;
    public string title;
    public string description;
    public UpgradeType type;
    public float value;

    public void Apply(GameObject player)
    {
        switch (type)
        {
            case UpgradeType.MoveSpeed:
                var pc = player.GetComponent<PlayerController>();
                if (pc != null) pc.moveSpeed *= (1f + value);
                break;
            case UpgradeType.Damage:
                var weap = player.GetComponent<AutoAimWeapon>();
                if (weap != null) weap.damage *= (1f + value);
                break;
            case UpgradeType.FireRate:
                var w2 = player.GetComponent<AutoAimWeapon>();
                if (w2 != null) w2.fireRate *= (1f + value);
                break;
            case UpgradeType.MaxHealth:
                var h = player.GetComponent<Health>();
                if (h != null) { h.maxHealth *= (1f + value); h.current = h.maxHealth; }
                break;
            case UpgradeType.PickupRange:
                var orb = player.GetComponentInChildren<XpOrb>(); // placeholder, could attach magnet
                // Optional: implement a Magnet component on the player
                break;
        }
    }
}
