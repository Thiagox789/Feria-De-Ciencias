using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class DPIHandler : MonoBehaviour
{
    private CanvasScaler scaler;

    private void Awake()
    {
        scaler = GetComponent<CanvasScaler>();
        if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) return;

        scaler.referenceResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        Debug.Log($"[DPIHandler] Ref res = {scaler.referenceResolution}, Screen = {Screen.width}x{Screen.height}, DPI = {Screen.dpi}");
    }
}
