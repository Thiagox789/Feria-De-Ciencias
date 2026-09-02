using UnityEngine;
using System.Collections.Generic;

public class VehicleSelectionManager : MonoBehaviour
{
    public static VehicleSelectionManager Instancia { get; private set; }

    [System.Serializable]
    public class VehiculoInfo
    {
        public string nombre;
        public GameObject prefab;
        public Sprite miniatura;
        public float tiempoRequerido;
    }

    [Header("Vehículos")]
    [SerializeField] private List<VehiculoInfo> vehiculos = new List<VehiculoInfo>();

    private int indiceSeleccionado = 0;

    public int IndiceSeleccionado => indiceSeleccionado;
    public VehiculoInfo VehiculoActual => vehiculos.Count > 0 ? vehiculos[indiceSeleccionado] : null;
    public List<VehiculoInfo> Vehiculos => vehiculos;

    public event System.Action<int> OnAutoCambiado;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        CargarSeleccion();
    }

    public void SeleccionarAuto(int indice)
    {
        if (indice < 0 || indice >= vehiculos.Count) return;

        if (!EsAutoDesbloqueado(indice)) return;

        indiceSeleccionado = indice;
        GuardarSeleccion();
        OnAutoCambiado?.Invoke(indice);
    }

    public void SeleccionarSiguiente()
    {
        int siguiente = indiceSeleccionado + 1;
        if (siguiente >= vehiculos.Count) siguiente = 0;

        while (siguiente != indiceSeleccionado && !EsAutoDesbloqueado(siguiente))
        {
            siguiente++;
            if (siguiente >= vehiculos.Count) siguiente = 0;
        }

        if (siguiente != indiceSeleccionado)
            SeleccionarAuto(siguiente);
    }

    public void SeleccionarAnterior()
    {
        int anterior = indiceSeleccionado - 1;
        if (anterior < 0) anterior = vehiculos.Count - 1;

        while (anterior != indiceSeleccionado && !EsAutoDesbloqueado(anterior))
        {
            anterior--;
            if (anterior < 0) anterior = vehiculos.Count - 1;
        }

        if (anterior != indiceSeleccionado)
            SeleccionarAuto(anterior);
    }

    public bool EsAutoDesbloqueado(int indice)
    {
        if (indice < 0 || indice >= vehiculos.Count) return false;
        return true;
    }

    public GameObject ObtenerPrefabVehiculo()
    {
        if (vehiculos.Count == 0) return null;
        return vehiculos[indiceSeleccionado].prefab;
    }

    private void GuardarSeleccion()
    {
        PlayerPrefs.SetInt("VehicleSelection", indiceSeleccionado);
        PlayerPrefs.Save();
    }

    private void CargarSeleccion()
    {
        if (PlayerPrefs.HasKey("VehicleSelection"))
            indiceSeleccionado = PlayerPrefs.GetInt("VehicleSelection");

        if (indiceSeleccionado < 0 || indiceSeleccionado >= vehiculos.Count)
            indiceSeleccionado = 0;
    }
}