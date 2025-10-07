// PredictionController.cs (REVISI)

using System.Collections;
using System.Collections.Generic; // Diperlukan untuk List
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PredictionController : MonoBehaviour
{
    [Header("API Settings")]
    public string apiBaseUrl = "http://192.168.1.10:5000"; // Ganti dengan IP-mu

    [Header("AR Components")]
    [SerializeField] private ARCameraManager cameraManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI genderText;
    [SerializeField] private TextMeshProUGUI emotionText;
    [SerializeField] private TextMeshProUGUI genderConfidenceText;
    [SerializeField] private TextMeshProUGUI emotionConfidenceText;

    [Header("Visual Effects")]
    [SerializeField] private GameObject happyParticlesPrefab;
    [SerializeField] private GameObject sadParticlesPrefab;
    [SerializeField] private GameObject angryParticlesPrefab;
    [SerializeField] private GameObject surprisedParticlesPrefab;

    [Header("Prediction Settings")]
    [SerializeField] private float predictionInterval = 3.0f;

    private Texture2D m_CameraTexture;

    private GameObject currentParticleEffect;

    void Start()
    {
        InvokeRepeating(nameof(TriggerPrediction), 2.0f, predictionInterval);
    }

    private void TriggerPrediction()
    {
        // PERUBAHAN: Sekarang hanya memanggil satu Coroutine utama
        StartCoroutine(CaptureAndSendImage());
    }

    private IEnumerator CaptureAndSendImage()
    {
        if (!cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            yield break;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
            outputDimensions = new Vector2Int(cpuImage.width / 2, cpuImage.height / 2),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        int size = cpuImage.GetConvertedDataSize(conversionParams);
        var buffer = new Unity.Collections.NativeArray<byte>(size, Unity.Collections.Allocator.Temp);

        cpuImage.Convert(conversionParams, buffer);
        cpuImage.Dispose();

        if (m_CameraTexture == null || m_CameraTexture.width != conversionParams.outputDimensions.x || m_CameraTexture.height != conversionParams.outputDimensions.y)
        {
            m_CameraTexture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);
        }

        m_CameraTexture.LoadRawTextureData(buffer);
        m_CameraTexture.Apply();
        buffer.Dispose();

        byte[] imageBytes = m_CameraTexture.EncodeToJPG();

        // PERUBAHAN: Memanggil coroutine pengiriman yang sudah direvisi
        yield return StartCoroutine(SendRequest("/predict", imageBytes, OnPredictionReceived));
    }

    // PERUBAHAN BESAR: Menggunakan multipart/form-data
    private IEnumerator SendRequest(string endpoint, byte[] data, System.Action<string> onComplete)
    {
        string url = apiBaseUrl + endpoint;

        // Membuat form data untuk dikirim
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        // "image" adalah nama field yang diharapkan oleh request.files['image'] di Flask
        // "image.jpg" adalah nama file yang akan diterima oleh server
        formData.Add(new MultipartFormFileSection("image", data, "image.jpg", "image/jpeg"));

        // Menggunakan UnityWebRequest.Post yang sudah dirancang untuk form data
        using (UnityWebRequest request = UnityWebRequest.Post(url, formData))
        {
            request.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log($"Mengirim gambar ke {url}...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Respons diterima: {request.downloadHandler.text}");
                onComplete?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error dari {endpoint}: {request.error}");
            }
        }
    }

    private void OnPredictionReceived(string jsonResponse)
    {
        PredictionResponse response = JsonUtility.FromJson<PredictionResponse>(jsonResponse);

        // Update teks utama
        genderText.text = $"Gender : {response.gender.label}";
        emotionText.text = $"Emotion : {response.emotion.label}";

        // Update teks confidence secara terpisah
        // ":P0" akan memformat angka 0.98 menjadi "98%"
        genderConfidenceText.text = $"Gender Confidence : {response.gender.confidence:P0}";
        emotionConfidenceText.text = $"Emotion Confidence : {response.emotion.confidence:P0}";

        // Update efek visual berdasarkan emosi
        UpdateVisuals(response.emotion);
    }

    // PERUBAHAN: Fungsi ini sekarang hanya butuh data emosi
    private void UpdateVisuals(EmotionResult emotion)
    {
        // Hancurkan efek partikel sebelumnya (jika ada)
        if (currentParticleEffect != null)
        {
            Destroy(currentParticleEffect);
        }

        GameObject prefabToSpawn = null;
        if (emotion != null)
        {
            switch (emotion.label.ToLower())
            {
                case "happy":
                    prefabToSpawn = happyParticlesPrefab;
                    break;
                case "sad":
                    prefabToSpawn = sadParticlesPrefab;
                    break;
                case "angry":
                    prefabToSpawn = angryParticlesPrefab;
                    break;
                case "surprised":
                    prefabToSpawn = surprisedParticlesPrefab;
                    break;
            }
        }

        // Jika ada prefab yang cocok, munculkan di depan kamera
        if (prefabToSpawn != null)
        {
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
            currentParticleEffect = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }
}