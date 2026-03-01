# BlackBoxControl — Theme System

---

## Overview

The app ships with three themes. All share identical control templates; only color
brush values differ per theme.

| Theme | File | Palette | Use case |
|-------|------|---------|---------|
| Green (default) | `Themes/GreenTheme.xaml` | Forest + Emerald | Fire safety / operational |
| Blue | `Themes/BlueTheme.xaml` | Midnight Navy + Azure | Corporate / professional |
| Dark | `Themes/DarkTheme.xaml` | Charcoal + Fire Orange | Low-light environments |

---

## File Structure

```
Themes/
  BaseStyles.xaml      ← ALL control templates (shared)
  GreenTheme.xaml      ← color brushes only + merges BaseStyles
  BlueTheme.xaml       ← color brushes only + merges BaseStyles
  DarkTheme.xaml       ← color brushes only + merges BaseStyles

Resources/
  LogicGateIcons.xaml  ← merged inside BaseStyles (always loaded)
```

### Theme file anatomy (~20 lines)

```xml
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="pack://application:,,,/Themes/BaseStyles.xaml"/>
    </ResourceDictionary.MergedDictionaries>

    <!-- Accent / brand -->
    <SolidColorBrush x:Key="PrimaryBrush"        Color="#3AB86B"/>
    <SolidColorBrush x:Key="PrimaryHoverBrush"   Color="#2DA55C"/>
    <SolidColorBrush x:Key="PrimaryPressedBrush" Color="#21924E"/>
    <SolidColorBrush x:Key="SecondaryBrush"      Color="#2BA3A0"/>
    <SolidColorBrush x:Key="AccentBrush"         Color="#3AB86B"/>
    <!-- ... remaining brushes ... -->
</ResourceDictionary>
```

---

## Color Brush Keys

Every brush key must be defined in every theme file.

### Accent

| Key | Purpose |
|-----|---------|
| `PrimaryBrush` | Main brand color — buttons, highlights, active states |
| `PrimaryHoverBrush` | Button hover state |
| `PrimaryPressedBrush` | Button pressed state |
| `SecondaryBrush` | Accent for secondary elements (bus nodes, info badges) |
| `AccentBrush` | Alias of PrimaryBrush (legacy, kept for compatibility) |

### Semantic

| Key | Purpose |
|-----|---------|
| `SuccessBrush` | Online / active / valid states |
| `DangerBrush` | Delete buttons / error states |
| `WarningBrush` | Warning indicators / threshold alerts |

### Surfaces

| Key | Purpose |
|-----|---------|
| `BackgroundBrush` | Window / page background (darkest) |
| `SurfaceBrush` | Cards, GroupBoxes, panels (mid) |
| `TitleBarBrush` | Custom title bar background |
| `MenuBarBrush` | Menu strip background |
| `BorderBrush` | All border / separator lines |
| `HoverBrush` | Row / card hover background |

### Text

| Key | Purpose |
|-----|---------|
| `ForegroundBrush` | Primary text |
| `ForegroundDimBrush` | Labels, subtitles, hints |
| `ForegroundDisabledBrush` | Disabled control text |

---

## Switching Theme at Runtime

Themes are switched via `ThemeManager` (called from `MenuViewModel`):

```csharp
ThemeManager.ChangeTheme(Application.Current, "Blue");
// Supported names: "Green", "Blue", "Dark"
```

Internally this clears `App.Resources.MergedDictionaries` and loads the new
theme file. Because `LogicGateIcons.xaml` is merged **inside** `BaseStyles.xaml`
(not directly in `App.xaml`), it is always reloaded with the theme — it cannot
be accidentally lost.

---

## Adding a New Theme

1. Copy `GreenTheme.xaml` to e.g. `Themes/RedTheme.xaml`
2. Update all color brush values
3. Register it in `ThemeManager` with the name `"Red"`
4. Add a menu item in `MenuViewModel` / `MainWindow.xaml`

No changes to `BaseStyles.xaml` or any form XAML are needed — all controls
use `DynamicResource` so they pick up the new brushes automatically.

---

## DynamicResource vs StaticResource

All color references in forms and `BaseStyles.xaml` use **`DynamicResource`**.
This is required for theme switching to work at runtime.

`StaticResource` is only acceptable for:
- Button styles referencing other styles within `BaseStyles.xaml`
  (e.g. `SecondaryButton BasedOn PrimaryButton`) where the referenced
  resource is guaranteed to be in the same file and loaded first.

**Never** use `StaticResource` for color brush keys in form XAML — the theme
switch will have no effect on those elements.
