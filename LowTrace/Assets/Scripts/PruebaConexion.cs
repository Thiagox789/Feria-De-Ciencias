using UnityEngine;
using System.IO.Ports;

public class PruebaConexion : MonoBehaviour
{
    [Header("Configuración de Puerto")]
    public string puertoCom = "/dev/ttyUSB0"; 
    public int baudios = 9600;
    private SerialPort stream;

    // Variables de lectura
    private float valorPotenciometro = 512f;
    private int botonAcelerar = 0;
    private int botonFrenar = 0;

    [Header("Filtros Anti-Ruido (Para 100k Inestable)")]
    [Range(0.01f, 1f)] public float suavizado = 0.1f; // Más bajo = más suave y menos temblor
    public float umbralCambio = 15f; // Ignora variaciones menores a este número (ej. saltos de 820 a 835)
    
    private float ultimoValorEstable = 512f;
    private float escalaSuave = 1f;
    private Renderer miRenderer;

    void Start()
    {
        miRenderer = GetComponent<Renderer>();
        stream = new SerialPort(puertoCom, baudios);
        stream.ReadTimeout = 20; 
        
        try {
            stream.Open();
            Debug.Log("¡Conectado a Arduino!");
        }
        catch (System.Exception e) {
            Debug.LogError("Error de conexión: " + e.Message);
        }
    }

    void Update()
    {
        LeerArduino();
        AplicarEfectos();
    }

    void LeerArduino()
    {
        if (stream != null && stream.IsOpen)
        {
            try
            {
                string datosEntrada = stream.ReadLine();
                string[] datos = datosEntrada.Split(',');

                if (datos.Length == 3)
                {
                    float lecturaNueva = float.Parse(datos[0]);

                    // --- FILTRO DE UMBRAL ---
                    // Si el cambio con respecto a la última lectura es menor que el umbral,
                    // asumimos que es ruido eléctrico y mantenemos el valor anterior.
                    if (Mathf.Abs(lecturaNueva - ultimoValorEstable) > umbralCambio)
                    {
                        ultimoValorEstable = lecturaNueva;
                    }

                    valorPotenciometro = ultimoValorEstable;
                    botonAcelerar = int.Parse(datos[1]);
                    botonFrenar = int.Parse(datos[2]);
                }
            }
            catch (System.TimeoutException) { }
        }
    }

    void AplicarEfectos()
    {
        // Convertimos el rango del potenciómetro a escala física (0.5 a 4.0)
        float escalaObjetivo = Mathf.Lerp(0.5f, 4.0f, valorPotenciometro / 1023f);
        
        // --- FILTRO LERP (AMORTIGUACIÓN) ---
        // Absorbe los saltos bruscos haciendo que el cubo viaje suavemente al objetivo
        escalaSuave = Mathf.Lerp(escalaSuave, escalaObjetivo, suavizado);
        transform.localScale = new Vector3(escalaSuave, escalaSuave, escalaSuave);

        // Control de color por botones
        if (botonAcelerar == 1) miRenderer.material.color = Color.green;
        else if (botonFrenar == 1) miRenderer.material.color = Color.red;
        else miRenderer.material.color = Color.white;
    }

    void OnApplicationQuit()
    {
        if (stream != null && stream.IsOpen) stream.Close();
    }
}
