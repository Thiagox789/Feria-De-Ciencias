/*
 * PROYECTO FERIA DE CIENCIAS - VOLANTE + PANTALLA OLED 0.91" (128x32 I2C)
 * Librerías: Adafruit_GFX y Adafruit_SSD1306
 *
 * Conexión Pantalla OLED 0.91" (I2C):
 *   - VCC -> 5V (o 3.3V)
 *   - GND -> GND
 *   - SCL -> Pin A5 (Arduino Uno)
 *   - SDA -> Pin A4 (Arduino Uno)
 *
 * Controles:
 *   - Volante: Potenciómetro en pin A0
 *   - Acelerador: Pulsador a GND en pin 2 (INPUT_PULLUP)
 *   - Freno: Pulsador a GND en pin 3 (INPUT_PULLUP)
 */

#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 32
#define OLED_RESET -1
#define SCREEN_ADDRESS 0x3C // Si no enciende, probar con 0x3D

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

// Tiempos
unsigned long tiempoAnteriorLectura = 0;
const long intervaloLectura = 20; // 50 Hz para inputs

// Calibración volante
int maximoCalibrado = 500;

// Datos recibidos por telemetría desde Unity (#VEL,TIEMPO_STR,CHK,TOT_CHK)
int velKmh = 0;
char strTiempo[16] = "00:00.000";
int chkActual = 0;
int chkTotales = 0;

// Buffer serie
char bufferSerial[64];
byte bufferIdx = 0;

void procesarPaqueteTelemetria(char* datos) {
  // Formato: #128,01:23.456,3,5
  if (datos[0] != '#') return;

  char* token = strtok(datos + 1, ",");
  if (token != NULL) velKmh = atoi(token);

  token = strtok(NULL, ",");
  if (token != NULL) {
    strncpy(strTiempo, token, sizeof(strTiempo) - 1);
    strTiempo[sizeof(strTiempo) - 1] = '\0';
  }

  token = strtok(NULL, ",");
  if (token != NULL) chkActual = atoi(token);

  token = strtok(NULL, ",");
  if (token != NULL) chkTotales = atoi(token);
}

void leerTelemetriaSerial() {
  while (Serial.available() > 0) {
    char c = Serial.read();
    if (c == '\n' || c == '\r') {
      if (bufferIdx > 0) {
        bufferSerial[bufferIdx] = '\0';
        procesarPaqueteTelemetria(bufferSerial);
        bufferIdx = 0;
      }
    } else {
      if (bufferIdx < sizeof(bufferSerial) - 1) {
        bufferSerial[bufferIdx++] = c;
      }
    }
  }
}

void dibujarPantalla() {
  display.clearDisplay();
  display.setTextColor(SSD1306_WHITE);

  // ================= FILA SUPERIOR: VELOCIDAD Y CHECKPOINTS =================
  // 1. Número de velocidad GRANDE (Tamaño 2)
  display.setTextSize(2);
  display.setCursor(0, 0);
  display.print(velKmh);

  // 2. Unidad "KM/H" (Tamaño 1 alineado abajo del número)
  display.setTextSize(1);
  display.setCursor(display.getCursorX() + 2, 7);
  display.print("KM/H");

  // 3. Checkpoints a la derecha en la fila superior (ej: CP 3/5)
  display.setCursor(80, 5);
  display.print("CP ");
  display.print(chkActual);
  display.print("/");
  display.print(chkTotales);

  // Línea divisoria muy sutil entre velocidad y tiempo
  display.drawFastHLine(0, 16, 128, SSD1306_WHITE);

  // ================= FILA INFERIOR: TIEMPO MÁXIMO PROTAGONISTA =================
  // El tiempo con milisegundos "00:00.000" centrado en Tamaño 1 espaciado / nítido
  display.setTextSize(1);
  display.setCursor(4, 21);
  display.print("TIME  ");
  display.print(strTiempo);

  display.display();
}

void setup() {
  pinMode(A0, INPUT);
  pinMode(2, INPUT_PULLUP);
  pinMode(3, INPUT_PULLUP);
  Serial.begin(9600);

  // Iniciar pantalla OLED I2C
  display.begin(SSD1306_SWITCHCAPVCC, SCREEN_ADDRESS);

  display.clearDisplay();
  display.display();
}

void loop() {
  // 1. Recibir telemetría desde Unity
  leerTelemetriaSerial();

  // 2. Leer controles y enviar a Unity cada 20 ms (50 Hz)
  unsigned long tiempoActual = millis();
  if (tiempoActual - tiempoAnteriorLectura >= intervaloLectura) {
    tiempoAnteriorLectura = tiempoActual;

    int lecturaReal = analogRead(A0);

    // Auto-calibración de giro
    if (lecturaReal > maximoCalibrado) {
      maximoCalibrado = lecturaReal;
    }

    int lecturaLimitada = constrain(lecturaReal, 0, maximoCalibrado);
    int x = map(lecturaLimitada, 0, maximoCalibrado, -100, 100);

    int acel = (digitalRead(2) == LOW) ? 1 : 0;
    int freno = (digitalRead(3) == LOW) ? 1 : 0;

    Serial.print("Volante: ");
    Serial.print(x);
    Serial.print(" | Max Detectado: ");
    Serial.print(maximoCalibrado);
    Serial.print(" | Acel: ");
    Serial.print(acel);
    Serial.print(" Freno: ");
    Serial.println(freno);

    // 3. Dibujar en la pantalla OLED
    dibujarPantalla();
  }
}