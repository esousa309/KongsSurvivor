using UnityEngine;

public class DiagnosticsOverlay : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 20, 280, 20),
            $"FPS: {1f/Time.deltaTime:0}   Ω: {CurrencyManager.TotalOmega}   Enemies: {FindObjectsOfType<Enemy>().Length}");
    }
}