using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattlePassUI : MonoBehaviour
{
    public Transform tiersContainer;
    public GameObject tierPrefab;
    
    void OnEnable()
    {
        PopulateBattlePass();
    }

    void PopulateBattlePass()
    {
        // Clear existing
        foreach (Transform child in tiersContainer)
            Destroy(child.gameObject);
        
        BattlePass battlePass = FindObjectOfType<BattlePass>();
        if (battlePass == null) return;
        
        foreach (var tier in battlePass.tiers)
        {
            GameObject slot = Instantiate(tierPrefab, tiersContainer);
            
            TextMeshProUGUI tierText = slot.transform.Find("TierNumber")?.GetComponent<TextMeshProUGUI>();
            if (tierText != null)
                tierText.text = "Tier " + tier.tierNumber.ToString();
            
            TextMeshProUGUI rewardText = slot.transform.Find("Reward")?.GetComponent<TextMeshProUGUI>();
            if (rewardText != null)
            {
                string rewardDisplay = tier.rewardType == "cookie" 
                    ? tier.rewardAmount + " 🍪" 
                    : tier.rewardName + " (" + tier.rewardType + ")";
                rewardText.text = rewardDisplay;
            }
            
            TextMeshProUGUI xpText = slot.transform.Find("XPRequired")?.GetComponent<TextMeshProUGUI>();
            if (xpText != null)
                xpText.text = tier.xpRequired + " XP";
            
            Image img = slot.GetComponent<Image>();
            if (img != null)
            {
                if (tier.isClaimed)
                    img.color = Color.green;
                else
                    img.color = Color.gray;
            }
        }
    }
}
