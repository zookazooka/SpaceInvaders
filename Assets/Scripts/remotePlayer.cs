using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using System.Threading;

public class remotePlayer : MonoBehaviour
{
    
    public Projectile laserPrefab; //setup prefab for laser of type pprojectile
    public float speed = 5.0f;

    private bool _laserActive;
        [SerializeField] private WebSocketClient websocket;


    // pretty sure i dont need update()

    public void Move(float command)
{
    Vector3 newPosition = transform.position; // Get the current position
    newPosition.x = command; // Modify only the x-coordinate
    transform.position = newPosition; // Assign the new position back
}


    public void Shoot()
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

   
}
