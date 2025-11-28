# MusicbeeTagEasily

[![Version](https://img.shields.io/badge/version-v2.0-blue.svg)](https://github.com/Stargazer-cc/MusicbeeTagEasily/releases)
[![MusicBee](https://img.shields.io/badge/MusicBee-3.0%2B-orange.svg)](https://getmusicbee.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Language](https://img.shields.io/badge/language-C%23-purple.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)

**[中文文档](README_CN.md) | English**

A MusicBee plugin for quickly browsing and applying existing tag values from your music library.

---

## ✨ Features

- **Dynamic Field Scanning**: Automatically scans all available tag fields without manual configuration
- **Custom Display Order**: Adjust field display order through a dual-list interface
- **Column-based Layout**: All fields displayed side-by-side in one interface for easy viewing
- **Real-time Track Monitoring**: Automatically detects track selection changes in MusicBee
- **One-click Application**: Double-click any tag value to apply it to selected tracks

---

## 📦 Installation

1. Download the `mb_MusicbeeTagEasily.dll` file
2. Open MusicBee Preferences → Plugins tab → Click "Add Plugin" → Select the downloaded `mb_MusicbeeTagEasily.dll` file

---

## 🚀 Usage

1. **Configure Fields**: `Edit` → `Preferences` → `Plugins` → Select plugin → `Configure`
   - Left panel: Available fields
   - Right panel: Selected fields (top-to-bottom corresponds to left-to-right display)
   - Use `>>` / `<<` to add/remove fields
   - Use `Move Up` / `Move Down` to adjust order

2. **Open Tool**: `Tools` → `MusicbeeTagEasily`

3. **Apply Tags**:
   - Select tracks in MusicBee
   - Double-click the desired tag value in the tool window

---

## 📁 Project Structure

```
├── Plugin.cs                    # Plugin entry point
├── MusicBeeInterface.cs         # MusicBee API interface
├── TagBrowserForm.cs            # Tag browser interface
├── SettingsForm.cs              # Settings interface
├── build.bat                    # Build script
└── readme.md                    # Documentation
```

---

## 📝 Version History

**v2.0** (2025-11-28)
- Dynamic scanning of all available fields
- Support for custom field display order
- Optimized scanning performance (batch API)
- Refactored settings interface

**v1.x**
- Initial implementation

---

## 📄 License

[MIT License](LICENSE)

---

## 🌐 Language Support

This plugin supports both English and Chinese interfaces:
- **English** (Default)
- **中文** (Chinese)

For Chinese documentation, please refer to [README_CN.md](README_CN.md).
