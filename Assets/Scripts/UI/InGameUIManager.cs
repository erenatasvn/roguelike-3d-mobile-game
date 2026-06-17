using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance { get; private set; }

    [Header("UI Elemanlari")]
    public TextMeshProUGUI txt_CurrentGold;
    public GameObject panel_PauseMenu;
    public TextMeshProUGUI txt_PauseStats; // Istatistiklerin yazilacagi text

    [Header("Geri Sayim")]
    public GameObject panel_Countdown;
    public TextMeshProUGUI txt_Countdown;

    private bool isPaused = false;

    public static bool hasTimerRun = false; // Timer'ın sadece 1 kez çalışmasını garantiler

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        RefreshGold();
        if (panel_PauseMenu != null) panel_PauseMenu.SetActive(false);
        if (panel_Countdown != null) panel_Countdown.SetActive(false);

        // Zindana ilk giriste (Sadece 1 kere) geri sayim yap
        if (!hasTimerRun)
        {
            hasTimerRun = true;
            StartCoroutine(CountdownCoroutine());
        }
    }

    private System.Collections.IEnumerator CountdownCoroutine()
    {
        // Zaman durur (boylece oyuncu ve dusmanlar hareket edemez) ama coroutine Time.timeScale'den bagimsiz calisir
        Time.timeScale = 0f; 
        if (panel_Countdown != null) panel_Countdown.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            if (txt_Countdown != null) txt_Countdown.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f); // Realtime kullaniyoruz cunku timeScale 0
        }

        if (txt_Countdown != null) txt_Countdown.text = "BASLA!";
        yield return new WaitForSecondsRealtime(0.5f);

        if (panel_Countdown != null) panel_Countdown.SetActive(false);
        Time.timeScale = 1f; // Oyunu baslat
    }

    // GoldOrb.cs altin topladiginda bunu cagiracak
    public void RefreshGold()
    {
        if (txt_CurrentGold != null)
        {
            txt_CurrentGold.text = GameManager.sessionGold.ToString();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (panel_PauseMenu != null) panel_PauseMenu.SetActive(true);
            RefreshPauseStats(); // Pause acildiginda istatistikleri yenile
        }
        else
        {
            Time.timeScale = 1f;
            if (panel_PauseMenu != null) panel_PauseMenu.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (panel_PauseMenu != null) panel_PauseMenu.SetActive(false);
    }

    private void RefreshPauseStats()
    {
        if (txt_PauseStats != null)
        {
            int mins = Mathf.FloorToInt(GameManager.sessionTimeSpent / 60f);
            int secs = Mathf.FloorToInt(GameManager.sessionTimeSpent % 60f);
            string timeString = string.Format("{0:00}:{1:00}", mins, secs);

            txt_PauseStats.text = 
                $"<color=#aaffaa>Gecilen Oda:</color> {GameManager.currentRoom}\n" +
                $"<color=#ffffaa>Gecen Sure:</color> {timeString}\n" +
                $"<color=#ffaaaa>Oldurulen Dusman:</color> {GameManager.sessionEnemiesKilled}\n" +
                $"<color=#ffd700>Toplanan Altin:</color> {GameManager.sessionGold}";
        }
    }

    public void ReturnToMainMenu()
    {
        GameManager.EndRunAndSaveGold(); // Menuye donerken kazanilan altinlari ana cuzdanina aktar

        Time.timeScale = 1f; // Sahne degisirken zamani normale dondur
        GameManager.currentRoom = 1; // Timer'ın bozulmaması için odayı sıfırla
        hasTimerRun = false; // Timer'ı sıfırla
        SceneManager.LoadScene("MainMenu");
    }
}
