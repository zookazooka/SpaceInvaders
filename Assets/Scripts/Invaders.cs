using UnityEngine;
using UnityEngine.SceneManagement;
public class Invaders : MonoBehaviour
{
    public int rows = 5;

    public int columns = 11; //grid of invaders

    public Projectile missilePrefab;
    public Invader[] prefabs;

    public float missileAttackRate = 1.0f;

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
        for (int row = 0; row <this.rows; row++){
            //width and height of our grid
            float width = 2.0f * (this.columns-1);
            float height = 2.0f * (this.rows-1);

            //find center point
            Vector2 centering = new Vector2(-width/2, -height/2);
            Vector3 rowPosition = new Vector3(centering.x , centering.y + row*2.0f ,0.0f);

            for (int col = 0; col<this.columns; col++) {
                Invader invader = Instantiate(this.prefabs[row], this.transform); //this adds to the game
                invader.killed += InvaderKilled;//rund invaderKilled on action invoked
                Vector3 position = rowPosition;
                position.x += col * 2.0f; //this changes the spacing for each invader

                invader.transform.localPosition = position; //sets the in game values should set local vals so parent object can still effect
            }
        }
    }

    private void Update() //update is called every frame the game is running
    {
        this.transform.position += _direction * this.speed.Evaluate(this.percentKilled) * Time.deltaTime; //moves the block of invaders to the right initially
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
        _direction.x *= -1.0f; //flip direction

        Vector3 position = this.transform.position;
        position.y -= 1.0f; //advance the invaders down the screen by 1
        this.transform.position = position;
    }
    private void InvaderKilled() {
        amountKilled +=1;

        if (this.amountKilled >= totalInvaders){
            //runs when last invader dies
            this.allKilled.Invoke();


        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(MissileAttack), this.missileAttackRate, this.missileAttackRate);
    }
    private void MissileAttack()
    {
        foreach(Transform invader in this.transform) 
        {
            if (!invader.gameObject.activeInHierarchy) 
            {
                continue;
            }

            //missile spawns are inversly proportional to number of invaders alive
            if (Random.value < (1.0f / (float)(totalInvaders-amountKilled))) {
                Instantiate(this.missilePrefab, invader.position, Quaternion.identity);
                break; //means only one missile can spawn per cycle
            }
        }
    }
}
