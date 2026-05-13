using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Skin Slots")]
    public Transform skinContainer;
    public GameObject skinSlotPrefab;
    
    [Header("Weapon Slots")]
    public Transform weaponContainer;
    public GameObject weaponSlotPrefab;
    
    void OnEnable()
    {
        PopulateInventory();
    }

    void PopulateInventory()
    {
        if (PlayerProgression.Instance == null) return;
        
        PlayerData data = PlayerProgression.Instance.GetData();
        
        // Clear existing
        foreach (Transform child in skinContainer)
            Destroy(child.gameObject);
        foreach (Transform child in weaponContainer)
            Destroy(child.gameObject);
        
        // Populate Skins
        foreach (string skin in data.unlockedSkins)
        {
            GameObject slot = Instantiate(skinSlotPrefab, skinContainer);
            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = skin;
            
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                string skinName = skin;
                btn.onClick.AddListener(() => EquipSkin(skinName));
            }
            
            // Highlight equipped
            if (skin == data.equippedSkin)
            {
                Image img = slot.GetComponent<Image>();
                if (img != null)
                    img.color = Color.green;
            }
        }
        
        // Populate Weapons
        foreach (string weapon in data.unlockedWeapons)
        {
            GameObject slot = Instantiate(weaponSlotPrefab, weaponContainer);
            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = weapon;
            
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                string weaponName = weapon;
                btn.onClick.AddListener(() => EquipWeapon(weaponName));
            }
            
            // Highlight equipped
            if (weapon == data.equippedWeapon)
            {
                Image img = slot.GetComponent<Image>();
                if (img != null)
                    img.color = Color.green;
            }
        }
    }

    void EquipSkin(string skinName)
    {
        PlayerProgression.Instance.EquipSkin(skinName);
        PopulateInventory();
    }

    void EquipWeapon(string weaponName)
    {
        PlayerProgression.Instance.EquipWeapon(weaponName);
        PopulateInventory();
    }
}
