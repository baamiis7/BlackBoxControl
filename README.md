# Fire Panel Simulation System

A professional WPF-based fire alarm panel simulation and configuration system with comprehensive device management, bus configuration, and cause & effect programming capabilities.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

## 🔥 Features

### Fire Panel Management
- ✅ Complete fire panel configuration
- ✅ Multiple panel support
- ✅ Network configuration (WiFi, Ethernet)
- ✅ Firmware version tracking
- ✅ Zone and loop management

### Loop & Device Management
- ✅ Multi-loop support (configurable)
- ✅ Device palette with visual icons
- ✅ Drag-and-drop device addition
- ✅ Address auto-assignment
- ✅ Device threshold configuration
- ✅ Analog value monitoring
- ✅ Sub-address support

### Bus Configuration
- ✅ RS485/RS232/CAN/Ethernet bus support
- ✅ Bus node management with visual tree
- ✅ Node address auto-assignment
- ✅ Input/Output configuration
- ✅ Bus statistics and monitoring
- ✅ Visual node palette

### Cause & Effect Programming
- ✅ Logic gate support (OR, AND, XOR)
- ✅ Visual logic gate icons
- ✅ Multiple input types:
  - Device inputs (loop/bus devices)
  - Time of day triggers
  - Date/time triggers
  - API webhook inputs
- ✅ Multiple output types:
  - Device outputs
  - SMS notifications
  - Email notifications
  - API webhooks
- ✅ Enable/disable individual rules
- ✅ Real-time validation

### Project Management
- ✅ Save/Load projects (.kbb format)
- ✅ Recent projects menu (last 10)
- ✅ JSON-based project files
- ✅ Import/Export capabilities
- ✅ Project backup support

### User Interface
- ✅ Professional dark theme
- ✅ Responsive layouts
- ✅ Tree view navigation
- ✅ Context-sensitive forms
- ✅ Device palette at bottom
- ✅ Real-time updates
- ✅ Minimal scrolling design

## 🚀 Getting Started

### Prerequisites

