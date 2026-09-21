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
    [SerializeField] [Range(0.001f, 0.1f)] private float umbralZonaMuerta = 0.02f; // 2% de margen mínimo en el centro
    [SerializeField] private float potenciometroMinimo = 0f;
    [SerializeField] private float potenciometroMaximo = 962f;

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

        // Si viene con etiquetas legibles de Arduino (ej: "Volante: 50 Acel: 1 Freno: 0" o "Volante: -45 | Max Detectado: 530 | Acel: 0 Freno: 0")
        if (texto.Contains("Acel:"))
        {
            try
            {
                string tagVol = texto.Contains("Volante:") ? "Volante:" : (texto.Contains("Valor:") ? "Valor:" : "");
                if (!string.IsNullOrEmpty(tagVol))
                {
                    int pVal = texto.IndexOf(tagVol) + tagVol.Length;
                    int pAcel = texto.IndexOf("Acel:");
                    string valStr = texto.Substring(pVal, pAcel - pVal).Trim();
                    if (valStr.Contains("|")) valStr = valStr.Split('|')[0].Trim();

                    if (float.TryParse(valStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float rawVal))
                    {
                        ProcesarLecturaVolante(rawVal);
                    }
                }

                int idxAcel = texto.IndexOf("Acel:") + 5;
                int idxFreno = texto.IndexOf("Freno:");
                if (idxAcel > 4 && idxFreno > idxAcel)
                {
                    string acelStr = texto.Substring(idxAcel, idxFreno - idxAcel).Trim();
                    if (acelStr.Contains("|")) acelStr = acelStr.Split('|')[0].Trim();
                    if (float.TryParse(acelStr, out float acelVal)) EntradaAcelerador = Mathf.Clamp01(acelVal);
                }

                if (idxFreno > 0)
                {
                    string frenoStr = texto.Substring(idxFreno + 6).Trim();
                    if (frenoStr.Contains("|")) frenoStr = frenoStr.Split('|')[0].Trim();
                    if (float.TryParse(frenoStr, out float frenoVal)) EntradaFreno = Mathf.Clamp01(frenoVal);
                }
                return;
            }
            catch { }
        }

        // Si el mensaje empieza solo con "Volante: " o "Valor: ", remover la etiqueta
        if (texto.StartsWith("Volante:")) texto = texto.Replace("Volante:", "").Trim();
        if (texto.StartsWith("Valor:")) texto = texto.Replace("Valor:", "").Trim();

        // Si es formato CSV (ejemplo: "-0.45,1,0" o "512,1,0")
        string[] datos = texto.Split(',');
        if (datos.Length > 1)
        {
            if (float.TryParse(datos[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float dir))
            {
                ProcesarLecturaVolante(dir);
            }

            if (float.TryParse(datos[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float acel))
                EntradaAcelerador = Mathf.Clamp01(acel);

            if (datos.Length >= 3 && float.TryParse(datos[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float frn))
                EntradaFreno = Mathf.Clamp01(frn);
        }
        else
        {
            // Lectura simple de una sola variable
            if (float.TryParse(texto, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float valorRaw))
            {
                ProcesarLecturaVolante(valorRaw);
            }
        }
    }

    private void ProcesarLecturaVolante(float valor)
    {
        // Si viene en rango de porcentaje (-100 a 100) desde Arduino map(val, min, max, -100, 100)
        if (Mathf.Abs(valor) > 1.0f && Mathf.Abs(valor) <= 100.0f)
        {
            EntradaDireccion = Mathf.Clamp(valor / 100.0f, -1f, 1f);
        }
        // Si viene en rango analógico raw (0 a 1023)
        else if (valor >= 0f && valor <= 1023f)
        {
            float max = potenciometroMaximo > potenciometroMinimo ? potenciometroMaximo : 962f;
            float dirNorm = Mathf.InverseLerp(potenciometroMinimo, max, valor); // 0 a 1
            EntradaDireccion = Mathf.Lerp(-1f, 1f, dirNorm);     // -1 a 1
        }
        else
        {
            EntradaDireccion = Mathf.Clamp(valor, -1f, 1f);
        }

        if (Mathf.Abs(EntradaDireccion) < umbralZonaMuerta)
        {
            EntradaDireccion = 0f;
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
