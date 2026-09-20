using UnityEngine;
using System.IO.Ports;
using System.Threading;

// Este script gestiona la lectura del puerto serie del Arduino Uno en segundo plano.
// Permite controlar la dirección con potenciómetro y los pedales (acelerador/freno).
public class ArduinoInputReader : MonoBehaviour
{
    private static ArduinoInputReader _instancia;

    public static ArduinoInputReader Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = FindFirstObjectByType<ArduinoInputReader>();
                if (_instancia == null)
                {
                    GameObject go = new GameObject("ArduinoInputReader");
                    _instancia = go.AddComponent<ArduinoInputReader>();
                }
            }
            return _instancia;
        }
    }

    [Header("Configuración Serie")]
    [SerializeField] private int baudRate = 9600;

    [Header("Calibración Volante")]
    [SerializeField] [Range(0.01f, 0.3f)] private float umbralZonaMuerta = 0.12f; // 12% de margen en el centro

    [Header("Valores Recibidos de Arduino")]
    public float EntradaDireccion = 0f;  // Rango de -1.0 a 1.0
    public float EntradaAcelerador = 0f; // Rango 0.0 a 1.0 (o 0 y 1)
    public float EntradaFreno = 0f;      // Rango 0.0 a 1.0 (o 0 y 1)
    public bool Conectado = false;

    private SerialPort puertoSerie;
    private Thread hiloLectura;
    private volatile bool leyendo = false;
    private volatile string ultimaLinea = "";

    private void Awake()
    {
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        _instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InicializarPuerto();
    }

    public void InicializarPuerto()
    {
        CerrarPuerto();

        string nombrePuerto = DataManager.Instancia != null && DataManager.Instancia.ajustes != null 
            ? DataManager.Instancia.ajustes.puertoSerial 
            : "";

        if (string.IsNullOrEmpty(nombrePuerto) || nombrePuerto == "Ninguno") 
        {
            Conectado = false;
            return;
        }

        try
        {
            puertoSerie = new SerialPort(nombrePuerto, baudRate);
            puertoSerie.ReadTimeout = 100;
            puertoSerie.WriteTimeout = 100;
            puertoSerie.Open();

            leyendo = true;
            hiloLectura = new Thread(LeerSerialEnSegundoPlano);
            hiloLectura.IsBackground = true;
            hiloLectura.Start();
            Conectado = true;
            Debug.Log($"[Arduino] Conectado exitosamente en puerto: {nombrePuerto}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Arduino] No se pudo abrir el puerto '{nombrePuerto}': {e.Message}");
            Conectado = false;
        }
    }

    private void LeerSerialEnSegundoPlano()
    {
        while (leyendo && puertoSerie != null && puertoSerie.IsOpen)
        {
            try
            {
                string linea = puertoSerie.ReadLine();
                if (!string.IsNullOrEmpty(linea))
                {
                    ultimaLinea = linea.Trim();
                }
            }
            catch (System.TimeoutException) { }
            catch (System.Exception) { break; }
        }
    }

    private void Update()
    {
        if (!Conectado || string.IsNullOrEmpty(ultimaLinea)) return;

        string texto = ultimaLinea;

        // Si el mensaje empieza con "Valor: ", remover la etiqueta
        if (texto.StartsWith("Valor:"))
        {
            texto = texto.Replace("Valor:", "").Trim();
        }

        // Si es formato CSV (ejemplo: "-0.45,1,0")
        string[] datos = texto.Split(',');
        if (datos.Length > 1)
        {
            if (float.TryParse(datos[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float dir))
                EntradaDireccion = Mathf.Clamp(dir, -1f, 1f);

            if (float.TryParse(datos[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float acel))
                EntradaAcelerador = Mathf.Clamp01(acel);

            if (datos.Length >= 3 && float.TryParse(datos[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float frn))
                EntradaFreno = Mathf.Clamp01(frn);
        }
        else
        {
            // Si es una sola lectura raw del potenciómetro (ejemplo: 0 a 998 de analogRead)
            if (float.TryParse(texto, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float valorRaw))
            {
                // Si el valor viene en rango analógico de potenciómetro (0 a 998)
                if (valorRaw >= 0f && valorRaw <= 1023f)
                {
                    float dirNorm = Mathf.InverseLerp(0f, 998f, valorRaw); // 0 a 1
                    float dirBruta = Mathf.Lerp(-1f, 1f, dirNorm);        // -1 a 1

                    // Aplicar zona muerta central amplia y re-escalado suave al salir del centro
                    if (Mathf.Abs(dirBruta) < umbralZonaMuerta)
                    {
                        EntradaDireccion = 0f;
                    }
                    else
                    {
                        float signo = Mathf.Sign(dirBruta);
                        float magnitud = Mathf.InverseLerp(umbralZonaMuerta, 1f, Mathf.Abs(dirBruta));
                        EntradaDireccion = signo * magnitud;
                    }
                }
                else
                {
                    EntradaDireccion = Mathf.Clamp(valorRaw, -1f, 1f);
                }
            }
        }
    }

    private void OnDestroy() => CerrarPuerto();
    private void OnApplicationQuit() => CerrarPuerto();

    public void CerrarPuerto()
    {
        leyendo = false;
        if (hiloLectura != null && hiloLectura.IsAlive)
        {
            hiloLectura.Join(200);
        }

        if (puertoSerie != null)
        {
            if (puertoSerie.IsOpen)
            {
                try { puertoSerie.Close(); } catch { }
            }
            puertoSerie.Dispose();
            puertoSerie = null;
        }

        Conectado = false;
    }
}
