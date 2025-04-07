# MazeRunner 3D

> A project for the **KIV/ZPG** course – a simple 3D game built on OpenTK.  
> Features player movement, a flashlight, environment collisions, and map-based level loading.

![screenshot](preview.png)

---

## 🧠 Key Features

Optional launch arguments:
- `--fullscreen` – starts the game in fullscreen mode
- `--mac` – enables Retina display mode (viewport scale 2.0)

---

## 🔗 Repository

👉 [https://github.com/MrEll3n/maze-runner](https://github.com/MrEll3n/maze-runner)

---

## 🧑‍💻 Author

This project was created as a semester assignment for the **KIV/ZPG** course – Fundamentals of Computer Graphics  
**[Vít Novotný]**  
Faculty of Applied Sciences, University of West Bohemia

---

- 🌌 3D environment with first-person camera.
- 💡 Flashlight (spotlight) emitted from the player with adjustable cone angle.
- 📦 Collision detection against a triangle mesh of wall geometry.
- 🗺️ Map loading from a text file (character-based representation).
- 🎮 Mouse and keyboard support (WASD + mouse + spacebar).
- 🧱 Textured walls, floors, and ceilings.

---

## ⌨️ Controls

| Key              | Action                         |
|------------------|--------------------------------|
| `W / A / S / D`  | Move forward/sideways          |
| `Mouse`          | Rotate camera                  |
| `Space`          | Jump                           |
| `Esc`            | Lock/unlock mouse              |
| `Alt + Enter`    | Toggle fullscreen              |
| `Alt + Q`        | Exit the game                  |
| `Scroll Wheel`   | Adjust FOV (zoom in/out)       |

---

## 🔧 How to Run

Requires [.NET 6.0+](https://dotnet.microsoft.com/) and the [OpenTK](https://github.com/opentk/opentk) library.
