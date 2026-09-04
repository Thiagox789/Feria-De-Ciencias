using UnityEngine;
using UnityEngine.EventSystems;

public class DebugUI : MonoBehaviour
{
    private string info = "";

    private void Update()
    {
        Vector2 mouse = Input.mousePosition;
        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 localPoint;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouse, canvas.worldCamera, out localPoint);

        info = $"Screen: {Screen.width}x{Screen.height}\n" +
               $"Mouse: {mouse.x:F0},{mouse.y:F0}\n" +
               $"Canvas local: {localPoint.x:F0},{localPoint.y:F0}\n" +
               $"DPI: {Screen.dpi}\n" +
               $"Fullscreen: {Screen.fullScreen}";

        if (EventSystem.current != null)
        {
            var go = EventSystem.current.currentSelectedGameObject;
            info += $"\nSelected: {(go != null ? go.name : "none")}";
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 20;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.yellow;
        GUI.Box(new Rect(10, 10, 500, 200), info, style);
    }
}
