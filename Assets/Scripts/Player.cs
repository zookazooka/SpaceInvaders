using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class Player : MonoBehaviour
{
    private Process process;
    private string latestCommand = "";
    private bool isRunning = true;
    public Projectile laserPrefab; //setup prefab for laser of type pprojectile
    public float speed = 5.0f;

    [SerializeField] private WebSocketClient websocket;

    private bool _laserActive;

    void Start() {
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

private async Task SendMovementAsync(string command) {

        UnityEngine.Debug.Log("Sending" + command);
        if (websocket == null) {
            UnityEngine.Debug.Log("NULL");
        }
        await websocket.sendMovement(command);
    }
    private void Update()
    {
        string command;
        lock (this) // Ensure thread safety
        {
            command = latestCommand;
            latestCommand = ""; // Clear after reading
        }
        
        if (!string.IsNullOrEmpty(command))
        {
            float lastTime = Time.time;
            switch (command[0])
            {
                case 'L':
                    this.transform.position += Vector3.left *this.speed* Time.deltaTime;
                    break;
                case 'R':
                    this.transform.position += Vector3.right *this.speed * Time.deltaTime;

                    break;
                case 'S':
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
             projectile.destroyed += LaserDestroyed;
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
