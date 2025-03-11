using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 direction; //public so we can set in the editor (different for lasers and missiles)
    public float speed;

    public System.Action destroyed;

    private void Update()
    {
        this.transform.position += this.direction * this.speed * Time.deltaTime;   
    }

    private void OnTriggerEnter2D(Collider2D other) //enters when a projectile collision occurs
    {
        if (this.destroyed != null){
        this.destroyed.Invoke(); //this is a callback that allows other scripts to tell when a projectile is destroyed(used to stop players shooting)
        }
        Destroy(this.gameObject); //destroy the projectile
    }
}
