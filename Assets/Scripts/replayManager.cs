using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }

    private List<float> killTimeStamps = new List<float>();
    private List<ReplayEvent> events = new List<ReplayEvent>();
    private int currentEventIndex = 0;

    private bool isReplaying = false;
    private float replayTimer = 0f;
    public AnimationCurve speed;


    [SerializeField] private Player localPlayer;
    [SerializeField] private Invaders invaders;
    public Projectile laserPrefab;

    public Projectile missilePrefab;

    private Vector3 invadersDirection;
    private float lastInvadersEventTime = 0f;
    private int replayAmountKilled = 0;

    private float actualSpeed;

    private float gameStartTime;
    private Vector3 _direction = Vector2.right;


    [System.Serializable]
    private class ReplayEvent
    {
        public float timestamp;
        public string eventType;
        public object data;
    }

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
    }

    void Start()
    {
        gameStartTime = Time.time;
        FindReferences();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isReplaying)
        {
            FindReferences();
            Debug.Log("Reassigned game object references after scene load.");
        }
    }

    private void FindReferences()
    {
        localPlayer = FindObjectOfType<Player>();
        invaders = FindObjectOfType<Invaders>();
        if (localPlayer == null) Debug.LogError("No Player found!");
        if (invaders == null) Debug.LogError("No Invaders found!");
    }

    public void LogEvent(string eventType, object data)
    {
        if (isReplaying) return;

        float timestamp = Time.time - gameStartTime;
        Debug.Log($"Event received: {eventType}, Data: {data}, Time: {timestamp}");
        events.Add(new ReplayEvent { timestamp = timestamp, eventType = eventType, data = data });
        if (eventType == "InvaderKilled") killTimeStamps.Add(timestamp);
    }

    public void StartReplay()
    {
        isReplaying = true;
        replayTimer = 0f;
        currentEventIndex = 0;
        invadersDirection = Vector3.right; // Initial direction
        lastInvadersEventTime = 0f;
        replayAmountKilled = 0;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (!isReplaying) return;

        replayTimer += Time.deltaTime;

        while (currentEventIndex < events.Count && 
               events[currentEventIndex].timestamp <= replayTimer)
        {
            ApplyEvent(events[currentEventIndex]);
            currentEventIndex++;
        }

        SimulateContinuousMovement(Time.deltaTime);

        if (currentEventIndex >= events.Count)
        {
            isReplaying = false;
            Debug.Log("Replay completed.");
        }
    }

    private void SimulateContinuousMovement(float deltaTime)
{
    // Check if invaders exist
    if (invaders == null || invaders.transform.childCount == 0) return;
        Debug.Log("amount dead in replay"+ replayAmountKilled);
        actualSpeed = this.speed.Evaluate((float)replayAmountKilled/55.0f);
        invaders.transform.position += _direction * actualSpeed * Time.deltaTime; //moves the block of invaders to the right initially
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        foreach(Transform invader in invaders.transform)
        {
            if(!invader.gameObject.activeInHierarchy) { //this is to check against any dectivated(dead) invaders
                continue;
            }

            if(_direction == Vector3.right && invader.position.x >= (rightEdge.x - 1.0f))
            {
                AdvanceRow();
            } else if (_direction == Vector3.left && invader.position.x <= (leftEdge.x +  1.0f)) //1.0f is so invaders dont clip off edge of screen
            {
                AdvanceRow();
            }
        }
}
 private void AdvanceRow()
{
    _direction.x *= -1f;
    Vector3 position = invaders.transform.position;
    position.y -= 1f;
    invaders.transform.position = position;
}
 private void ApplyEvent(ReplayEvent e)
    {
        if (localPlayer == null || invaders == null) return;

        switch (e.eventType)
        {
            case "LocalPosition":
                var moveData = (dynamic)e.data;
                Vector3 newPosition = localPlayer.transform.position;
                newPosition.x = moveData.x;
                localPlayer.transform.position = newPosition;
                break;

            case "InvaderKilled":
                var killData = (dynamic)e.data;
                int index = killData.index;
                if (index >= 0 && index < invaders.invaderObjects.Length)
                {
                    invaders.invaderObjects[index].gameObject.SetActive(false);
                    replayAmountKilled++;
                }
                break;

           case "InvadersAdvanced":
              // var eventData = (dynamic)e.data;
                //invaders.transform.position = new Vector3(eventData.position.x, eventData.position.y, 0);
                //invadersDirection = new Vector3(eventData.direction.x, eventData.direction.y, 0);
                Debug.Log($"Applied InvadersAdvanced: Position={invaders.transform.position}, Direction={invadersDirection}");
                break;

            case "LocalShoot":
                var shootData = (dynamic)e.data;
                Vector3 shootPosition = localPlayer.transform.position;
                shootPosition.x = shootData.x;
                Instantiate(laserPrefab, shootPosition, Quaternion.identity);
                break;
            case "MissileSpawn":
                var missileData = (dynamic)e.data;
                Vector3 missilePos = missileData.position;
                Instantiate(missilePrefab, missilePos, Quaternion.identity);
                break;

            default:
                Debug.LogWarning($"Unhandled event type: {e.eventType}");
                break;
        }
    }

    public bool IsReplaying()
    {
        return isReplaying;
    }
}