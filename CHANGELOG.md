# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial public release of BlackBoxControl
- Testing infrastructure setup
- CI/CD pipeline configuration
- Code quality tools (.editorconfig, StyleCop)

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

## [1.0.0] - 2025-01-20

### Added
- **Fire Panel Management**
  - Complete fire panel configuration
  - Multiple panel support
  - Network configuration (WiFi, Ethernet)
  - Firmware version tracking
  - Zone and loop management

- **Loop & Device Management**
  - Multi-loop support (configurable)
  - Device palette with visual icons
  - Drag-and-drop device addition
  - Address auto-assignment
  - Device threshold configuration
  - Analog value monitoring
  - Sub-address support
  - Supported devices:
    - Smoke detectors
    - Heat detectors
    - Emergency call points
    - Sounders
    - Beacons
    - And more...

- **Bus Configuration**
  - RS485/RS232/CAN/Ethernet bus support
  - Bus node management with visual tree
  - Node address auto-assignment
  - Input/Output configuration
  - Bus statistics and monitoring
  - Visual node palette
  - Supported nodes:
    - IO Modules
    - Control panels
    - Relay modules
    - Network interfaces

- **Cause & Effect Programming**
  - Logic gate support (OR, AND, XOR)
  - Visual logic gate icons
  - Multiple input types:
    - Device inputs (loop/bus devices)
    - Time of day triggers
    - Date/time triggers
    - API webhook inputs
  - Multiple output types:
    - Device outputs
    - SMS notifications
    - Email notifications
    - API webhooks
  - Enable/disable individual rules
  - Real-time validation

- **Project Management**
  - Save/Load projects (.kbb format)
  - Recent projects menu (last 10)
  - JSON-based project files
  - Import/Export capabilities
  - Project backup support

- **User Interface**
  - Professional dark theme
  - Responsive layouts
  - Tree view navigation
  - Context-sensitive forms
  - Device palette at bottom
  - Real-time updates
  - Minimal scrolling design

- **Documentation**
  - Comprehensive README
  - Installation instructions
  - Usage examples
  - Project structure documentation
  - MIT License

### Changed
- N/A (initial release)

### Fixed
- Bus nodes not appearing in tree view
- ComboBox white background in dark theme
- Various UI alignment issues

---

## How to Update This Changelog

When making changes to the project, update this file following these guidelines:

### Categories

Use these categories for changes:

- **Added** - New features
- **Changed** - Changes in existing functionality
- **Deprecated** - Soon-to-be removed features
- **Removed** - Removed features
- **Fixed** - Bug fixes
- **Security** - Security fixes/improvements

### Version Numbers

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version (X.0.0) - Incompatible API changes
- **MINOR** version (0.X.0) - New functionality (backward compatible)
- **PATCH** version (0.0.X) - Bug fixes (backward compatible)

### Examples

#### Adding a New Feature

```markdown
## [Unreleased]

### Added
- Device search functionality in loop view (#123)
- Export project to PDF format (#124)
```

#### Fixing a Bug

```markdown
## [Unreleased]

### Fixed
- Device addresses not updating after deletion (#125)
- Memory leak in real-time monitoring (#126)
```

#### Before a Release

When releasing version 1.1.0, move items from [Unreleased] to a new version section:

```markdown
## [Unreleased]

### Added
- (empty, ready for next changes)

## [1.1.0] - 2025-02-15

### Added
- Device search functionality in loop view (#123)
- Export project to PDF format (#124)

### Fixed
- Device addresses not updating after deletion (#125)
- Memory leak in real-time monitoring (#126)
```

### Linking Issues

Always reference GitHub issues:

```markdown
### Fixed
- Device validation error when address is 0 (#123)
- Crash when loading corrupted project files (Fixes #124, #125)
```

### Writing Good Changelog Entries

#### ✅ GOOD Examples

```markdown
### Added
- Device search with filtering by type and address (#123)
- PDF export with customizable template support (#124)
- Real-time device status monitoring with WebSocket connection (#125)

### Fixed
- Device addresses now correctly update when devices are reordered (#126)
- Memory leak in monitoring loop that occurred after 24 hours (#127)
- Crash when loading projects with special characters in names (#128)
```

#### ❌ BAD Examples

```markdown
### Added
- Stuff (#123)
- New feature
- Fixed things

### Fixed
- Bug fix
- Various improvements
```

### Commit Messages vs Changelog

**Commit messages** are for developers and describe what changed in the code:
```
feat(devices): implement address validation logic
```

**Changelog entries** are for users and describe what they'll experience:
```
### Added
- Device address validation to prevent conflicts and duplicates
```

---

## Release Notes Template

When creating a new release, use this template:

```markdown
## [X.Y.Z] - YYYY-MM-DD

### 🎉 Highlights

Brief overview of the most important changes in this release.

### Added
- Feature 1 with description (#issue)
- Feature 2 with description (#issue)

### Changed
- Change 1 with explanation (#issue)
- Change 2 with explanation (#issue)

### Fixed
- Bug fix 1 with impact description (#issue)
- Bug fix 2 with impact description (#issue)

### ⚠️ Breaking Changes (if any)

- Description of breaking change and migration path

### 📦 Dependencies

- Updated dependency X from v1.0 to v2.0
- Added new dependency Y v1.5

### 🙏 Contributors

Thanks to @username1, @username2 for their contributions!
```

---

## Version History

- **1.0.0** (2025-01-20) - Initial public release

---

[Unreleased]: https://github.com/baamiis7/BlackBoxControl/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/baamiis7/BlackBoxControl/releases/tag/v1.0.0