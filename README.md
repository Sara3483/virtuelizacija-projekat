# VirtuelizacijaProjekat - Baterija

Analiza i upravljanje podacima o stanju napunjenosti Li-ion baterija 
koriscenjem WCF servisa i manipulacije fajlovima

## Opis

Projekat implementira klijent-servis sistem za obradu podataka o baterijama

Klijent:
- ucitava CSV fajl
- salje podatke sekvencijalno (redom) tako sto prolazi kroz for petlju

Servis:
- vrsi validaciju
- obradjuje podatke
- cuva podatke na disku
- vraca ACK/NACK i status(IN_PROGRESS ili COMPLETED)

## Protokol komunikacije
- StartSession(EisMeta)
- PushSample(EisSample)
- EndSession()

## Meta podaci
- BatteryId
- TestId
- SoCPercentage
- FileName
- TotalRows

## Sample podaci
- RowIndex
- FrequencyHz
- R_ohm
- X_ohm
- T_degC
- Range_ohm
- TimestampLocal

