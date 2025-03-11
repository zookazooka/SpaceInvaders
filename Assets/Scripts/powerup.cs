using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { TripleShot, SpeedBoost }
    public PowerUpType powerUpType;
    public float duration = 5f; 

    private void OnTriggerEnter2D(Collider2D other)
    {
         if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.gameObject.layer == LayerMask.NameToLayer("Missile")) {
            
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                ActivatePowerUp(player);
            }

            Destroy(gameObject); 
        }
    }

    private void ActivatePowerUp(Player player)
    {
        switch (powerUpType)
        {
            case PowerUpType.TripleShot:
                player.StartCoroutine(player.ActivateTripleShot(duration));
                break;
            case PowerUpType.SpeedBoost:
                player.StartCoroutine(player.ActivateSpeedBoost(duration));
                break;
        }
    }
}
