using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MysteryShip : MonoBehaviour
{
    [SerializeField] private WebSocketClient websocket;
    [SerializeField] private Invaders invaders; // Reference to Invaders script (assign in Inspector)
    public Projectile missilePrefab;
    public GameObject[] powerUpPrefabs;
    public float powerUpDropChance = 0.02f; // 20% chance to drop a power-up
    public System.Action killed;
    float elapsedTime = 0.0f;
    public static float FinalTime { get; private set; } // Static variable to hold amountKilled

    public bool allKilled = false; // Tracks if invaders are all dead (default false)
    public float missileAttackRate = 2.0f;
    public float speed = 2.0f; // Default speed value
    private Vector3 _direction = Vector2.right;

    private float powerUpDropInterval = 2.0f; // Interval between random power-up drops
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
        
        InvokeRepeating(nameof(MissileAttack), this.missileAttackRate, this.missileAttackRate);
        
        // Start the random power-up drop process
        InvokeRepeating(nameof(DropPowerUp), powerUpDropInterval, powerUpDropInterval);
    }

    private async Task SendPowerUpAsync(int index) {
        if (websocket == null) {
            return;
        }
        await websocket.sendPowerUp(index);
    }

    private async Task waitForPlayers() {
        while (WebSocketClient.serverFull == false) {
            Debug.Log("Waiting for players");
            await Task.Delay(1000);
        }
    }

    public void remotePowerUp(int index) {
        Instantiate(powerUpPrefabs[index], this.transform.position, Quaternion.identity);
        ReplayManager.Instance.LogEvent("PowerSpawn", new {position = this.transform.position, type = index }); //logs invader deaths.


    }

    private void Update()
    {
        //if (!gameStarted) return;
        if (ReplayManager.Instance.IsReplaying()) return;

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
        ReplayManager.Instance.LogEvent("MystPosition", new { x = this.transform.position.x });

    }

    private void DropPowerUp()
    {
        // Attempt to drop a power-up at a random chance
        if (Random.value < powerUpDropChance && powerUpPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[randomIndex], this.transform.position, Quaternion.identity);
            _=SendPowerUpAsync(randomIndex);
            ReplayManager.Instance.LogEvent("PowerSpawn", new {position = this.transform.position, type = randomIndex }); //logs invader deaths.

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
        if (ReplayManager.Instance.IsReplaying()) return;

        if (this.gameObject.activeInHierarchy) // Only spawn missiles if ship is active
        {
            Debug.Log("MissileAttack() called");

            if (Random.value < 0.9f) // 90% chance to spawn missile
            {
                Debug.Log("BANG"); // Check if the function is running

                Instantiate(missilePrefab, this.transform.position, Quaternion.identity);
                ReplayManager.Instance.LogEvent("MissileSpawn", new {position = this.transform.position}); //logs invader deaths.

            }
        }
    }
}
