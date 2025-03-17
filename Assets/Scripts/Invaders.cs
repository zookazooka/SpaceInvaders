using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
public class Invaders : MonoBehaviour
{
    public int rows = 5;

    public int columns = 11; //grid of invaders

    public Projectile missilePrefab;
    public Invader[] prefabs;

    public float missileAttackRate = 1.0f;

    public int[] invaderArray;

    public Invader[] invaderObjects;
    
    public float actualSpeed;

    public WebSocketClient websocket;
    private bool gameStarted = false;

public event System.Action allKilled = delegate { }; // Prevent null reference


    private Vector3 _direction = Vector2.right;
    //animationCurve allows us to determine speed via a graph with time axis percentKilled
    public AnimationCurve speed;

    public int amountKilled {get; private set;}
    public int totalInvaders => this.rows*this.columns;

    public float percentKilled => (float)this.amountKilled / (float)this.totalInvaders;

    //this produces an invader for each row and column we have set, they then have thir positions set in a grid by the following for loops
    private void Awake()
    {   

        invaderArray = new int[rows * columns];
        invaderObjects = new Invader[rows*columns];
        for (int row = 0; row <this.rows; row++){
            //width and height of our grid
            float width = 2.0f * (this.columns-1);
            float height = 2.0f * (this.rows-1);

            //find center point 
            Vector2 centering = new Vector2(-width/2, -height/2);
            Vector3 rowPosition = new Vector3(centering.x , centering.y + row*2.0f ,0.0f);

            for (int col = 0; col<this.columns; col++) {

                int index = row * columns + col;

                Invader invader = Instantiate(this.prefabs[row], this.transform); //this adds to the game
                invader.killed += () => InvaderKilled(index);//rund invaderKilled on action invoked
                Vector3 position = rowPosition;
                position.x += col * 2.0f; //this changes the spacing for each invader

                invader.transform.localPosition = position; //sets the in game values should set local vals so parent object can still effect
                invaderArray[index] = 1;
                invaderObjects[index] = invader;

            }
        }
    }

    private void Update() //update is called every frame the game is running
    {
        if (ReplayManager.Instance.IsReplaying()) return;
        actualSpeed = this.speed.Evaluate(this.percentKilled);
        this.transform.position += _direction * actualSpeed * Time.deltaTime; //moves the block of invaders to the right initially
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        foreach(Transform invader in this.transform)
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
    Vector3 position = transform.position;
    position.y -= 1f;
    transform.position = position;
    ReplayManager.Instance.LogEvent("InvadersAdvanced", new { position = position, direction = _direction });
}
    public void InvaderKilled(int index) {
        if (ReplayManager.Instance.IsReplaying()) return;
        UnityEngine.Debug.Log("RUNS HERE");
        if (invaderArray[index] == 1)
        {
            amountKilled +=1;
            invaderArray[index] = 0;
            
            ReplayManager.Instance.LogEvent("InvaderKilled", new { index }); //logs invader deaths.
            if (this.amountKilled >= totalInvaders){
                //runs when last invader dies
                this.allKilled.Invoke();
            }
            
            _=SendIndexAsync(index.ToString());
            
        }
        
    }

    async private Task SendIndexAsync(string index){
        await websocket.sendIndex(index);
    }

    public void KillInvader(int index) {
        if (invaderArray[index] == 1)
        {
            amountKilled +=1;
            invaderArray[index] = 0;
            invaderObjects[index].gameObject.SetActive(false);
            

            if (this.amountKilled >= totalInvaders){
                //runs when last invader dies
                this.allKilled.Invoke();
            }
            
        }
    }



    

    private async void Start()
    {
        await waitForPlayers();
        gameStarted = true;
        InvokeRepeating(nameof(MissileAttack), this.missileAttackRate, this.missileAttackRate);
    }

    private async Task waitForPlayers() {
        
    }



    private void MissileAttack()
    {
        if (ReplayManager.Instance.IsReplaying()) return;

        foreach(Transform invader in this.transform) 
        {
            if (!invader.gameObject.activeInHierarchy) 
            {
                continue;
            }

            //missile spawns are inversly proportional to number of invaders alive
            if (Random.value < (1.0f / (float)(totalInvaders-amountKilled))) {
                Debug.Log("MISSILE ATTACK");
                
                Instantiate(this.missilePrefab, invader.position, Quaternion.identity);
                ReplayManager.Instance.LogEvent("MissileSpawn", new {position = invader.position}); //logs invader deaths.


                _=SendMPositionAsync(invader.position.ToString());

                break; //means only one missile can spawn per cycle
            }
        }
    }

    async private Task SendMPositionAsync(string position) {
        await websocket.sendMissilePosition(position);
    }

    public void RemoteMissileAttack(string position)
    {
        position = position.Trim('(',')');
        string[] components = position.Split(',');

        float x = float.Parse(components[0]);
        float y = float.Parse(components[1]);
        float z = float.Parse(components[2]);

        Vector3 invaderPos = new Vector3(x, y, z);
        Instantiate(this.missilePrefab, invaderPos, Quaternion.identity);
    }
}