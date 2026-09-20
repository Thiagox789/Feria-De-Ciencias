/*
 * PROYECTO FERIA DE CIENCIAS - VOLANTE SIMPLIFICADO DE ARDUINO UNO
 */

void setup() {
  pinMode(A0, INPUT);   // Potenciómetro del volante
  pinMode(2, INPUT_PULLUP); // Botón Acelerador (opcional)
  pinMode(3, INPUT_PULLUP); // Botón Freno (opcional)
  Serial.begin(9600);
}

void loop() {
  int x = analogRead(A0);
  Serial.print("Valor: ");
  Serial.println(x);
  delay(10); // Lecturas estables a ~100 veces por segundo
}
