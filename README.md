# osu! Beatmap Splitter

![osu! Beatmap Splitter](screenshots/osu-splitter.png)

A powerful Windows application that allows you to split osu! beatmaps into multiple parts for practice purposes. The application reads beatmap data directly from osu! memory while the game is running, making it incredibly responsive and accurate.

## ✨ Features

- **Real-time Beatmap Detection**: Automatically detects and reads the currently selected beatmap from osu! memory
- **Visual Background Display**: Shows beatmap background images with custom scaling and text overlays
- **Flexible Splitting**: Split beatmaps into 2-100 parts based on hit object count
- **Smart Difficulty Naming**: Automatically creates difficulty names like "Hard Part 1/3"
- **.osz Archive Creation**: Creates importable beatmap archives for osu!
- **Custom UI Design**: Modern dark theme with rounded corners, smooth animations, and SF Pro fonts
- **Memory Reading**: Uses structured memory reading for reliable data extraction

##  Quick Start

### Prerequisites

- **Windows 10/11** (64-bit or 32-bit)
- **osu!** installed and running
- **.NET Framework 4.8** or higher (included with Windows)
- **osu! running** for the application to read beatmap data

### Installation

1. Download the latest release from the [Releases](https://github.com/cfxosu/osu-splitter/releases) page
2. Extract the ZIP file to any folder
3. Run `osu-splitter.exe`
4. Make sure osu! is running in the background

### First Time Setup

1. **Launch the application** while osu! is running
2. **Select a beatmap** in osu!
3. **Adjust the split count** using the slider (2-100 pieces)
4. **Click "Split Beatmap"** to create the split versions

## 📖 How It Works

### Beatmap Detection
The application reads beatmap data directly from osu!'s memory using structured memory reading techniques. This ensures:
- Real-time updates when you change beatmaps
- Accurate hit object counting
- Background image detection and display

### Splitting Process
When you click "Split Beatmap", the application:
1. **Reads the current beatmap** from osu! memory
2. **Parses the .osu file** to extract hit objects
3. **Divides hit objects** evenly across the specified number of parts
4. **Creates new .osu files** with updated difficulty names
5. **Offers to create** a .osz archive for easy import

### Example Output
If you split "Hard" difficulty into 3 parts, you'll get:
- `Song Name [Hard Part 1/3].osu`
- `Song Name [Hard Part 2/3].osu`
- `Song Name [Hard Part 3/3].osu`

## 🎨 User Interface

### Main Features
- **Beatmap Path Display**: Shows the current .osu file path
- **Hit Object Counter**: Displays total hit objects in the current beatmap
- **Background Preview**: Shows beatmap background with artist/title overlay
- **Split Control**: Slider and text input for controlling split count
- **Custom Title Bar**: Drag to move, minimize/close buttons

### Visual Design
- **Dark Theme**: Easy on the eyes during long practice sessions
- **Rounded Corners**: Modern Windows 11 style design
- **Smooth Animations**: Hover effects and transitions
- **Custom Fonts**: SF Pro Display for professional appearance
- **Responsive Layout**: Adapts to window resizing

## 🔧 Technical Details

### Built With
- **Language**: C# (.NET Framework 4.8)
- **UI Framework**: Windows Forms with custom rendering
- **Memory Reading**: Structured memory reading via `OsuMemoryDataProvider`
- **Image Processing**: GDI+ for background image scaling and text overlay
- **File Operations**: System.IO for .osu file parsing and .osz creation

### Project Structure
```
osu-splitter/
├── src/                    # Main application source
│   ├── Form1.cs           # Main UI form
│   ├── Program.cs         # Application entry point
│   └── assets/            # UI assets (fonts, images)
├── OsuMemoryDataProvider/ # Memory reading library
├── ProcessMemoryDataFinder/ # Memory scanning utilities
└── assets/                # Shared assets
```

### Memory Reading
The application uses a sophisticated memory reading system that:
- Scans for osu! process
- Reads structured data from specific memory addresses
- Handles different osu! versions automatically
- Provides real-time beatmap information

## 🙏 Acknowledgments & Credits

### Core Dependencies
- **ProcessMemoryDataFinder** - Memory scanning and process utilities
  - Repository: https://github.com/Piotrekol/ProcessMemoryDataFinder
  - Used for memory reading and process management functionality

### Community & Resources
- **osu-trainer** for inspiration
- **osu! community** for inspiration and testing
- **Memory reading libraries** for enabling real-time data access
- **SF Pro fonts** for the beautiful UI typography
