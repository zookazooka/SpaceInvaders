using UnityEngine;
using System.Diagnostics;
using System.Threading;

public class JTAGUARTReader : MonoBehaviour
{
    private Process process;
    private string latestCommand = "";
    private bool isRunning = true;

    void Start()
    {
        // Configure the process to run nios2-terminal via the command shell
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
            if (line != null && (line == "L" || line == "R" || line == "S"))
            {
                lock (this) // Thread-safe access to latestCommand
                {
                    latestCommand = line;
                }
            }
            Thread.Sleep(1); // Small sleep to reduce CPU usage
        }
    }

    void Update()
    {
        string command;
        lock (this) // Ensure thread safety
        {
            command = latestCommand;
            latestCommand = ""; // Clear after reading
        }

        if (!string.IsNullOrEmpty(command))
        {
            switch (command)
            {
                case "L":
                    transform.Translate(Vector3.left * Time.deltaTime * 5f);
                    UnityEngine.Debug.Log("Left");
                    break;
                case "R":
                    transform.Translate(Vector3.right * Time.deltaTime * 5f);
                    UnityEngine.Debug.Log("Right");
                    break;
                case "S":
                    UnityEngine.Debug.Log("Stationary");
                    break;
            }
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