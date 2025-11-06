using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerXP : MonoBehaviour
{
    [Header("XP Settings")]
    public int level = 1;
    public float xp = 0f;
    public float xpToNext = 5f;
    
    [Header("Level Scaling")]
    public float baseXPRequired = 5f;
    public float xpMultiplier = 1.5f;
    
    [Header("Events")]
    public Action<int> OnLevelUp;
    public Action<float> OnXPGained;
    public Action<float> OnXPChanged;
    
    void Start()
    {
        CalculateNextLevelXP();
    }
    
    public void AddXP(float amount)
    {
        xp += amount;
        OnXPGained?.Invoke(amount);
        OnXPChanged?.Invoke(xp);
        
        // Check for level up
        while (xp >= xpToNext)
        {
            LevelUp();
        }
    }
    
    void LevelUp()
    {
        xp -= xpToNext;
        level++;
        
        CalculateNextLevelXP();
        
        OnLevelUp?.Invoke(level);
        
        // Notify GameManager for upgrade selection
        if (GameManager.Instance != null)
        {
            // GameManager will handle upgrade UI
        }
        
        // Visual/audio feedback
        StartCoroutine(LevelUpEffect());
    }
    
    void CalculateNextLevelXP()
    {
        xpToNext = baseXPRequired * Mathf.Pow(xpMultiplier, level - 1);
    }
    
    IEnumerator LevelUpEffect()
    {
        // Simple visual effect
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            
            // Flash yellow
            for (int i = 0; i < 3; i++)
            {
                sr.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                sr.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    public float GetXPPercent()
    {
        return xp / xpToNext;
    }
    
    public void ResetXP()
    {
        level = 1;
        xp = 0f;
        CalculateNextLevelXP();
        
        OnXPChanged?.Invoke(xp);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Collect XP orbs
        if (other.CompareTag("XPOrb"))
        {
            XPOrb orb = other.GetComponent<XPOrb>();
            if (orb != null)
            {
                AddXP(orb.xpValue);
            }
            
            Destroy(other.gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Alternative collision detection for XP orbs
        if (collision.gameObject.CompareTag("XPOrb"))
        {
            XPOrb orb = collision.gameObject.GetComponent<XPOrb>();
            if (orb != null)
            {
                AddXP(orb.xpValue);
            }
            
            Destroy(collision.gameObject);
        }
    }
}