- **Operating System:** Windows 10 or Windows 11
- **Development Environment:** Visual Studio 2022 (Community, Professional, or Enterprise)
- **.NET Framework:** 4.7.2 or higher
- **Git:** For version control (optional but recommended)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/baamiis/BlackBoxControlPanelSimulation.git
   ```

2. **Open the solution**
   ```
   Navigate to the cloned directory
   Double-click BlackBoxControlPanelSimulation.sln
   ```

3. **Restore NuGet packages**
   ```
   Visual Studio will automatically restore packages
   Or manually: Tools → NuGet Package Manager → Restore
   ```

4. **Build the solution**
   ```
   Press Ctrl+Shift+B
   Or: Build → Build Solution
   ```

5. **Run the application**
   ```
   Press F5 (Debug mode)
   Or: Ctrl+F5 (Release mode)
   ```

## 📁 Project Structure

```
BlackBoxControlPanelSimulation/
├── Models/                      # Data models
│   ├── BlackBoxControlPanel.cs            # Fire panel model
│   ├── Loop.cs                 # Loop model
│   ├── LoopDevice.cs          # Device model
│   ├── Bus.cs                  # Bus model
│   ├── BusNode.cs             # Bus node model
│   ├── CauseEffect.cs         # C&E model
│   └── ProjectData.cs         # Save/load models
├── ViewModels/                 # MVVM ViewModels
│   ├── MainViewModel.cs       # Main window VM
│   ├── BlackBoxControlPanelViewModel.cs  # Panel VM
│   ├── LoopViewModel.cs       # Loop VM
│   ├── BusViewModel.cs        # Bus VM
│   ├── CauseAndEffectViewModel.cs
│   └── MenuViewModel.cs       # Menu VM
├── Views/                      # XAML Views
│   ├── MainWindow.xaml        # Main window
│   ├── BlackBoxControlPanelForm.xaml     # Panel form
│   ├── LoopForm.xaml          # Loop form
│   ├── BusForm.xaml           # Bus form
│   └── CauseAndEffectForm.xaml
├── Services/                   # Business logic
│   ├── ProjectService.cs      # Save/load service
│   ├── RecentProjectsManager.cs
│   └── ThemeManager.cs        # Theme service
├── Helpers/                    # Utility classes
│   ├── RelayCommand.cs        # Command helper
│   └── ViewModelBase.cs       # Base VM class
├── Resources/                  # Application resources
│   ├── LogicGateIcons.xaml    # Logic gate icons
│   └── Themes/                # Theme files
├── Images/                     # Device images
│   ├── Emergency_Call.png
│   ├── Smoke_Detector.png
│   └── ...
├── BusImages/                  # Bus node images
│   ├── IO_Module.png
│   ├── Control_Panel.png
│   └── ...
├── DeviceConfigurations/       # Device JSON configs
└── BusNodeConfigurations/      # Bus node configs
```

## 🎮 Usage

### Creating a New Project

1. **Start the application**
2. **File → New Project** (or it starts with a new project)
3. **Configure the fire panel:**
   - Click on "New Fire Panel" in the tree
   - Fill in panel details (name, location, etc.)
   - Save changes

### Adding Devices

1. **Expand the Loops container**
2. **Click on a loop** (e.g., "Loop 1")
3. **Select a device** from the palette at the bottom
4. **Click the device image** to add it to the loop
5. **Configure device** by clicking on it in the tree

### Configuring Busses

1. **Expand the Busses container**
2. **Click on a bus** (e.g., "Bus 1")
3. **Select a bus node** from the palette
4. **Click the node image** to add it to the bus
5. **Node appears in tree** with its icon

### Creating Cause & Effect Rules

1. **Expand "Cause and Effects"**
2. **Right-click** → **Add New C&E** (or use menu)
3. **Select logic gate** (OR, AND, XOR)
4. **Add inputs:**
   - Click "Add Device Input" for device triggers
   - Click "Add Time Input" for time-based triggers
   - Click "Add API Input" for webhook triggers
5. **Add outputs:**
   - Click "Add Device Output" for device actions
   - Click "Add SMS/Email Output" for notifications
   - Click "Add API Output" for webhooks
6. **Enable the rule** with the checkbox
7. **Save**

### Saving Projects

1. **File → Save** (Ctrl+S)
2. **Choose location** and filename
3. **Project saved** as `.kbb` file (JSON format)
4. **Recent projects** menu updated automatically

### Loading Projects

1. **File → Open** (Ctrl+O)
2. **Select `.kbb` file**
3. **Project loads** with all configurations
4. Or use **File → Open Recent** for quick access

## 🛠️ Technologies Used

- **Framework:** WPF (.NET Framework 4.7.2)
- **Language:** C# 7.3
- **Architecture:** MVVM (Model-View-ViewModel)
- **Data Binding:** INotifyPropertyChanged
- **Serialization:** Newtonsoft.Json
- **UI Framework:** XAML
- **Version Control:** Git

## 🎨 UI Features

### Color Scheme
- **Background:** Dark theme (#1E1E1E, #2D2D30)
- **Accent:** Orange (#F39C12)
- **Success:** Green (#2ECC71)
- **Info:** Blue (#3498DB)
- **Text:** Light gray (#CCCCCC)

### Design Principles
- Professional dark theme
- Minimal scrolling (two-column layouts)
- Context-sensitive forms
- Visual feedback on all actions
- Consistent spacing and alignment
- Icon-based navigation

## 📄 File Format

Projects are saved in `.kbb` format (JSON):

```json
{
  "ProjectName": "My Fire Panel",
  "ProjectVersion": "1.0",
  "CreatedDate": "2025-01-20T10:30:00",
  "LastModifiedDate": "2025-01-20T15:45:00",
  "BlackBoxControlPanels": [
    {
      "PanelName": "Main Panel",
      "Location": "Building A",
      "Loops": [...],
      "Busses": [...],
      "CauseAndEffects": [...]
    }
  ]
}
```

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

### Code Style
- Follow C# naming conventions
- Use MVVM pattern consistently
- Add XML comments to public methods
- Keep methods focused and small
- Write descriptive commit messages

## 🐛 Known Issues

- ~~Bus nodes not appearing in tree~~ (Fixed in v1.1)
- ~~ComboBox white background~~ (Fixed in v1.1)
- TreeView expansion state not persisting on reload
- Some binding errors in Output window (non-critical)

## 📝 Roadmap

### Version 1.2 (Planned)
- [ ] Real-time panel monitoring
- [ ] Alarm simulation
- [ ] Device status indicators
- [ ] Log viewer
- [ ] Export to PDF/CSV

### Version 1.3 (Future)
- [ ] Multi-language support
- [ ] Cloud backup
- [ ] Panel communication (serial/TCP)
- [ ] Historical data logging
- [ ] Mobile app integration

## 📞 Support

For support, please:
- **Email:** baamiis7@gmail.com
- **GitHub Issues:** [Report a bug](https://github.com/baamiis/BlackBoxControlPanelSimulation/issues)

## 👨‍💻 Author
**Khalid Hamdou**
**baamiis**
- GitHub: [@baamiis](https://github.com/baamiis)
- Email: baamiis7@gmail.com

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- WPF framework by Microsoft
- Newtonsoft.Json library
- Visual Studio 2022
- GitHub for hosting
- Claude AI for development assistance

## 📸 Screenshots

### Main Interface
*Main window with fire panel tree, bus configuration form, and device palette*

### Bus Configuration
*Bus configuration with two-column layout and device grid*

### Cause & Effect
*Cause & Effect editor with logic gates and input/output configuration*

---

**Made with ❤️ by Khalid Hamdou baamiis ltd**

*Last Updated: January 2025*
