# Food Nutrition App

**Author:** Rong Xiao  
**Module:** 6G6Z0014 – Mobile Computing  
**Assessment:** Developing a Cross-Platform Mobile App  

A cross-platform .NET MAUI application on the **Food and Drink** theme. Users can photograph food for simulated recognition, manually search nutrition data, listen to results via text-to-speech, and review saved history in SQLite.

## Features

| Feature | Description |
|---------|-------------|
| Photo recognition | Camera capture with mock OCR / USDA API fallback |
| Manual search | Text search with input validation |
| Voice input | Microphone speech-to-text on search page |
| Auto flash | Ambient light sensor triggers flashlight in low light |
| Vibration | Haptic feedback when scan succeeds |
| Text-to-speech | Read nutrition facts aloud (WCAG accessibility) |
| History | SQLite local storage of scan/search records |
| Help page | WCAG 2 accessibility documentation |

## Hardware Used

1. **Camera** – food photo capture  
2. **Microphone** – voice food name input  
3. **Flash** – automatic torch in dark environments  
4. **Vibration** – success feedback  
5. **Text-to-Speech** – nutrition readout  
6. **Ambient light sensor** – low-light detection (Android)

## Project Structure

```
FoodNutritionApp/
├── Models/           FoodItem, HistoryRecord
├── Services/         API, SQLite, light sensor
├── ViewModels/       MVVM logic
├── Views/            XAML pages
├── Converters/       Value converters
└── Platforms/        Android permissions & sensors
```

## Development Plan

- [x] MVVM shell navigation (Main, Camera, Search, Detail, History, Help)
- [x] Mock + USDA nutrition API with network fallback
- [x] SQLite history persistence
- [x] Camera, flash, vibration, TTS, speech-to-text integration
- [x] Accessibility labels and help documentation
- [ ] Deploy to Android phone emulator
- [ ] Deploy to Android tablet emulator
- [ ] Record 10–15 minute screencast for Moodle/Xuexitong

## Requirements

- Visual Studio 2022 (17.8+) with **.NET Multi-platform App UI** workload
- .NET 10 SDK
- Android SDK (for emulator deployment)
- Windows developer mode (for Windows deployment)

## How to Run

1. Open `FoodNutritionApp.csproj` in Visual Studio.
2. Select **Android Emulator** or **Windows Machine** as the target.
3. Press **F5** to build and run.
4. Grant camera and microphone permissions when prompted.

## Deployment (Assessment)

For full deployment marks, demonstrate the app on:

1. Android phone emulator (e.g. Pixel 5 API 33)  
2. Android tablet emulator (e.g. Nexus 10 / Pixel C)  
3. Optional: Windows Machine target  

## GitHub Commits

Commit regularly with descriptive messages, for example:

- `feat: add camera service with flash auto-enable`
- `feat: add SQLite history storage`
- `feat: add speech-to-text search input`
- `fix: improve validation error messages`

## Screencast Checklist

Explain in your video:

1. UI/UX and WCAG accessibility features  
2. Each hardware feature (show flash/vibration in code if using emulator)  
3. Search validation and network error handling  
4. Deployment on phone + tablet  
5. GitHub commit history and code structure  

## License

Academic coursework – Manchester Metropolitan University.
