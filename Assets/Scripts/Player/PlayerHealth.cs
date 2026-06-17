using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public static float maxHealth = 100f;
    public static float currentHealth;

    [Header("UI Referansları")]
    public Slider healthSlider;
    public GameObject deathPanel;
    public TextMeshProUGUI txt_DeathStats; // Olum ekraninda basilacak final istatistikler

    [Header("Başlangıç Ayarları (Inspector'dan Ayarla)")]
    public int startingLevel = 1;          // Oyunun başlatılacağı level
    public float startingMaxHealth = 100f; // Başlangıç maksimum can

    private Coroutine dotCoroutine;
    private Renderer[] playerRenderers;
    private Color[] originalColors;

    void Start()
    {
        if (currentHealth <= 0)
        {
            maxHealth = startingMaxHealth;
            currentHealth = maxHealth;
            PlayerStats.ResetStats();
            AbilityManager.Instance?.ResetAll();
            
            PlayerLevel pl = FindAnyObjectByType<PlayerLevel>();
            if (pl != null)
            {
                pl.ResetLevel(startingLevel);
            }
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Karakterin renklerini kaydet (yanma efekti için)
        playerRenderers = GetComponentsInChildren<Renderer>();
        if (playerRenderers != null && playerRenderers.Length > 0)
        {
            originalColors = new Color[playerRenderers.Length];
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i].material != null)
                    originalColors[i] = playerRenderers[i].material.color;
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0) return;

        // Dodge kontrolü
        if (Random.value < PlayerStats.dodgeChance) return;

        currentHealth -= damageAmount;

        // Hasar sayısını göster
        DamageNumberSpawner.Spawn(transform.position + Vector3.up * 0.3f - transform.forward * 0.6f, damageAmount, true);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        // Extra Life devreye girer
        if (PlayerStats.hasExtraLife && !PlayerStats.extraLifeUsed)
        {
            PlayerStats.extraLifeUsed = true;
            currentHealth = maxHealth;
            if (healthSlider != null) healthSlider.value = currentHealth;
            Debug.Log("EXTRA LIFE! Karakter diriltildi.");
            return;
        }

        Debug.Log("GAME OVER!");
        currentHealth = 0f;

        // Ölüm animasyonunu oynat
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Death");

        // Paneli animasyon bitince göstermek için Coroutine başlat
        StartCoroutine(ShowDeathPanelDelayed());
    }

    private System.Collections.IEnumerator ShowDeathPanelDelayed()
    {
        yield return new WaitForSeconds(2f); // Yere düşme süresi

        // Olum ekranindaki son istatistikleri yazdir
        if (txt_DeathStats != null)
        {
            int mins = Mathf.FloorToInt(GameManager.sessionTimeSpent / 60f);
            int secs = Mathf.FloorToInt(GameManager.sessionTimeSpent % 60f);
            string timeString = string.Format("{0:00}:{1:00}", mins, secs);

            txt_DeathStats.text =
                $"<color=#aaffaa>Ulasilan Oda:</color> {GameManager.currentRoom}\n" +
                $"<color=#ffffaa>Hayatta Kalma Suresi:</color> {timeString}\n" +
                $"<color=#ffaaaa>Katledilen Dusman:</color> {GameManager.sessionEnemiesKilled}\n" +
                $"<color=#ffd700>Kazanilan Altin:</color> {GameManager.sessionGold}";
        }

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ApplyDoT(float duration, float tickDamage, float interval = 1f)
    {
        if (dotCoroutine != null) StopCoroutine(dotCoroutine);
        dotCoroutine = StartCoroutine(DoTRoutine(duration, tickDamage, interval));
    }

    private System.Collections.IEnumerator DoTRoutine(float duration, float tickDamage, float interval)
    {
        GameObject activeVFX = null;
        if (VFXManager.Instance != null)
        {
            activeVFX = VFXManager.Instance.PlayDoTVFX(ElementType.Blaze, transform);
        }

        float elapsed = 0f;
        while (elapsed < duration && currentHealth > 0)
        {
            yield return new WaitForSeconds(interval);
            elapsed += interval;
            TakeDamage(tickDamage);
        }

        if (activeVFX != null)
        {
            ObjectPooler.ReturnToPool(activeVFX);
        }
    }

    // Can arttırma (HP Boost yeteneği veya Bloodthirst)
    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    // Sadece mevcut canı iyileştirir, max canı değiştirmez (Bloodthirst)
    public void HealFlat(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healthSlider != null) healthSlider.value = currentHealth;
    }

    

    public void RetryGame()
    {
        GameManager.EndRunAndSaveGold(); // Kazanilan altinlari ekle

        currentHealth = 0f; // Start()'ta sıfırlamayı tetikler
        Time.timeScale = 1f;

        GameManager gm = FindAnyObjectByType<GameManager>();
        GameManager.currentRoom = (gm != null) ? gm.startingRoom : 1;
        InGameUIManager.hasTimerRun = false; // Sayacı sıfırla
        
        GameManager.lastRoomEnemyCount = 0;

        PlayerStats.ResetStats();
        if (AbilityManager.Instance != null) AbilityManager.Instance.ResetAll();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        GameManager.EndRunAndSaveGold(); // Kazanilan altinlari ekle

        Time.timeScale = 1f;
        GameManager.currentRoom = 1; // Timer bozulmaması için
        InGameUIManager.hasTimerRun = false; // Sayacı sıfırla

        PlayerStats.ResetStats();
        if (AbilityManager.Instance != null) AbilityManager.Instance.ResetAll();

        SceneManager.LoadScene(0);
    }
}