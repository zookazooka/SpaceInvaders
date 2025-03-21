using UnityEngine;

public class Invader : MonoBehaviour
{
    public Sprite[] animationSprites; //array of sprites
    public float animationTime; //time for icon change

    public System.Action killed;
    private SpriteRenderer _spriteRenderer; //changes which sprite is being rendered
    private int _animationFrame; // keeps track of what is being rendered
    [SerializeField] private Player player;

    private void Awake() //first function that gets called(automatically)
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); //assigns sprite renderer to game object we are running script on
        player = FindObjectOfType<Player>();


    }

    private void Start() //gets invoked on very first frame for game object
    {
        InvokeRepeating(nameof(AnimateSprite), this.animationTime, this.animationTime);  //calls animate sprite 
    }
    private void AnimateSprite()
    {
        _animationFrame++;//go to next frame

        if (_animationFrame >= this.animationSprites.Length) {
            _animationFrame =0;//reset animation sequence at end of array
        }
        _spriteRenderer.sprite = this.animationSprites[_animationFrame]; //setting rendered sprite to the sequence
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ReplayManager.Instance.IsReplaying()) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Laser")) { //on laser-invader collision deactivate invader
            this.gameObject.SetActive(false);

            this.killed.Invoke();
            player.IncrementScore();
        }   
    }
}
