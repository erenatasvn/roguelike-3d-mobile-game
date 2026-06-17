using UnityEngine;
using TMPro;

// Zindan sekmesini yöneten script.
// Mevcut zindan ilerlemesini gösterir.
public class DungeonManager : MonoBehaviour
{
    [Header("Zindan Bilgisi")]
    public TextMeshProUGUI txt_DungeonInfo;
    public TextMeshProUGUI txt_Progress;

    void OnEnable() => RefreshUI();

    void RefreshUI()
    {
        SaveData save = MainMenuManager.CurrentSave;
        if (save == null) return;

        if (txt_DungeonInfo != null)
            txt_DungeonInfo.text = $"Zindan {save.currentDungeon}";

        if (txt_Progress != null)
        {
            int cleared = save.dungeonsCleared;
            txt_Progress.text = cleared > 0
                ? $"Tamamlanan Zindan: {cleared}\nYeni zindan acik!"
                : "Ilk zindani tamamla, yeni zindan acilsin!";
        }
    }
}
