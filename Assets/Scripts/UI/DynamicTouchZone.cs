using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicTouchZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Referanslar")]
    // Hareket ettirecegimiz Joystick'in RectTransform bileseni
    public RectTransform joystickRect;

    // Projedeki mevcut Joystick kodunun scripti
    public Joystick joystickScript;

    // Joystick'in varsayilan baslangic pozisyonu
    private Vector2 defaultAnchoredPosition;

    void Start()
    {
        if (joystickRect != null)
        {
            // Baslangictaki UI koordinatlarini kaydet
            defaultAnchoredPosition = joystickRect.anchoredPosition;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Joystick'i dokunulan konuma tasi
        joystickRect.position = eventData.position;

        // Dokunma eventini ilgili script'e ilet
        if (joystickScript != null)
            joystickScript.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Surukleme hareketini joystick'e ilet
        if (joystickScript != null)
            joystickScript.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Dokunma bitince joystick'i varsayilan konumuna sifirla
        joystickRect.anchoredPosition = defaultAnchoredPosition;

        // Birakilma eventini ilgili script'e ilet
        if (joystickScript != null)
            joystickScript.OnPointerUp(eventData);
    }
}