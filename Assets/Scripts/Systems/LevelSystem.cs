using UnityEngine;
using PlayerLevel = LevelSystem;

public class LevelSystem : MonoBehaviour
{
    public int level = 1;
    public int xp = 0;
    public int xpToNext = 5;

    public System.Action<int> OnLevelUp;

    public void AddXp(int amount)
    {
        xp += amount;
        while (xp >= xpToNext)
        {
            xp -= xpToNext;
            level++;
            xpToNext = Mathf.RoundToInt(xpToNext * 1.5f + 1);
        }
    }
    
    public void TriggerLevelUpReward()
    {
        OnLevelUp?.Invoke(level);
    }
}