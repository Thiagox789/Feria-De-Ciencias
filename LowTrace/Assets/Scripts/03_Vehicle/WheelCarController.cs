using UnityEngine;

// Este es el script principal que maneja la física y conducción del vehículo.
// Usa los WheelColliders de Unity para la tracción y amortiguación de las ruedas,
// ofreciendo un control tipo arcade (ágil, responsivo y fácil de manejar).
public class WheelCarController : MonoBehaviour
{
    [Header("Referencias de Objetos y Física")]
    [SerializeField] private Rigidbody cuerpoFisicoAuto;
    [SerializeField] private WheelCollider ruedaDelanteraIzquierda;
    [SerializeField] private WheelCollider ruedaDelanteraDerecha;
    [SerializeField] private WheelCollider ruedaTraseraIzquierda;
    [SerializeField] private WheelCollider ruedaTraseraDerecha;

    [Header("Modelos 3D de las Ruedas")]
    [SerializeField] private Transform mallaDelanteraIzquierda;
    [SerializeField] private Transform mallaDelanteraDerecha;
    [SerializeField] private Transform mallaTraseraIzquierda;
    [SerializeField] private Transform mallaTraseraDerecha;

    private Quaternion[] rotacionesInicialesMallas = new Quaternion[4];

    [Header("Aceleración y Velocidad")]
    [SerializeField] private float fuerzaMotor = 4000f; // Torque del motor
    [SerializeField] private float velocidadMaximaAdelante = 250f; // En km/h
    [SerializeField] private float velocidadMaximaReversa = 60f;

    [Header("Frenado")]
    [SerializeField] private float fuerzaFreno = 4000f;
    [SerializeField] private float frenoMotorAlSoltarAcelerador = 800f;

    [Header("Dirección (Giro de Ruedas)")]
    [SerializeField] private float anguloMaximoGiro = 35f; // Ángulo a baja velocidad
    [SerializeField] private float anguloMinimoGiroAltaVelocidad = 18f; // Ángulo a máxima velocidad
    [SerializeField] [Range(0.05f, 0.4f)] private float velocidadSuavizadoGiro = 0.15f;

    [Header("Asistencia de Giro Arcade")]
    [SerializeField] private float fuerzaAyudaGiro = 10f;
    [SerializeField] private float velocidadMinimaParaAyudaGiro = 15f;
    [SerializeField] private float velocidadRotacionMaximaY = 1.8f;
    [SerializeField] private float amortiguacionRotacionReposo = 10f;
    [SerializeField] private float amortiguacionRotacionGirando = 5f;

    [Header("Agarre y Deslizamiento (Grip)")]
    [SerializeField] private float agarreRuedasTraseras = 1.2f;
    [SerializeField] private float agarreRuedasDelanteras = 1.6f;

    [Header("Amortiguación Lateral (Evita resbalar como en hielo)")]
    [SerializeField] [Range(0.8f, 1f)] private float amortiguacionVelocidadLateral = 0.92f;

    [Header("Centro de Masa")]
    [SerializeField] private Transform puntoCentroDeMasa;

    [Header("Valores de Lectura (Debug)")]
    [SerializeField] private float velocidadActualKMH;
    [SerializeField] private float porcentajeVelocidadMax;
    [SerializeField] private float anguloGiroActual;
    [SerializeField] private float entradaDireccion;
    [SerializeField] private float entradaAcelerador;
    [SerializeField] private float entradaFreno;

    private bool carreraIniciada;

    // Propiedades públicas para la cámara y los ítems de la pista
    public float RatioVelocidad01 => porcentajeVelocidadMax;
    public float SpeedRatio01 => porcentajeVelocidadMax;
    public float VelocidadAdelante => velocidadActualKMH / 3.6f;
    public float CurrentForwardSpeed => velocidadActualKMH / 3.6f;
    public float VelocidadActualKMH => Mathf.Abs(velocidadActualKMH);
    public float CurrentSpeedKMH => Mathf.Abs(velocidadActualKMH);

    private void Awake()
    {
        if (cuerpoFisicoAuto == null)
        {
            cuerpoFisicoAuto = GetComponent<Rigidbody>();
        }

        // Bajamos el centro de masa para evitar que el auto se vuelque fácilmente en curvas
        if (puntoCentroDeMasa != null)
        {
            cuerpoFisicoAuto.centerOfMass = puntoCentroDeMasa.localPosition;
        }

        // Limitamos la velocidad de rotación para evitar trompos infinitos
        cuerpoFisicoAuto.maxAngularVelocity = velocidadRotacionMaximaY;

        // Guardamos las rotaciones iniciales de las llantas 3D
        rotacionesInicialesMallas[0] = mallaDelanteraIzquierda != null ? mallaDelanteraIzquierda.localRotation : Quaternion.identity;
        rotacionesInicialesMallas[1] = mallaDelanteraDerecha != null ? mallaDelanteraDerecha.localRotation : Quaternion.identity;
        rotacionesInicialesMallas[2] = mallaTraseraIzquierda != null ? mallaTraseraIzquierda.localRotation : Quaternion.identity;
        rotacionesInicialesMallas[3] = mallaTraseraDerecha != null ? mallaTraseraDerecha.localRotation : Quaternion.identity;

        AplicarConfiguracionAgarre();
    }

