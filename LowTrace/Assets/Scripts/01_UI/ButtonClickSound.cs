using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.PlaySFXBoton();
        });
    }
}
