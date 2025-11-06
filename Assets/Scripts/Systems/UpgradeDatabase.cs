
using UnityEngine;

public class UpgradeDatabase : MonoBehaviour
{
    public Upgrade[] upgrades;

    // Minimal helper to get 3 random upgrades on level-up
    public Upgrade[] GetRandomChoices()
    {
        if (upgrades == null || upgrades.Length == 0) return new Upgrade[0];
        Upgrade[] result = new Upgrade[Mathf.Min(3, upgrades.Length)];
        int count = 0;
        int guard = 0;
        while (count < result.Length && guard < 100)
        {
            guard++;
            var pick = upgrades[Random.Range(0, upgrades.Length)];
            bool dup = false;
            for (int i = 0; i < count; i++) if (result[i].id == pick.id) dup = true;
            if (!dup) result[count++] = pick;
        }
        return result;
    }
}
