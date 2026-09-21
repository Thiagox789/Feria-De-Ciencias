/*
 * PROYECTO FERIA DE CIENCIAS - VOLANTE CON CALIBRACIÓN AUTOMÁTICA
 */

unsigned long tiempoAnterior = 0; 
const long intervalo = 20; 

// Iniciamos el máximo en un valor base conservador. 
// Apenas gires el volante al tope, este número se actualizará solo.
int maximoCalibrado = 500; 

void setup() {
  pinMode(A0, INPUT);       
  pinMode(2, INPUT_PULLUP); 
  pinMode(3, INPUT_PULLUP); 
  Serial.begin(9600);       
}

void loop() {
  unsigned long tiempoActual = millis();

  if (tiempoActual - tiempoAnterior >= intervalo) {
    tiempoAnterior = tiempoActual;

    int lecturaReal = analogRead(A0);

    // AUTO-CALIBRACIÓN: Si la lectura actual supera nuestro máximo guardado,
    // actualizamos el tope dinámicamente.
    if (lecturaReal > maximoCalibrado) {
      maximoCalibrado = lecturaReal;
    }

    // Usamos la variable 'maximoCalibrado' en lugar del número fijo 530
    int lecturaLimitada = constrain(lecturaReal, 0, maximoCalibrado);

    // Convertimos el rango dinámico en la salida del volante (-100 a 100)
    int x = map(lecturaLimitada, 0, maximoCalibrado, -100, 100);

    int acel = (digitalRead(2) == LOW) ? 1 : 0;
    int freno = (digitalRead(3) == LOW) ? 1 : 0;

    Serial.print("Volante: ");
    Serial.print(x); 
    Serial.print(" | Max Detectado: "); // Te agrego esto para que veas en el monitor cómo se calibra solo
    Serial.print(maximoCalibrado);
    Serial.print(" | Acel: ");
    Serial.print(acel);
    Serial.print(" Freno: ");
    Serial.println(freno);
  }
}