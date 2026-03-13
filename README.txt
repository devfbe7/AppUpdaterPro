# 💻 App Updater Pro

Ein modernes WPF-Tool zum Verwalten und Aktualisieren von Windows-Programmen über Winget.

## ✨ Features

- 📋 Installierte Programme anzeigen
- ⬆️ Verfügbare Updates anzeigen
- ⟳ Alle Programme auf einmal aktualisieren
- 🔍 Gefilterte Updates für bestimmte Programme
- 🔄 Auto-Refresh alle 60 Sekunden
- 📁 Ausgabe als Textdatei exportieren
- 📝 Automatisches Logging aller Aktionen

## 🛠️ Voraussetzungen

- Windows 10 / 11
- [Winget](https://github.com/microsoft/winget-cli) (vorinstalliert ab Windows 11)
- .NET 6 oder höher

## 🚀 Installation

1. Repository klonen:
```bash
   git clone https://github.com/fbe777/App-Updater-Pro.git
```
2. Projekt in Rider oder Visual Studio öffnen
3. Bauen und starten (`Strg + F5`)

## 📖 Winget Befehle Übersicht

| Befehl | Beschreibung |
|--------|-------------|
| `list` | Installierte Programme anzeigen |
| `upgrade` | Updates anzeigen |
| `upgrade --all` | Alle Updates installieren |
| `upgrade --query <Name>` | Bestimmtes Programm updaten |
| `install <Name>` | Programm installieren |
| `uninstall <Name>` | Programm deinstallieren |

## 🤝 Contributing

Beiträge sind willkommen! Bitte lies zuerst [CONTRIBUTING.md](CONTRIBUTING.md).

## 📄 Lizenz

Dieses Projekt steht unter der [MIT License](LICENSE).
