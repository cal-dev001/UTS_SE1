// SplashScreenManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk mengelola scene!

public class SplashScreenManager : MonoBehaviour
{
    [Tooltip("Waktu dalam detik sebelum pindah ke scene berikutnya.")]
    public float delayInSeconds = 3f;

    [Tooltip("Nama file scene utama yang akan dimuat. Pastikan namanya sama persis!")]
    public string mainSceneName = "SampleScene"; // Ganti "SampleScene" jika nama scene AR-mu berbeda

    // Start dipanggil saat frame pertama sebelum Update
    void Start()
    {
        // Memulai Coroutine untuk menangani penundaan (delay)
        StartCoroutine(LoadMainSceneAfterDelay());
    }

    private IEnumerator LoadMainSceneAfterDelay()
    {
        // 1. Tunggu selama waktu yang ditentukan
        Debug.Log($"Splash screen ditampilkan. Menunggu {delayInSeconds} detik...");
        yield return new WaitForSeconds(delayInSeconds);

        // 2. Setelah menunggu, muat scene utama
        Debug.Log($"Waktu tunggu selesai. Memuat scene: {mainSceneName}");
        SceneManager.LoadScene(mainSceneName);
    }
}