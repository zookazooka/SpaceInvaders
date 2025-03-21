using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;

public class Player : MonoBehaviour
{
    private Process process;
    private string latestCommand = "";
    private bool isRunning = true;
    public Projectile laserPrefab; //setup prefab for laser of type pprojectile
    public float speed = 5.0f;
    public int playerScore;
    private bool _laserActive;
    private bool canShootTriple = false;
    private bool gameStarted = false;
    [SerializeField] private WebSocketClient websocket;

    private float inputTime;
    private float responseTime;

  //  [SerializeField] private ReplayManager ReplayManager;

    async void Start() {
        playerScore = 0;
        
        process = new Process();
        process.StartInfo.FileName = "cmd.exe"; // Use cmd.exe to run the batch file
        process.StartInfo.Arguments = "/C \"C:/intelFPGA_lite/18.1/nios2eds/Nios II Command Shell.bat\" nios2-terminal";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true; // For debugging
        process.StartInfo.CreateNoWindow = true;
        

         try
        {
            process.Start();
            UnityEngine.Debug.Log("Started nios2-terminal subprocess");

            // Start a thread to read output asynchronously
            Thread readThread = new Thread(ReadOutput);
            readThread.Start();
            await waitForPlayers();
            gameStarted = true;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Failed to start nios2-terminal: " + e.Message);
        }
    
    }
    void ReadOutput()
    {
        while (isRunning && !process.HasExited)
        {
            string line = process.StandardOutput.ReadLine()?.Trim();
            
                lock (this) // Thread-safe access to latestCommand
                {
                    latestCommand = line;
                }
            
            Thread.Sleep(1); // Small sleep to reduce CPU usage
        }
    }

    //better way to implement this is have a public function in websocket.cs but i cba rn
    private async Task waitForPlayers() {
        while (WebSocketClient.serverFull == false) {
             UnityEngine.Debug.Log("Waiting for players");
             await Task.Delay(3000);
         }
       
    }

    private async Task SendMovementAsync(string command) {

        UnityEngine.Debug.Log("Sending" + command);
        if (websocket == null) {
            UnityEngine.Debug.Log("NULL");
        }
        await websocket.sendMovement(command);
    }
    private void Update()
    {
        if (ReplayManager.Instance.IsReplaying()) return;
        if (!gameStarted) return;
        string command;
        lock (this) // Ensure thread safety
        {
            command = latestCommand;
            inputTime = Time.time;

            latestCommand = ""; // Clear after reading
        }
        
        if (!string.IsNullOrEmpty(command))
        {
            float lastTime = Time.time;
            switch (command[0])
            {
                case 'L':
                    this.transform.position += Vector3.left * this.speed * Time.deltaTime;
                    UnityEngine.Debug.Log("Latency: " + (Time.time - inputTime));
                    break;
                case 'R':
                    this.transform.position += Vector3.right * this.speed * Time.deltaTime;
                    UnityEngine.Debug.Log("Latency: " + (Time.time - inputTime));

                    break;
                case 'S':
                    UnityEngine.Debug.Log("Latency: " + (Time.time - inputTime));

                    break;
            }
            switch (command[2])
            {
                case 'Y':
                Shoot();
                _=SendLaserAsync();
                break;
                case 'N':
                break;
            }
        //command = string.Concat(command[0], command[2]);
        //_=SendMovementAsync(command);
        _=SendPositionAsync(this.transform.position.x);
        

        }
        ReplayManager.Instance.LogEvent("LocalPosition", new { x = this.transform.position.x });
        
    }
    private async Task SendPositionAsync(float position) {
        if (websocket == null) {
        }
        await websocket.sendPosition(position.ToString());
    }
    private async Task SendLaserAsync() {
        await websocket.sendLaser();
    }

    private void Shoot()
    {
        if (!_laserActive) {
             //when we shoot we instantiate a new prefab, using the players position and rotation is set to 'default' or identity
             Projectile projectile = Instantiate(this.laserPrefab, this.transform.position, Quaternion.identity);
             ReplayManager.Instance.LogEvent("LocalShoot", new { x= this.transform.position.x }); //logs invader deaths.

             projectile.destroyed += LaserDestroyed;
             if (canShootTriple)
            {
                float offsetX = 0.2f;

                // Left laser
                Projectile leftProjectile = Instantiate(this.laserPrefab, this.transform.position + new Vector3(-offsetX, 0, 0), Quaternion.identity);
                leftProjectile.destroyed += LaserDestroyed;

                // Right laser
                Projectile rightProjectile = Instantiate(this.laserPrefab, this.transform.position + new Vector3(offsetX, 0, 0), Quaternion.identity);
                rightProjectile.destroyed += LaserDestroyed;
            }
            _laserActive = true;
        }
      
    }
    private void LaserDestroyed() 
    {
        _laserActive = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invader") || other.gameObject.layer == LayerMask.NameToLayer("Missile")) {
            //game over
            UnityEngine.Debug.Log($"Attempting to load 'endGame'. IsReplaying: {ReplayManager.Instance.IsReplaying()}");
        SceneManager.LoadScene("endGame");
        UnityEngine.Debug.Log("Scene load command issued for 'endGame'.");
        PlayerPrefs.SetInt("playerScore", playerScore);

        }
    }

    public IEnumerator ActivateTripleShot(float duration)
    {
        canShootTriple = true;
        yield return new WaitForSeconds(duration);
        canShootTriple = false;
    }

    public IEnumerator ActivateSpeedBoost(float duration)
    {
        speed *= 2; // Double the speed
        yield return new WaitForSeconds(duration);
        speed /= 2; // Reset speed
    }

    public void IncrementScore() {
        playerScore = playerScore + 10;
        UnityEngine.Debug.Log("Incremented");
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (process != null && !process.HasExited)
        {
            process.Kill();
            process.WaitForExit();
            process.Dispose();
        }
        UnityEngine.Debug.Log("nios2-terminal subprocess terminated");
    }
}
