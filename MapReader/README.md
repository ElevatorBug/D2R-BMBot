# Eigenständiger Kartenbetrachter

`MapReader.csproj` baut **D2R-MapReader.exe** für Windows x64 / .NET Framework 4.7.2 oder neuer. Dieses Projekt benötigt weder den Bot-Build noch seine Einstellungen. Es enthält keine Kampf-, Loot-, Bewegungs-, Tastatur- oder Mausroutinen.

Die ursprüngliche Kartenfunktion liest den Seed aus D2R und rekonstruiert die Karte mit `map.exe` und einem vorhandenen Diablo II: LoD 1.13c-Datenordner. Die Kartenfelder werden nicht vollständig direkt aus dem D2R-Speicher gelesen. Die Anwendung zeigt die ursprünglichen Kartendaten ohne die Bot-spezifischen Wegkorrekturen an.

## Benutzung

1. `D2R-MapReader.exe` starten. `map.exe`, `Newtonsoft.Json.dll`, `D2R-MapReader.exe.config` und `demo-map.jsonl` gehören daneben.
2. **Demo öffnen** lädt eine ausdrücklich synthetische Testkarte. **Dump öffnen** liest vorhandene `DumpMap.txt`- oder JSONL-Dateien. Beide Funktionen brauchen keinen Spielprozess und keinen LoD-Ordner.
3. Für einen bekannten Seed: LoD 1.13c-Ordner wählen, Seed als Dezimalzahl eingeben, Schwierigkeit wählen und **Karte erzeugen** anklicken.
4. Für das Lesen aus D2R: Prozess-ID von `D2R.exe` im Windows-Task-Manager unter „Details“ nachsehen, exakten Charakternamen eingeben und **Spieldaten lesen** anklicken. Das liest einmalig Seed, Schwierigkeit, Gebiet und Position. Anschließend **Karte erzeugen** anklicken. Es gibt kein automatisches Polling.
5. Im Gebietsmenü eine Karte wählen. Grün = begehbar, Orange = Ausgang, Violett = NPC, Weiß = Objekt, Blau = zuletzt gelesene Spielerposition. **Dump speichern** exportiert die Generator-Ausgabe, **PNG speichern** das gewählte Gebiet einschließlich Markierungen.

Ein importierter Dump hat keine Spielerposition. Eine manuelle Änderung von Seed oder Schwierigkeit entfernt die Zuordnung zum zuletzt gelesenen Spieler. Die Position ist eine Momentaufnahme; sie folgt keiner Bewegung.

## Aufbau

| Datei | Aufgabe |
| --- | --- |
| `MapModels.cs` | Gemeinsame JSON-Modelle; historischer Typname `MapAreaStruc` bleibt zur Quellkompatibilität erhalten. Dieser Partial enthält keine Bot-Abhängigkeit. |
| `MapReaderCore.cs` | Map-Generator mit Timeout, getrennten Ausgabekanälen und korrekt zitiertem LoD-Pfad; JSONL-Parser und RLE-Decodierung. |
| `GameMapContext.cs` | Eigenständiger Kontextleser mit exakten Speicherzugriffen und einmaligem Windows-Prozesszugriff. |
| `MapViewer.cs` | Oberfläche, Kartenanzeige und Export. |
| `../Core/ProcessMemoryReader.cs` | Gemeinsame Schnittstelle und nativer Windows-Lesezugriff. |

Der Kontextleser übernimmt das Unit-Table-Muster und die Feld-Offsets aus dem bisherigen `PlayerScan`. Er liest den ursprünglichen Quick-Scan-Bereich von 644 Zeigern und wählt den Charakter über seinen Namen. Er enthält keinen allgemeinen Scan aller Einheiten. Nicht gefundene/mehrdeutige Muster, unvollständige Lesezugriffe und erkannte Gebietswechsel erzeugen einen Fehler statt scheinbar gültiger Daten. Die doppelte Prüfung relevanter Felder reduziert Übergangsfehler, ist jedoch keine atomare Momentaufnahme des Spielprozesses.

Die Seed-Umkehrung verwendet das multiplikative Inverse des bisherigen Hash-Multiplikators modulo 2^32 statt einer Suchschleife. Karten-Dimensionen und RLE-Längen werden vor dem Anlegen des Rasters geprüft. Fehlende Kartenzeilen bleiben gesperrt.

Der Windows-Prozess wird nur mit `PROCESS_QUERY_INFORMATION | PROCESS_VM_READ` geöffnet und anschließend geschlossen. Das separate Projekt enthält keinen Prozess-Schreibzugriff und keine Injection. Diese Trennung bestätigt weder die Aktualität der Spiel-Offsets noch eine Nicht-Erkennung durch Warden.

## Bauen und prüfen

In einer Visual-Studio Developer Command Prompt mit dem .NET Framework 4.7.2 Developer Pack:

```bat
msbuild MapReader\MapReader.csproj /t:Rebuild /p:Configuration=Release
msbuild tests\MapReaderTests.csproj /t:Rebuild /p:Configuration=Release
tests\bin\map-reader\MapReaderTests.exe
```

Der erste Befehl baut ausschließlich den Kartenbetrachter. Dafür werden keine NuGet-Pakete des Bots benötigt; `Newtonsoft.Json.dll` und `map.exe` sind bereits im Repository vorhanden. `map.exe` bleibt das unveränderte bestehende Backend; LoD-Spieldateien sind nicht enthalten.

Validierung hier: beide eigenständigen Projekte mit Mono und .NET-4.7.2-Referenzen gebaut; **12 Kartenleser-Tests und 11 vorhandene Runtime-Tests bestanden**. Alle C#-Quellen des bisherigen Bots einschließlich des gemeinsamen Kartenkerns kompiliert; dabei wurde für die Linux-Prüfung die Framework-Referenz für `System.Net.Http` verwendet. Vollständiger Windows/MSBuild-Bot-Build samt Ressourcen/Fody, Windows-Oberfläche, echtes `map.exe` mit LoD-Daten und aktueller D2R-Speicherzugriff wurden hier nicht ausgeführt. Der Mono-Test des Generatorprozesses benutzt ein kontrolliertes Testprogramm, nicht das LoD-Backend.
