using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Threading;
using System.Collections;

public class Player : MonoBehaviour
{
    private Process process;
    private string latestCommand = "";
    private bool isRunning = true;
    public Projectile laserPrefab; //setup prefab for laser of type pprojectile
    public float speed = 5.0f;

    private bool _laserActive;
    private bool canShootTriple = false;

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
            UnityEngine.Debug.Log(command);
            float lastTime = Time.time;
            switch (command[0])
            {
                case 'L':
                    this.transform.position += Vector3.left * this.speed * Time.deltaTime;
                    break;
                case 'R':
                    this.transform.position += Vector3.right * this.speed * Time.deltaTime;

                    break;
                case 'S':
                    break;
            }
            switch (command[2])
            {
                case 'Y':
                Shoot();
                break;
                case 'N':
                break;
            }
        }
        
        
    }
    private void Shoot()
    {
        if (!_laserActive)
        {
            Projectile projectile = Instantiate(this.laserPrefab, this.transform.position, Quaternion.identity);
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
