using UnityEngine;

public class Cookie : MonoBehaviour
{
    public int value = 10;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerProgression progression = other.GetComponent<PlayerProgression>();
            if (progression != null)
            {
                progression.AddCookies(value);
            }
            
            Destroy(gameObject);
        }
    }
}

public static class CookieSpawner
{
    public static GameObject cookiePrefab;
    
    public static void SpawnCookie(Vector3 position)
    {
        if (cookiePrefab == null)
        {
            // Create simple cookie if prefab not assigned
            GameObject cookieObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cookieObj.name = "Cookie";
            cookieObj.AddComponent<Cookie>();
            cookieObj.GetComponent<SphereCollider>().isTrigger = true;
            cookieObj.GetComponent<Renderer>().material.color = Color.yellow;
            cookieObj.transform.localScale = Vector3.one * 0.5f;
            cookiePrefab = cookieObj;
        }
        
        GameObject cookie = GameObject.Instantiate(cookiePrefab, position, Quaternion.identity);
        GameObject.Destroy(cookie, 10f); // Despawn after 10 seconds
    }
}
