using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BattlePassTier
{
    public int tierNumber;
    public int xpRequired;
    public string rewardType; // "skin", "weapon", "cookie"
    public string rewardName;
    public int rewardAmount;
    public bool isClaimed;
}

public class BattlePass : MonoBehaviour
{
    public List<BattlePassTier> tiers = new List<BattlePassTier>();
    public int currentTier = 1;
    
    void Start()
    {
        InitializeBattlePass();
        CheckProgression();
    }

    void InitializeBattlePass()
    {
        if (tiers.Count == 0)
        {
            // Default Battle Pass Setup
            tiers.Add(CreateTier(1, 0, "cookie", "", 100));
            tiers.Add(CreateTier(2, 200, "skin", "Commando", 0));
            tiers.Add(CreateTier(3, 500, "cookie", "", 250));
            tiers.Add(CreateTier(4, 1000, "weapon", "Rifle", 0));
            tiers.Add(CreateTier(5, 2000, "skin", "Ninja", 0));
            tiers.Add(CreateTier(6, 3500, "cookie", "", 500));
            tiers.Add(CreateTier(7, 5000, "weapon", "Sniper", 0));
            tiers.Add(CreateTier(8, 7500, "skin", "Gold", 0));
            tiers.Add(CreateTier(9, 10000, "cookie", "", 1000));
            tiers.Add(CreateTier(10, 15000, "skin", "Legend", 0));
        }
        
        LoadProgress();
    }

    BattlePassTier CreateTier(int tier, int xp, string type, string name, int amount)
    {
        return new BattlePassTier 
        { 
            tierNumber = tier, 
            xpRequired = xp, 
            rewardType = type, 
            rewardName = name, 
            rewardAmount = amount,
            isClaimed = false
        };
    }

    void CheckProgression()
    {
        if (PlayerProgression.Instance == null) return;
        
        PlayerData data = PlayerProgression.Instance.GetData();
        
        foreach (var tier in tiers)
        {
            if (data.xp >= tier.xpRequired && !tier.isClaimed)
            {
                currentTier = tier.tierNumber;
                UnlockReward(tier);
            }
        }
        
        SaveProgress();
    }

    void UnlockReward(BattlePassTier tier)
    {
        Debug.Log("Unlocked Tier " + tier.tierNumber + ": " + tier.rewardType + " - " + tier.rewardName);
        
        if (tier.rewardType == "skin" && !string.IsNullOrEmpty(tier.rewardName))
        {
            PlayerProgression.Instance.UnlockSkin(tier.rewardName);
        }
        else if (tier.rewardType == "weapon" && !string.IsNullOrEmpty(tier.rewardName))
        {
            PlayerProgression.Instance.UnlockWeapon(tier.rewardName);
        }
        else if (tier.rewardType == "cookie")
        {
            PlayerProgression.Instance.AddCookies(tier.rewardAmount);
        }
        
        tier.isClaimed = true;
    }

    public void ClaimReward(int tierNumber)
    {
        var tier = tiers.Find(t => t.tierNumber == tierNumber);
        if (tier != null && tier.isClaimed)
        {
            Debug.Log("Claimed reward for tier " + tierNumber);
            // UI feedback here
        }
    }

    void SaveProgress()
    {
        // Serialize battle pass state to PlayerPrefs
        string json = JsonUtility.ToJson(new BattlePassSave { tiers = tiers });
        PlayerPrefs.SetString("BattlePass_Save", json);
        PlayerPrefs.Save();
    }

    void LoadProgress()
    {
        if (PlayerPrefs.HasKey("BattlePass_Save"))
        {
            string json = PlayerPrefs.GetString("BattlePass_Save");
            BattlePassSave save = JsonUtility.FromJson<BattlePassSave>(json);
            if (save != null && save.tiers != null)
            {
                tiers = save.tiers;
            }
        }
    }
}

[System.Serializable]
public class BattlePassSave
{
    public List<BattlePassTier> tiers;
}
