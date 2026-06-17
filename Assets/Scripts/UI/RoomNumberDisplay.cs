using UnityEngine;
using TMPro;

public class RoomNumberDisplay : MonoBehaviour
{
    public TMP_Text roomText; 

    // Portal açıldığında çalışır
    void OnEnable()
    {
        // Inspector'dan unuttuysak, objenin kendi üstündeki TextMeshPro'yu otomatik bul
        if (roomText == null)
        {
            roomText = GetComponent<TMP_Text>();
        }

        if (roomText != null)
        {
            // Portaldan geçilecek sonraki odayı göster
            roomText.text = "Oda: " + (GameManager.currentRoom + 1);
        }
    }
}
