# Food Nutrition App

**Author:** Wenyi He  
**Assessment:** Developing a Cross-Platform Mobile App  
**Hand-in deadline:** 3 June 2026  

A cross-platform **.NET MAUI** app on the **Food and Drink** theme. Users can search or photograph food for nutrition data, listen to results with text-to-speech, manage a local food list (SQLite CRUD), and use device hardware such as camera, GPS, and microphone.

---

## App Pages

| Page | Purpose |
|------|---------|
| **Main** | Home navigation hub |
| **Search** | Manual food search, validation, voice input (microphone) |
| **Camera** | Photo capture, simulated OCR recognition, auto-flash |
| **Detail** | Nutrition facts, TTS read-aloud with stop button |
| **Food List (History)** | Category filter, pull-to-refresh, swipe Edit/Delete |
| **Edit Record** | Update saved food entries with validation |
| **Location** | GPS latitude/longitude and reverse geocoding address |
| **Settings** | Light / Dark / Follow System theme, font size scale |
| **Help** | WCAG 2 accessibility and hardware feature documentation |

---

## Features

| Feature | Description |
|---------|-------------|
| Photo recognition | Camera capture with local + mock OCR simulation |
| Manual search | Text search with input validation and error handling |
| Voice input | Microphone speech-to-text on the search page |
| Data fallback chain | Local JSON → USDA API → Mock API (see below) |
| Auto flash | Ambient light sensor triggers flashlight in low light (Android) |
| Vibration | Haptic feedback when a photo scan succeeds |
| Text-to-speech | Read nutrition facts aloud; separate **Stop Reading** button |
| Food list CRUD | SQLite: Create (search/camera), Read, Update (Edit), Delete |
| Category filter | Filter list by Fruit, Meat, Drink, Grain, etc. |
| Pull to refresh | Refresh food list on the History page |
| Themes | Light, Dark, or Follow System (default) |
| Accessibility | Large fonts, screen reader labels, Help page (WCAG 2) |

---

## Data Strategy (Dual Insurance)

Nutrition data is loaded through `HybridNutritionApi` in this order:

1. **Local Database** — bundled `Resources/Raw/foods.json` (works offline, most reliable for demos)
2. **USDA API** — FoodData Central remote search when online (`Source: USDA API`)
3. **Mock API** — `MockNutritionApi` with predefined foods and simulated delay (`Source: Mock API`)

Photo recognition uses local data first, then **Mock API Camera** (simulated OCR from image hash).

Check the **Source** field on the Detail page during demos to show which layer returned the result.

### Verify Mock API still works

1. Turn off Wi-Fi / mobile data (or use aeroplane mode on a device).
2. Search for a food **not** in local JSON, e.g. `noodles` or `tofu`.
3. Confirm results appear with `Source: Mock API`.

---

## Hardware Used

| # | Hardware | Where used |
|---|----------|------------|
| 1 | **Camera** | Photo food recognition |
| 2 | **Microphone** | Voice search input |
| 3 | **GPS / Location** | Latitude, longitude, address on Location page |
| 4 | **Flash** | Auto-enabled in low light when taking photos |
| 5 | **Vibration** | Success feedback after photo scan |
| 6 | **Text-to-Speech** | Read nutrition aloud on Detail page |
| 7 | **Ambient light sensor** | Low-light detection before flash (Android) |

---

## Project Structure

```
FoodNutritionApp/
├── Models/              FoodItem, HistoryRecord, FoodCategories
├── Services/
│   ├── INutritionApi.cs
│   ├── HybridNutritionApi.cs    ← local → USDA → mock chain
│   ├── LocalFoodDataService.cs
│   ├── MockNutritionApi.cs
│   ├── RealNutritionApi.cs
│   ├── DatabaseService.cs
│   ├── AppSettingsService.cs
│   └── ISpeechPlaybackService.cs (+ Windows / MAUI implementations)
├── ViewModels/          MVVM logic (CommunityToolkit.Mvvm)
├── Views/               XAML pages
├── Converters/
├── Resources/Raw/       foods.json (local bundled data)
└── Platforms/           Android (permissions, light sensor), Windows (TTS)
```

---

## Development Plan

### Core (done)

- [x] MVVM Shell navigation and all main pages
- [x] Hybrid data chain: local JSON → USDA → Mock API
- [x] SQLite history with CRUD (Create via search/camera, Edit, Delete, Clear)
- [x] Camera, flash, vibration, TTS (with stop), speech-to-text, GPS location
- [x] Search validation, category filter, pull-to-refresh
- [x] Light / Dark / Follow System theme and font scaling
- [x] Accessibility labels and Help page (WCAG 2)

### Submission & deployment (deadline: 3 June 2026)

- [ ] Push regular commits to GitHub Classroom repository
- [ ] Deploy and demo on Android **phone** emulator or device
- [ ] Deploy and demo on Android **tablet** emulator (or Windows + Android)
- [ ] Record 10–15 minute screencast for Moodle / MMUtube

### Four-part submission schedule (deadline: 3 June 2026)

| Part | Focus | What to show |
|------|-------|--------------|
| **1** | Main UI framework | Home, list, detail, navigation |
| **2** | Hardware features | Camera, TTS, location, vibration, microphone |
| **3** | UI/UX optimisation | Themes, categories, refresh, accessibility |
| **4** | Bug fixes & polish | TTS stop, validation, data fallback, final demo |

---

## Requirements

- Visual Studio 2022 (17.8+) with **.NET Multi-platform App UI** workload
- .NET 10 SDK
- Android SDK (for emulator / device deployment)
- Windows developer mode (optional, for Windows Machine target)

---

## How to Run

1. Open `FoodNutritionApp.slnx` (or `FoodNutritionApp.csproj`) in Visual Studio.
2. Select **Android Emulator**, **Android Device**, or **Windows Machine** as the target.
3. Press **F5** to build and run.
4. Grant **camera**, **microphone**, and **location** permissions when prompted.

---

## Deployment (Assessment)

For full deployment marks, demonstrate the app on:

1. **Android phone** emulator or physical device (e.g. Pixel 5 API 33)
2. **Android tablet** emulator (e.g. Pixel C / Nexus 10) **or** Windows + Android
3. Optional: **Windows Machine** target for lab development

Hardware features should behave consistently across demonstrated platforms where supported.

---

## GitHub Usage

Commit regularly with clear messages before the deadline. Example messages:

```
feat: add HybridNutritionApi local-USDA-mock fallback chain
feat: add SQLite food list with swipe edit and delete
feat: add GPS location page with geocoding
feat: add TTS stop button and Windows speech service
fix: resolve SQLite category migration on upgrade
docs: update README for four-part submission plan
```

The repository should include this README with author name, app overview, development plan, and run instructions.

---

## Screencast Checklist (10–15 minutes)

Walk through each marking criterion clearly — tutors may not run the code; the video is the main marking source.

1. **UI/UX & accessibility** — themes, font size, Help page, WCAG labels  
2. **Hardware** — camera, TTS start/stop, location (lat/long), microphone, vibration, flash (slow, deliberate demos)  
3. **Functionality** — search, photo, list, detail, navigation  
4. **Validation & errors** — empty search, invalid edit fields, network / offline fallback  
5. **Data & CRUD** — show `Source` field; Create, Edit, Delete, pull-to-refresh  
6. **Deployment** — phone + tablet (or Android + Windows)  
7. **Code & GitHub** — MVVM structure, `HybridNutritionApi`, commit history  

---

## License

Academic coursework — Manchester Metropolitan University.