    private void AplicarConfiguracionAgarre()
    {
        EstablecerFriccionLateral(ruedaDelanteraIzquierda, agarreRuedasDelanteras);
        EstablecerFriccionLateral(ruedaDelanteraDerecha, agarreRuedasDelanteras);
        EstablecerFriccionLateral(ruedaTraseraIzquierda, agarreRuedasTraseras);
        EstablecerFriccionLateral(ruedaTraseraDerecha, agarreRuedasTraseras);
    }

    private void EstablecerFriccionLateral(WheelCollider col, float rigidez)
    {
        if (col == null) return;
        WheelFrictionCurve curva = col.sidewaysFriction;
        curva.stiffness = rigidez;
        col.sidewaysFriction = curva;
    }

    private void Update()
    {
        // Leemos las teclas presionadas por el jugador (Flechas o WASD)
        entradaAcelerador = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f;
        entradaFreno = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? 1f : 0f;

        // Al tocar cualquier tecla de movimiento se inicia el conteo del tiempo de carrera
        if (!carreraIniciada && (entradaAcelerador != 0 || Mathf.Abs(entradaDireccion) > 0.05f))
        {
            carreraIniciada = true;
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.IniciarCarrera();
            }
        }
    }

    private void FixedUpdate()
    {
        // Procesamos la física en cada paso fijo del motor físico (FixedUpdate)
        SuavizarEntradaDireccion();
        CalcularVelocidad();
        ProcesarAceleracion();
        ProcesarFrenado();
        ProcesarFrenoDeMotor();
        ProcesarDireccion();
        ProcesarAyudaDeGiro();
        ProcesarAmortiguacionLateral();
        SincronizarMallasRuedas();
    }

    private void SuavizarEntradaDireccion()
    {
        float direccionObjetivo = 0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direccionObjetivo = 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direccionObjetivo = -1f;

        // Suavizamos el giro del volante progresivamente
        entradaDireccion = Mathf.Lerp(entradaDireccion, direccionObjetivo, velocidadSuavizadoGiro);
    }

    private void CalcularVelocidad()
    {
        // Calculamos la velocidad real hacia adelante en km/h
        velocidadActualKMH = Vector3.Dot(cuerpoFisicoAuto.transform.forward, cuerpoFisicoAuto.linearVelocity) * 3.6f;
        porcentajeVelocidadMax = Mathf.InverseLerp(0f, velocidadMaximaAdelante, Mathf.Abs(velocidadActualKMH));
    }

    private void ProcesarAceleracion()
    {
        float fuerzaDisponible = Mathf.Lerp(fuerzaMotor, 0f, porcentajeVelocidadMax);

        if (entradaAcelerador > 0f && Mathf.Abs(velocidadActualKMH) < velocidadMaximaAdelante)
        {
            ruedaTraseraIzquierda.motorTorque = fuerzaDisponible * entradaAcelerador;
            ruedaTraseraDerecha.motorTorque = fuerzaDisponible * entradaAcelerador;
        }
        else
        {
            ruedaTraseraIzquierda.motorTorque = 0f;
            ruedaTraseraDerecha.motorTorque = 0f;
        }
    }

    private void ProcesarFrenado()
    {
        if (entradaFreno > 0f)
        {
            if (velocidadActualKMH > 1f)
            {
                // Frenando en seco hacia adelante
                ruedaDelanteraIzquierda.brakeTorque = fuerzaFreno * entradaFreno;
                ruedaDelanteraDerecha.brakeTorque = fuerzaFreno * entradaFreno;
                ruedaTraseraIzquierda.brakeTorque = fuerzaFreno * entradaFreno;
                ruedaTraseraDerecha.brakeTorque = fuerzaFreno * entradaFreno;
            }
            else
            {
                // Ya detenido: marcha atrás suave
                LimpiarFrenos();
                float fuerzaReversa = fuerzaMotor * 0.4f;
                ruedaTraseraIzquierda.motorTorque = -entradaFreno * fuerzaReversa;
                ruedaTraseraDerecha.motorTorque = -entradaFreno * fuerzaReversa;
            }
        }
        else
        {
            LimpiarFrenos();
        }
    }

    private void ProcesarFrenoDeMotor()
    {
        // Si no aceleramos ni frenamos, el auto desacelera paulatinamente
        if (entradaAcelerador == 0f && entradaFreno == 0f && Mathf.Abs(velocidadActualKMH) > 0.5f)
        {
            ruedaTraseraIzquierda.brakeTorque = frenoMotorAlSoltarAcelerador;
            ruedaTraseraDerecha.brakeTorque = frenoMotorAlSoltarAcelerador;
        }
    }

    private void LimpiarFrenos()
    {
        ruedaDelanteraIzquierda.brakeTorque = 0f;
        ruedaDelanteraDerecha.brakeTorque = 0f;
        ruedaTraseraIzquierda.brakeTorque = 0f;
        ruedaTraseraDerecha.brakeTorque = 0f;
    }

    private void ProcesarDireccion()
    {
        // A mayor velocidad, menor es el ángulo máximo de giro para mantener el control
        float anguloPermitido = Mathf.Lerp(anguloMaximoGiro, anguloMinimoGiroAltaVelocidad, porcentajeVelocidadMax);
        anguloGiroActual = entradaDireccion * anguloPermitido;

        ruedaDelanteraIzquierda.steerAngle = anguloGiroActual;
        ruedaDelanteraDerecha.steerAngle = anguloGiroActual;
    }

    private void ProcesarAyudaDeGiro()
    {
        bool girando = Mathf.Abs(entradaDireccion) > 0.05f;
        float amortiguacion = girando ? amortiguacionRotacionGirando : amortiguacionRotacionReposo;
        Vector3 velocidadAngular = cuerpoFisicoAuto.angularVelocity;
        velocidadAngular.y = Mathf.Lerp(velocidadAngular.y, 0f, amortiguacion * Time.fixedDeltaTime);
        cuerpoFisicoAuto.angularVelocity = velocidadAngular;

        float kmh = Mathf.Abs(velocidadActualKMH);
        if (kmh < velocidadMinimaParaAyudaGiro) return;

        float factorVelocidad = Mathf.Clamp01(kmh / velocidadMaximaAdelante);
        float multiplicadorAyuda = Mathf.Sin(factorVelocidad * Mathf.PI);
        float signoDireccion = velocidadActualKMH < 0f ? -1f : 1f;

        cuerpoFisicoAuto.AddTorque(Vector3.up * entradaDireccion * fuerzaAyudaGiro * multiplicadorAyuda * signoDireccion, ForceMode.Acceleration);
    }

    private void ProcesarAmortiguacionLateral()
    {
        if (EnElAire()) return;
        Vector3 velocidadLocal = cuerpoFisicoAuto.transform.InverseTransformDirection(cuerpoFisicoAuto.linearVelocity);
        velocidadLocal.x *= amortiguacionVelocidadLateral;
        cuerpoFisicoAuto.linearVelocity = cuerpoFisicoAuto.transform.TransformDirection(velocidadLocal);
    }

    private void SincronizarMallasRuedas()
    {
        ActualizarMallaRueda(ruedaDelanteraIzquierda, mallaDelanteraIzquierda, 0);
        ActualizarMallaRueda(ruedaDelanteraDerecha, mallaDelanteraDerecha, 1);
        ActualizarMallaRueda(ruedaTraseraIzquierda, mallaTraseraIzquierda, 2);
        ActualizarMallaRueda(ruedaTraseraDerecha, mallaTraseraDerecha, 3);
    }

    private void ActualizarMallaRueda(WheelCollider col, Transform malla3D, int indice)
    {
        if (malla3D == null || col == null) return;
        col.GetWorldPose(out Vector3 posicionMundo, out Quaternion rotacionMundo);
        Transform padre = malla3D.parent;
        if (padre != null)
        {
            malla3D.localPosition = padre.InverseTransformPoint(posicionMundo);
            Quaternion rotacionPadre = Quaternion.Inverse(padre.rotation);
            Quaternion deltaRotacion = rotacionMundo * Quaternion.Inverse(col.transform.rotation);
            malla3D.localRotation = rotacionesInicialesMallas[indice] * (rotacionPadre * deltaRotacion * padre.rotation);
        }
        else
        {
            malla3D.SetPositionAndRotation(posicionMundo, rotacionMundo);
        }
    }

    public bool EnElAire()
    {
        if (!ruedaDelanteraIzquierda.GetGroundHit(out _)) return true;
        if (!ruedaDelanteraDerecha.GetGroundHit(out _)) return true;
        if (!ruedaTraseraIzquierda.GetGroundHit(out _)) return true;
        if (!ruedaTraseraDerecha.GetGroundHit(out _)) return true;
        return false;
    }

    // Aplica un empujón de velocidad (al recolectar un Turbo)
    public void ApplyBoost(float fuerzaTurbo)
    {
        cuerpoFisicoAuto.AddForce(cuerpoFisicoAuto.transform.forward * fuerzaTurbo, ForceMode.VelocityChange);
    }
}