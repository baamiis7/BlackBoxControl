# Contributing to BlackBoxControl

First off, thank you for considering contributing to BlackBoxControl! It's people like you that make BlackBoxControl such a great tool.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Process](#development-process)
- [Coding Standards](#coding-standards)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to [baamiis7@gmail.com](mailto:baamiis7@gmail.com).

## Getting Started

### Prerequisites

- **Visual Studio 2022** (Community, Professional, or Enterprise)
- **.NET Framework 4.7.2** or higher
- **Git** for version control
- Basic knowledge of **C#** and **WPF**

### Setting Up Your Development Environment

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/BlackBoxControl.git
   cd BlackBoxControl
   ```
3. **Add the upstream repository**:
   ```bash
   git remote add upstream https://github.com/baamiis7/BlackBoxControl.git
   ```
4. **Open the solution** in Visual Studio:
   ```bash
   # Double-click BlackBoxControl.sln or
   start BlackBoxControl.sln
   ```
5. **Restore NuGet packages**:
   - Visual Studio will do this automatically
   - Or manually: Right-click solution → Restore NuGet Packages
6. **Build the solution** (Ctrl+Shift+B)
7. **Run the application** (F5)

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the [issue tracker](https://github.com/baamiis7/BlackBoxControl/issues) to see if the problem has already been reported.

When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** to demonstrate the steps
- **Describe the behavior you observed** and what you expected to see
- **Include screenshots or animated GIFs** if possible
- **Include your environment details**:
  - OS version (Windows 10, Windows 11)
  - .NET Framework version
  - Visual Studio version
  - Application version

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful** to most users
- **List some examples** of how it would be used
- **Specify which version** you're using

### Your First Code Contribution

Unsure where to begin? You can start by looking through these issues:

- **good first issue** - Issues that should only require a few lines of code
- **help wanted** - Issues that are a bit more involved

### Pull Requests

We actively welcome your pull requests! Here's how to contribute code:

1. Fork the repo and create your branch from `main`
2. Make your changes
3. If you've added code, add tests
4. Ensure the test suite passes
5. Make sure your code follows the project's coding standards
6. Write a good commit message
7. Issue the pull request

## Development Process

### Branching Strategy

We follow a simplified **GitHub Flow**:

- **`main`** - Production-ready code, always stable
- **`feature/feature-name`** - New features
- **`fix/bug-description`** - Bug fixes
- **`docs/what-changed`** - Documentation updates
- **`refactor/what-changed`** - Code refactoring

### Branch Naming Convention

```
<type>/<short-description>

Examples:
feature/add-device-validation
fix/loop-address-bug
docs/update-readme
refactor/improve-viewmodel-base
test/add-panel-tests
chore/update-dependencies
```

### Workflow

1. **Create a branch** from `main`:
   ```bash
   git checkout main
   git pull upstream main
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** and commit regularly:
   ```bash
   git add .
   git commit -m "feat: add device validation"
   ```

3. **Keep your branch updated** with main:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

4. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Open a Pull Request** on GitHub

## Coding Standards

### General Principles

- **SOLID Principles** - Follow SOLID design principles
- **DRY (Don't Repeat Yourself)** - Avoid code duplication
- **KISS (Keep It Simple, Stupid)** - Favor simplicity over complexity
- **YAGNI (You Aren't Gonna Need It)** - Don't add functionality until needed

### C# Coding Conventions

We follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

#### Naming Conventions

```csharp
// Classes, Interfaces, Methods, Properties - PascalCase
public class FirePanel { }
public interface IDeviceService { }
public void SaveConfiguration() { }
public string PanelName { get; set; }

// Private fields - camelCase with underscore prefix
private string _panelName;
private IDeviceService _deviceService;

// Local variables and parameters - camelCase
int deviceCount = 0;
public void AddDevice(string deviceName) { }

// Constants - PascalCase
public const int MaxLoopCount = 8;

// Events - PascalCase
public event EventHandler DeviceAdded;
```

#### Code Organization

```csharp
// Order of class members:
// 1. Fields (private fields first)
// 2. Constructors
// 3. Properties
// 4. Events
// 5. Methods (public first, then private)
// 6. Nested types

public class FirePanel : ViewModelBase
{
    // Fields
    private string _panelName;
    private ObservableCollection<Loop> _loops;
    
    // Constructors
    public FirePanel()
    {
        _loops = new ObservableCollection<Loop>();
    }
    
    // Properties
    public string PanelName
    {
        get => _panelName;
        set
        {
            if (_panelName != value)
            {
                _panelName = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Events
    public event EventHandler PanelSaved;
    
    // Public methods
    public void Save()
    {
        // Implementation
    }
    
    // Private methods
    private void ValidatePanel()
    {
        // Implementation
    }
}
```

#### Method Guidelines

- **Keep methods small** - Ideally under 20 lines
- **Single Responsibility** - Each method should do one thing
- **Meaningful names** - Method names should clearly describe what they do
- **Avoid side effects** - Methods should be predictable

```csharp
// ✅ GOOD: Small, focused, clear purpose
public void AddDevice(LoopDevice device)
{
    if (device == null)
        throw new ArgumentNullException(nameof(device));
    
    ValidateDevice(device);
    _devices.Add(device);
    OnDeviceAdded(device);
}

// ❌ BAD: Too long, does too much
public void AddDeviceAndSaveAndNotify(LoopDevice device)
{
    // 100 lines of mixed responsibilities...
}
```

### XAML Conventions

```xaml
<!-- Use meaningful names for UI elements -->
<Button x:Name="SaveButton" Content="Save" />

<!-- Use data binding over code-behind -->
<TextBox Text="{Binding PanelName, UpdateSourceTrigger=PropertyChanged}" />

<!-- Order attributes logically -->
<Button 
    x:Name="AddDeviceButton"
    Content="Add Device"
    Command="{Binding AddDeviceCommand}"
    Width="120"
    Height="30"
    Margin="10,5"
    Style="{StaticResource PrimaryButtonStyle}" />
```

### Comments and Documentation

#### XML Documentation

**ALL public members** must have XML documentation:

```csharp
/// <summary>
/// Represents a fire alarm panel with multiple loops and buses.
/// </summary>
public class FirePanel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the name of the fire panel.
    /// </summary>
    /// <value>The panel name as a string. Must not be null or empty.</value>
    public string PanelName { get; set; }
    
    /// <summary>
    /// Adds a new loop to the fire panel.
    /// </summary>
    /// <param name="loop">The loop to add. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when loop is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when maximum loop count is exceeded.
    /// </exception>
    public void AddLoop(Loop loop)
    {
        // Implementation
    }
}
```

#### Inline Comments

```csharp
// ✅ GOOD: Explains "why", not "what"
// Calculate address using loop position to ensure uniqueness across the panel
int address = loopId * 100 + devicePosition;

// ❌ BAD: States the obvious
// Set the address
address = 100;

// ✅ GOOD: Explains complex logic
// We need to recalculate all addresses when a device is removed because
// the addressing scheme is position-based and gaps are not allowed
RecalculateAddresses();
```

## Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type

Must be one of the following:

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Changes that don't affect code meaning (formatting, etc.)
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **perf**: Performance improvements
- **test**: Adding or correcting tests
- **chore**: Changes to build process, tools, dependencies, etc.

### Scope

The scope is optional and specifies the place of the commit change:

- **loops**: Loop management features
- **devices**: Device-related functionality
- **buses**: Bus configuration
- **causeeffect**: Cause & effect system
- **ui**: User interface changes
- **tests**: Test-related changes
- **docs**: Documentation changes

### Subject

- Use imperative, present tense: "add" not "added" nor "adds"
- Don't capitalize first letter
- No period (.) at the end
- Maximum 72 characters

### Body

- Explain the motivation for the change
- Contrast with previous behavior
- Wrap at 72 characters

### Footer

- Reference issues: `Closes #123`, `Fixes #456`
- Note breaking changes: `BREAKING CHANGE: <description>`

### Examples

```bash
# Simple feature
feat(devices): add address validation for loop devices

# Bug fix with details
fix(loops): correct device counting in loop capacity check

When adding devices beyond the loop capacity, the validation
was incorrectly allowing one extra device due to an off-by-one
error in the comparison logic.

Closes #45

# Breaking change
feat(api)!: change project file format to JSON

BREAKING CHANGE: Project files are now saved as JSON instead of XML.
Users will need to re-save their projects in the new format. A
migration tool is provided in Tools > Migrate Project.

Closes #67

# Documentation update
docs(readme): add screenshots and usage examples

# Multiple changes
refactor(viewmodels): improve property change notifications

- Extract common property change logic to base class
- Add validation support in ViewModelBase
- Update all ViewModels to use new base class methods

Closes #89, #90
```

## Pull Request Process

### Before Submitting

1. **Test your changes** thoroughly
2. **Run all tests** and ensure they pass
3. **Update documentation** if needed
4. **Follow coding standards**
5. **Write meaningful commit messages**

### PR Description

Use the pull request template and include:

- **Description** of changes
- **Type of change** (bug fix, feature, etc.)
- **Related issues** (Closes #123)
- **Testing performed**
- **Screenshots** (for UI changes)
- **Checklist** completion

### Review Process

1. At least **one reviewer** must approve
2. All **CI checks must pass**
3. All **conversations must be resolved**
4. Code must follow **coding standards**
5. **Tests must be included** for new features

### Merging

- Pull requests are merged using **squash and merge**
- The commit message should follow the conventional commits format
- Delete the branch after merging

## Testing Guidelines

### Writing Tests

We use **xUnit** for unit testing. All new features must include tests.

```csharp
using Xunit;
using FluentAssertions;

namespace BlackBoxControl.Tests.ViewModels
{
    public class FirePanelViewModelTests
    {
        [Fact]
        public void AddLoop_WithValidLoop_ShouldAddToCollection()
        {
            // Arrange
            var viewModel = new FirePanelViewModel();
            var loop = new Loop { Name = "Loop 1" };
            
            // Act
            viewModel.AddLoop(loop);
            
            // Assert
            viewModel.Loops.Should().Contain(loop);
            viewModel.Loops.Should().HaveCount(1);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PanelName_WithInvalidValue_ShouldThrowException(string invalidName)
        {
            // Arrange
            var viewModel = new FirePanelViewModel();
            
            // Act & Assert
            Action act = () => viewModel.PanelName = invalidName;
            act.Should().Throw<ArgumentException>();
        }
    }
}
```

### Test Coverage Goals

- **Minimum 70% code coverage** for new features
- **Critical paths** must have 100% coverage
- **All public methods** should have tests
- **Edge cases** must be tested

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test class
dotnet test --filter FirePanelViewModelTests
```

## Documentation

### Code Documentation

- Add XML documentation to all public members
- Keep documentation up-to-date with code changes
- Include examples in documentation when helpful

### README Updates

Update the README.md when:
- Adding new features
- Changing installation steps
- Modifying usage instructions
- Adding dependencies

### Additional Documentation

Consider updating:
- `docs/architecture.md` - For architectural changes
- `docs/api-reference.md` - For API changes
- `CHANGELOG.md` - For all notable changes

## Style Guide

### File Organization

```
BlackBoxControl/
├── Models/              # Data models only
├── ViewModels/          # Presentation logic
├── Views/               # XAML views
├── Services/            # Business logic
├── Helpers/             # Utility classes
├── Converters/          # Value converters
└── Resources/           # Resources (styles, images)
```

### File Naming

- Use **PascalCase** for file names
- Match file name to the class name
- One class per file (generally)

Examples:
- `FirePanel.cs`
- `FirePanelViewModel.cs`
- `FirePanelView.xaml`

## Questions?

If you have questions, you can:

1. **Check existing issues** - Your question might already be answered
2. **Open a new issue** - We're happy to help
3. **Email us** - baamiis7@gmail.com

## Recognition

Contributors will be recognized in our [README](README.md) and release notes.

Thank you for contributing to BlackBoxControl! 🎉

---

**Last Updated:** December 2024