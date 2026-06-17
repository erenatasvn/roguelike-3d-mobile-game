using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Eger giren sey karakterimiz ise
        if (other.CompareTag("Player"))
        {
            // Portaldan gecilince odanin sayisini 1 artir
            GameManager.currentRoom++;
            Debug.Log("Portaldan gecildi! Yeni Oda: " + GameManager.currentRoom);

            // Su anki sahneyi bastan yukle
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}