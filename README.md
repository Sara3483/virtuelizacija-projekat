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

## WCF konfig

Servis koristi:
- netTcpBinding
- streamed transfer mode
- podesene timeout vrednosti 
- MaxRecievedMessagesSize konfig

## Validacija i fault handling

Implementirani faults:
- validation fault
- data format fault

Validacije:
- monotoni rast za row index
- provera za frequency > 0
- R_ohm i T_degC u opsegu realnih vrednosti
- validacija null podataka

## Upravljanje resursima

Implementiran Dispose pattern sa FileStream, StreamWriter, StreamReader i IDisposable klasama

## Rad sa fajlovima

- DirectoryInfo i FileInfo
- rekurzivni prolaz kroz fajlove
- parsiranje preko CultureInfo.InvariantCulture

## Provera za greske

- kreiran novi fajl B12 po sabolnu baze
- prekopiran nasumicno izabran validan .csv fajl radi lakseg testiranja
- u Test_1 se nalazi .csv fajl sa nepotpunim podacima 
- u Test_2 se nalazi .csv fajl sa brojem redova vecim od 39
- greske se loguju u invalid_rows.log