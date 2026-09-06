using UnityEngine;

/// <summary>
/// Item coleccionable en la pista: moneda (puntos) o turbo (impulso de velocidad).
/// Requiere un Collider marcado como "Is Trigger" en el mismo GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrackItem : MonoBehaviour
{
    public enum ItemType
    {
        Coin,
        Turbo
    }

    [Header("Configuración del Item")]
    [SerializeField] private ItemType itemType = ItemType.Coin;
    [Tooltip("Tag que debe tener el auto del jugador para poder recolectar el item.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Moneda")]
    [SerializeField] private int coinValue = 10;

    [Header("Turbo")]
    [SerializeField] private float boostForce = 15f;

    [Header("Feedback (opcional)")]
    [SerializeField] private GameObject collectVFX;
    [SerializeField] private AudioClip collectSFX;
    [Tooltip("Velocidad de rotación visual del item para que se note en la pista (efecto puramente estético).")]
    [SerializeField] private float visualSpinSpeed = 90f;

    private bool collected = false;

    private void Update()
    {
        transform.Rotate(Vector3.up, visualSpinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag(playerTag)) return;

        ArcadeCarController car = other.GetComponent<ArcadeCarController>();
        if (car == null) return;

        collected = true;
        ApplyEffect(car);
        PlayFeedback();

        gameObject.SetActive(false);
    }

    private void ApplyEffect(ArcadeCarController car)
    {
        switch (itemType)
        {
            case ItemType.Coin:
                Debug.Log($"Moneda recolectada: +{coinValue} puntos");
                break;

            case ItemType.Turbo:
                car.ApplyBoost(boostForce);
                Debug.Log("¡Turbo activado!");
                break;
        }
    }

    private void PlayFeedback()
    {
        if (collectVFX != null)
        {
            Instantiate(collectVFX, transform.position, Quaternion.identity);
        }

        if (collectSFX != null)
        {
            AudioSource.PlayClipAtPoint(collectSFX, transform.position);
        }
    }
}
