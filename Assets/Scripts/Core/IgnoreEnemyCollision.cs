using UnityEngine;

// Bu script eklendiği objenin (görünmez Duvar) düşmanlarla çarpışmasını yoksayar.
// Böylece düşmanlar içinden geçebilir ama oyuncu (Player) geçemez.

public class IgnoreEnemyCollision : MonoBehaviour
{
    private Collider myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics.IgnoreCollision(collision.collider, myCollider);
        }
    }

    // Tetiklenme ihtimaline karşı (eğer üst üste doğarlarsa)
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Physics.IgnoreCollision(collision.collider, myCollider);
        }
    }
}
