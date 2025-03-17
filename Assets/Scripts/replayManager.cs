using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    public GameObject[] powerUpPrefabs;

    
    [SerializeField] private Player localPlayer;
    [SerializeField] private Invaders invaders;
    [SerializeField] private remotePlayer remotePlayer;
    [SerializeField] private MysteryShip MysteryShip;
    public Projectile laserPrefab;
    public Projectile missilePrefab;

    private Vector3 invadersDirection;
    private float lastInvadersEventTime = 0f;
    private int replayAmountKilled = 0;

    private float actualSpeed;
    private float gameStartTime;
    private Vector3 _direction = Vector2.right;
    private float startTime;
    private bool stopReplay = false;
    private float lastSimulatedTime = 0f; // Track last simulated timestamp

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
            StartCoroutine(ApplyInitialEventsAfterFrame());
            Debug.Log("Reassigned game object references; initial events will be applied after frame end.");
        }
    }

    private void FindReferences()
    {
        localPlayer = FindObjectOfType<Player>();
        invaders = FindObjectOfType<Invaders>();
        MysteryShip = FindObjectOfType<MysteryShip>();
        remotePlayer = FindObjectOfType<remotePlayer>();
        if (localPlayer == null) Debug.LogError("No Player found!");
        if (invaders == null) Debug.LogError("No Invaders found!");
    }
    private IEnumerator ApplyInitialEventsAfterFrame()
    {
        if (invaders == null || invaders.invaderObjects == null || invaders.invaderObjects.Length == 0)
        {
        Debug.LogError("Invaders not initialized.");
        yield return 0;
        }
        yield return new WaitForEndOfFrame();
        ApplyInitialEvents();
    }

   private void ApplyInitialEvents()
    {

        Debug.Log("invaders check "+ invaders.invaderObjects.Length);
        lastSimulatedTime = 0f; // Reset to game start
        currentEventIndex= 0;
        Debug.Log("checking: "+ events[currentEventIndex].timestamp);
        while (currentEventIndex < events.Count && events[currentEventIndex].timestamp <= startTime)
        {
            ReplayEvent currentEvent = events[currentEventIndex];
            Debug.Log("Pre, kill invader at time: "+currentEvent.timestamp);
            if (currentEvent.eventType == "InvaderKilled"){
            ApplyEvent(currentEvent); // Apply the event (e.g., deactivate killed invaders)
            }
            lastSimulatedTime = currentEvent.timestamp;
            currentEventIndex++;
        }
        Debug.Log($"Applied initial events up to startTime {startTime}; currentEventIndex is now {currentEventIndex} Time is: "+events[currentEventIndex].timestamp);
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
        invadersDirection = Vector3.right;
        lastInvadersEventTime = 0f;
        replayAmountKilled = 0;
        startTime = findHighlight();
        stopReplay = false;
        lastSimulatedTime = 0f;
        
        SceneManager.LoadScene("SpaceInvaders");
    }

    private float findHighlight()
    {
        if (killTimeStamps == null || killTimeStamps.Count == 0)
            return -1;

        if (killTimeStamps.Count == 1)
            return killTimeStamps[0];

        killTimeStamps.Sort();

        int maxKills = 0;
        float bestStartTime = 0f;
        float windowSize = 5f;

        for (int i = 0; i < killTimeStamps.Count; i++)
        {
            float windowStart = killTimeStamps[i];
            float windowEnd = windowStart + windowSize;

            int killsInWindow = 0;
            for (int j = i; j < killTimeStamps.Count && killTimeStamps[j] <= windowEnd; j++)
            {
                killsInWindow++;
            }

            if (killsInWindow > maxKills)
            {
                maxKills = killsInWindow;
                bestStartTime = windowStart;
            }
        }

        return bestStartTime;
    }

    void Update()
    {
        if (!isReplaying) return;

        replayTimer += Time.deltaTime;

        if (replayTimer >= 5f)
        {
            isReplaying = false;
            Debug.Log("Replay terminated after 5 seconds.");
            SceneManager.LoadScene("endGame");
            return;
        }

        while (currentEventIndex < events.Count && 
               events[currentEventIndex].timestamp <= startTime + replayTimer)
        {
            ApplyEvent(events[currentEventIndex]);
            currentEventIndex++;

            if (stopReplay)
            {
                isReplaying = false;
                Debug.Log("Replay terminated due to EndGame event.");
                return;
            }
        }

        //SimulateContinuousMovement(Time.deltaTime);

        if (currentEventIndex >= events.Count)
        {
            isReplaying = false;
            Debug.Log("Replay completed (all events processed).");
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
            case "RemotePosition":
                var RmoveData = (dynamic)e.data;
                Vector3 RnewPosition = remotePlayer.transform.position;
                newPosition.x = RmoveData.x;
                remotePlayer.transform.position = RnewPosition;
                break;
            case "MystPosition":
                var MystmoveData = (dynamic)e.data;
                Vector3 MystnewPosition = MysteryShip.transform.position;
                MystnewPosition.x = MystmoveData.x; // Corrected variable name
                MysteryShip.transform.position = MystnewPosition;
                break;
            case "InvPosition":
                var InvMoveData = (dynamic)e.data;
                invaders.transform.position = InvMoveData.position;
                break;
            case "InvaderKilled":
                Debug.Log("invader killed at time: "+e.timestamp);
                var killData = (dynamic)e.data;
                int index = killData.index;
                if (index >= 0 && index < invaders.invaderObjects.Length)
                {
                    invaders.invaderObjects[index].gameObject.SetActive(false);
                    replayAmountKilled++;
                }
                break;
            case "PowerSpawned": 
                var powerData = (dynamic)e.data;
                Instantiate(powerUpPrefabs[powerData.type], powerData.position, Quaternion.identity);
                break;

            case "InvadersAdvanced":
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

            case "EndGame":
                stopReplay = true;
                Debug.Log("EndGame event encountered.");
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