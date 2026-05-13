using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int cookies;
    public int level;
    public int xp;
    public List<string> unlockedSkins;
    public string equippedSkin;
    public List<string> unlockedWeapons;
    public string equippedWeapon;
    
    public PlayerData()
    {
        cookies = 0;
        level = 1;
        xp = 0;
        unlockedSkins = new List<string> { "Default" };
        equippedSkin = "Default";
        unlockedWeapons = new List<string> { "Pistol" };
        equippedWeapon = "Pistol";
    }
}

public class PlayerProgression : MonoBehaviour
{
    public static PlayerProgression Instance;
    
    [Header("Current Session")]
    public int sessionCookies = 0;
    
    private PlayerData data;
    private const string SaveKey = "PrivatkaBrawl_Save";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        LoadData();
    }

    public void AddCookies(int amount)
    {
        sessionCookies += amount;
        data.cookies += amount;
        AddXp(amount);
        SaveData();
    }

    public void AddXp(int amount)
    {
        data.xp += amount;
        int xpNeeded = data.level * 100;
        
        if (data.xp >= xpNeeded)
        {
            data.level++;
            data.xp -= xpNeeded;
            Debug.Log("Level Up! Now level " + data.level);
        }
        SaveData();
    }

    public void UnlockSkin(string skinName)
    {
        if (!data.unlockedSkins.Contains(skinName))
        {
            data.unlockedSkins.Add(skinName);
            SaveData();
        }
    }

    public void EquipSkin(string skinName)
    {
        if (data.unlockedSkins.Contains(skinName))
        {
            data.equippedSkin = skinName;
            SaveData();
        }
    }

    public void UnlockWeapon(string weaponName)
    {
        if (!data.unlockedWeapons.Contains(weaponName))
        {
            data.unlockedWeapons.Add(weaponName);
            SaveData();
        }
    }

    public void EquipWeapon(string weaponName)
    {
        if (data.unlockedWeapons.Contains(weaponName))
        {
            data.equippedWeapon = weaponName;
            SaveData();
        }
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            data = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            data = new PlayerData();
            SaveData();
        }
    }

    public PlayerData GetData()
    {
        return data;
    }
}
