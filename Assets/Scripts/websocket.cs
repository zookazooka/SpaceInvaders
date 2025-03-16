using NativeWebSocket;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
//using System.Diagnostics;
using System.Threading;
using System;
using System.Threading.Tasks;

public class WebSocketClient : MonoBehaviour
{
    private WebSocket websocket;
    [SerializeField] private Player player;
    [SerializeField] private remotePlayer rplayer;
    [SerializeField] private Invaders invaders;
    [SerializeField] private MysteryShip mysteryship;

    string latestMessage;

    public static bool serverFull = false;

    async void Start()
    {
        websocket = new WebSocket("ws://18.175.247.141:3000");
        //websocket = new WebSocket("ws://localhost:3000");
        websocket.OnOpen += () => 
        {
            Debug.Log("Connected to server");

            
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("ERROR: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received: " + message);
            //handle remote player function here (message);
        
            //string messageType;
            if (message.Substring(0, 3) == "MOV") { //unused
                //handle movement
                //rplayer.Move(message.Substring(3));
            }
            else if(message.Substring(0, 3) == "IND") { //index of invader killed
                //handle index
                invaders.KillInvader(int.Parse(message.Substring(3)));
            }
            else if (message.Substring(0, 3) == "POS") {  //update position of remote player
                string position = message.Substring(3);
                ReplayManager.Instance.LogEvent("RemotePosition", new { x = float.Parse(position) });
                rplayer.Move(float.Parse(position));
            }
            else if (message.Substring(0, 3) == "MOS") { //index of invader firing missile
                string position = message.Substring(3);
                invaders.RemoteMissileAttack(position);
            }
            else if (message == "FULL") {
                Debug.Log("FOUND PLAYER");
                serverFull = true;
            }
            else if (message == "Laser") {
                Debug.Log("LASER");
                rplayer.Shoot();
            }
            else if (message.Substring(0, 3) == "POW") {
                string index = message.Substring(3);
                mysteryship.remotePowerUp(int.Parse(index));
            }

        };

        websocket.OnClose += (e) => {
            Debug.Log("Closed websocket server with: " + e);
        };

        await websocket.Connect();

    }

    /*
    async void handleMessage(string message, string type)
    {
        //if statements to interpret message e.g. if (message == left) {}
        if (type == "movement") {
            //handle movement in remote player
            rplayer.Move(message.Substring(3));
        }

        else if (type == "index") {
            invaders.KillInvader(int.Parse(message.Substring(3)));
        }

    }
     */
    
    async Task sendInput(string data, string type)
    {
        //send data

        

        if (websocket.State == WebSocketState.Open)
        {   
            string message = "";
            if (type == "index") {
                message = "IND" + data;
            }
            else if (type == "movement") {
                message = "MOV" + data;
            }
            else if (type == "position") {
                message = "POS" + data;
            }
            else if (type == "mposition"){
                message = "MOS" + data;
            }
            else if (type == "powerup") {
                message = "POW" + data;
            }
            await websocket.Send(System.Text.Encoding.UTF8.GetBytes(message));
        }
    }
    public async Task sendPosition(string position) {
        await sendInput(position, "position");
    }
    
    
    public async Task sendIndex(string index) {
        await sendInput(index, "index");
    }
    public async Task sendLaser() {
        await websocket.Send(System.Text.Encoding.UTF8.GetBytes("Laser"));
    }

    public async Task sendMovement(string movement) {
        try {
            Debug.Log("SENDING: " + movement);
            await sendInput(movement, "movement");
        }
        catch (Exception e) {
            Debug.LogError("ERROR " + e.ToString());
        }
    }

    public async Task sendMissilePosition(string position) {
        Debug.Log("SENDING: " + position);
        await sendInput(position, "mposition");
    }

    public async Task sendPowerUp(int index) {
        Debug.Log("Sending: " + index);
        await sendInput(index.ToString(), "powerup");
    }


    async void OnApplicationQuit()
    {
        if (websocket != null) {
            await websocket.Close();
        }
    }


    void Update()
    {
        #if !UNITY_WEBGL
        if (websocket != null)
        {
            // Process all queued messages (OnMessage will update latestMessage)
            websocket.DispatchMessageQueue();
        }
        #endif

    // Process only the latest received message, discarding any earlier ones
    


    }
}