using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;
    
    [Header("Shooting")]
    public float damage = 10f;
    public float fireRate = 0.1f;
    public float range = 100f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    
    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private float nextFireTime = 0f;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Gravity & Jump
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        transform.Rotate(Vector3.up * mouseX);
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            Vector3 rot = cam.transform.localEulerAngles;
            rot.x -= mouseY;
            cam.transform.localEulerAngles = rot;
        }

        // Shooting
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);
            
            BotAI bot = hit.transform.GetComponent<BotAI>();
            if (bot != null)
            {
                bot.TakeDamage(damage);
            }
            
            // Spawn cookie on hit (chance 20%)
            if (Random.value < 0.2f)
            {
                CookieSpawner.SpawnCookie(hit.point);
            }
        }
    }
}
