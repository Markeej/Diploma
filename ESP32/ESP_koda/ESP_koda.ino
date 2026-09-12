#include <HardwareSerial.h>
#include <TinyGPSPlus.h>

HardwareSerial GPSSerial(2);
HardwareSerial SIMSerial(1);

TinyGPSPlus gps;

// GPS pini na ESP32
const int GPS_RX = 16;
const int GPS_TX = 17;

// SIM800L pini na ESP32
const int SIM_RX = 18;
const int SIM_TX = 19;

// API
const char* API_URL = "API-URL";

const char* DEVICE_ID = "DEVICE-ID";

const char* DEVICE_SECRET = "DEVICE-SECRET";

// Interval pošiljanja: 2 minuti
const unsigned long INTERVAL = 120000;

unsigned long zadnjePosiljanje = 0;


void posljiUkaz(String ukaz, int cakanje = 1000)
{
    SIMSerial.println(ukaz);
    delay(cakanje);

    while (SIMSerial.available())
    {
        Serial.write(SIMSerial.read());
    }
}


void vzpostaviGprs()
{
    posljiUkaz("AT");

    posljiUkaz("AT+SAPBR=3,1,\"CONTYPE\",\"GPRS\"");

    posljiUkaz("AT+SAPBR=3,1,\"APN\",\"internet\"");

    posljiUkaz("AT+SAPBR=3,1,\"USER\",\"mobitel\"");

    posljiUkaz("AT+SAPBR=3,1,\"PWD\",\"internet\"");

    posljiUkaz("AT+SAPBR=1,1", 5000);

    Serial.println("GPRS povezava vzpostavljena.");
}


void posljiLokacijo(double latitude, double longitude)
{
    String json = "{\"deviceId\":\"" + String(DEVICE_ID) + "\",\"deviceSecret\":\"" + String(DEVICE_SECRET) + "\",\"latitude\":" + String(latitude, 6) + ",\"longitude\":" + String(longitude, 6) +"}";

    Serial.println();
    Serial.println("Posiljam lokacijo:");
    Serial.println(json);

   
    posljiUkaz("AT+HTTPINIT");
    posljiUkaz("AT+HTTPPARA=\"CID\",1");

    posljiUkaz("AT+HTTPPARA=\"URL\",\"" +String(API_URL) + "\"");
    posljiUkaz("AT+HTTPPARA=\"CONTENT\",\"application/json\"");
    SIMSerial.println("AT+HTTPDATA=" + String(json.length()) +",10000");

    delay(1500);
    SIMSerial.print(json);
    delay(2000);
    
    posljiUkaz("AT+HTTPACTION=1",12000);
    posljiUkaz("AT+HTTPREAD",3000);

    posljiUkaz("AT+HTTPTERM");

    Serial.println("Lokacija poslana.");
}


void setup()
{
    Serial.begin(115200);

    // UART za GPS
    GPSSerial.begin(
        9600,
        SERIAL_8N1,
        GPS_RX,
        GPS_TX
    );

    // UART za SIM800L
    SIMSerial.begin(
        9600,
        SERIAL_8N1,
        SIM_RX,
        SIM_TX
    );

    delay(3000);

    Serial.println();
    Serial.println("E-ZIVALI GPS TRACKER");

    Serial.println("Vzpostavljam GPRS povezavo...");

    // GPRS odpremo samo enkrat
    vzpostaviGprs();

    Serial.println("Cakam na GPS lokacijo...");
}


void loop()
{
   
    while (GPSSerial.available())
    {
        gps.encode(GPSSerial.read());
    }

    if (gps.location.isValid() && (zadnjePosiljanje == 0 || millis() - zadnjePosiljanje >= INTERVAL))
    {
        double latitude = gps.location.lat();

        double longitude = gps.location.lng();

        Serial.println();
        Serial.println("GPS lokacija pridobljena.");

        Serial.print("Latitude: ");
        Serial.println(latitude, 6);

        Serial.print("Longitude: ");
        Serial.println(longitude, 6);

        posljiLokacijo(latitude, longitude);

        zadnjePosiljanje = millis();
    }

    delay(10);
}