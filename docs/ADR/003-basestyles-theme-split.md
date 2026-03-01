# ADR-003: BaseStyles + Per-Theme Color Files

**Status:** Accepted
**Date:** 2025

---

## Context

The application originally had three theme files (DarkTheme, BlueTheme, GreenTheme),
each containing 300+ lines of duplicated XAML control templates. The only difference
between files was the ~20 color values. Any style change required editing three files.

## Decision

Extract all control templates into a single **`Themes/BaseStyles.xaml`** file.
Each theme file becomes ~20 lines of color brush definitions that merge BaseStyles:

```xml
<!-- BlueTheme.xaml -->
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="pack://application:,,,/Themes/BaseStyles.xaml"/>
    </ResourceDictionary.MergedDictionaries>
    <SolidColorBrush x:Key="PrimaryBrush" Color="#3B9BFF"/>
    <!-- ... -->
</ResourceDictionary>
```

All color references inside `BaseStyles.xaml` use **`DynamicResource`** so that
switching theme replaces brushes at runtime.

`LogicGateIcons.xaml` is merged **inside** `BaseStyles.xaml` (not `App.xaml`)
to ensure it is always reloaded with any theme switch.

## Rationale

- Single place to fix control appearance bugs
- Adding a theme requires only a 20-line color file
- `DynamicResource` ensures theme switching works at runtime without restart
- Merging `LogicGateIcons` in `BaseStyles` prevents it being lost when
  `ThemeManager` clears `App.Resources.MergedDictionaries`

## Consequences

**Positive:**
- 3 × 300 lines → 1 × 300 + 3 × 20 lines (net saving ~500 lines)
- Themes are trivial to create; only color palette knowledge needed
- Control shapes/animations are consistent across all themes

**Negative:**
- All themes share identical control shapes — per-theme shape variation is not supported
  (acceptable: branding is color-only)
- `DynamicResource` has a small runtime lookup cost vs `StaticResource`
  (negligible for a configuration tool)
