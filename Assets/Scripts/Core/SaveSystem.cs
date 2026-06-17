using UnityEngine;
using System.IO;


public static class SaveSystem
{
    private static readonly string SavePath = 
        Path.Combine(Application.persistentDataPath, "save.json");

    //  Kaydet
    public static void Save(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Kaydedildi → {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Kayıt hatası: {e.Message}");
        }
    }

    //  Yükle 
    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] Kayıt dosyası yok — Yeni oyuncu oluşturuluyor.");
            return new SaveData(); // (isFirstLaunch = true)
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[SaveSystem] Yüklendi → {SavePath}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Yükleme hatası: {e.Message} — Varsayılan veri kullanılıyor.");
            return new SaveData();
        }
    }

    // Kayıt var mı
    public static bool HasSave() => File.Exists(SavePath);

    // Kayıt dosyasının yolunu döndür
    public static string GetSavePath() => SavePath;

    //  Sil/Sıfırla
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem] Kayıt silindi.");
        }
    }
}
