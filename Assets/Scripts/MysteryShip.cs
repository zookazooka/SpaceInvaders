using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MysteryShip : MonoBehaviour
{
    [SerializeField] private WebSocketClient websocket;
    [SerializeField] private Invaders invaders; // Reference to Invaders script (assign in Inspector)
    public Projectile missilePrefab;
    public System.Action killed;
    float elapsedTime = 0.0f;
    public static float FinalTime { get; private set; } // Static variable to hold amountKilled

    public bool allKilled = false; // Tracks if invaders are all dead (default false)
    public float missileAttackRate = 2.0f;
    public float speed = 2.0f; // Default speed value
    private Vector3 _direction = Vector2.right;

    private bool gameStarted = false;

    private void Awake()
    {
        if (invaders != null)
        {
            invaders.allKilled += MakeKillable; // Subscribe to allKilled event
        }
        else
        {
            Debug.LogError("Invaders reference is not set in MysteryShip!");
        }
    }

    private void OnDestroy()
    {
        if (invaders != null)
        {
            invaders.allKilled -= MakeKillable; // Unsubscribe to avoid memory leaks
        }
    }

    private async void Start()
    {
        elapsedTime = 0f; // Initialize timer at the start
        await waitForPlayers();
        gameStarted = true;
        InvokeRepeating(nameof(MissileAttack), this.missileAttackRate, this.missileAttackRate);
    }

    private async Task waitForPlayers() {
        while (WebSocketClient.serverFull == false) {
            Debug.Log("Waiting for players");
            await Task.Delay(1000);
        }
    }

    private void Update()
    {
        if (!gameStarted) return;
        elapsedTime += Time.deltaTime;
        // Move the ship horizontally, flipping direction at screen edges
        this.transform.position += _direction * speed * Time.deltaTime;
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        if (_direction == Vector3.right && this.transform.position.x >= (rightEdge.x - 1.0f))
        {
            _direction.x *= -1.0f; // Flip to left
        }
        else if (_direction == Vector3.left && this.transform.position.x <= (leftEdge.x + 1.0f))
        {
            _direction.x *= -1.0f; // Flip to right
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (allKilled && other.gameObject.layer == LayerMask.NameToLayer("Laser"))
        {
            FinalTime = elapsedTime;
            this.gameObject.SetActive(false); // Deactivate the ship
            SceneManager.LoadScene("victoryScene");

        }
    }

    private void MakeKillable()
    {
        
        allKilled = true; // Set flag to allow killing
        this.missileAttackRate = 1.0f; //shoots faster
        Debug.Log("MysteryShip is now killable!");
        
    }

    private void MissileAttack()
    {
        if (this.gameObject.activeInHierarchy) // Only spawn missiles if ship is active
        {
            Debug.Log("MissileAttack() called"); // Check if the function is running

            if (Random.value < 0.9f) // 50% chance to spawn missile
            {
                            Debug.Log("BANG"); // Check if the function is running

Instantiate(missilePrefab, this.transform.position , Quaternion.identity);
            }
        }
    }
}