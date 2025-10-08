// ApiResponse.cs

using System;

// Class ini TIDAK perlu diubah, tapi namanya kita ganti agar lebih jelas
[Serializable]
public class GenderResult
{
    public string label;
    public float confidence;
}

// Class ini juga TIDAK perlu diubah, namanya juga kita ganti
[Serializable]
public class EmotionResult
{
    public string label;
    public float confidence;
}

// INI CLASS UTAMA YANG BARU
// Strukturnya mencerminkan JSON yang dikirim oleh Flask
[Serializable]
public class PredictionResponse
{
    public GenderResult gender;
    public EmotionResult emotion;
}