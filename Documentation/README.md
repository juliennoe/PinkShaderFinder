# 🎯 Shader Finder

**Find and fix broken shaders in your Unity project efficiently.**

## ✨ Features

- Detects all materials using the internal error shader.
- Suggests the most common shader used in each folder.
- Allows per-material shader reassignment.
- Apply a global shader to selected materials with one click.
- Includes search bar, selection tools, and help dropdown.
- Color-coded buttons for a clearer UI.
- Persistent global shader between sessions.
- Safe to use: disables UI in Play Mode.

## 🧪 How to Use

1. Open via `Tools > Shader Finder`.
2. Click **Find Error Shaders** to scan your project.
3. Assign shaders manually or select a **Global Shader**.
4. Use **Apply All** to batch update materials.
5. Use search and filters to quickly find what you need.

## ✅ Compatibility

- Unity **2021.3** or higher
- Editor-only tool (won’t run in builds)

## 📦 Installation

### Using Unity Package Manager

1. Open `Packages/manifest.json`
2. Add this line:

```json
"com.juliennoe.shaderfinder": "https://github.com/juliennoe/shaderfinder.git"
```

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](./LICENSE) file.
