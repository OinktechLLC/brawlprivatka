using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;
    public GameObject botPrefab;
    public int botCount = 5;
    
    void Start()
    {
        SpawnPlayer();
        SpawnBots();
    }

    void SpawnPlayer()
    {
        if (spawnPoints.Length > 0)
        {
            Instantiate(playerPrefab, spawnPoints[0].position, Quaternion.identity);
        }
        else
        {
            Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    void SpawnBots()
    {
        for (int i = 0; i < botCount; i++)
        {
            Vector3 spawnPos = Random.insideUnitSphere * 50f;
            spawnPos.y = 1f;
            
            if (spawnPoints.Length > 1)
            {
                spawnPos = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
            }
            
            Instantiate(botPrefab, spawnPos, Quaternion.identity);
        }
    }
}
