# Ecosystem Simulation Game — Unity Project

This repository contains all the C# scripts and configuration needed to run a simple ecosystem simulation in Unity. The game generates a procedural terrain, grows plants, and spawns herbivores and predators that interact through eating, reproduction and simple genetic inheritance.

## Features

- **Procedural Terrain**: Generates a mesh with Perlin noise for height, including edge falloff to create natural-looking islands or plains.
- **Dynamic Plant Growth**: A `PlantField` component maintains biomass in grid cells and grows plants over time.
- **Animal AI**: Two basic animal types — herbivores and predators — move, eat, reproduce, and evolve based on genetic traits (speed, sight, size and hue).
- **Genetic Inheritance**: Offspring inherit traits from parents with slight mutation, allowing populations to evolve over time.
- **User Controls**: Use the `Spawner` component to create animals at the mouse cursor or random positions. Adjust traits via UI sliders and choose between presets (Lapin, Cerf, Loup, Lynx) or a custom gene configuration.
- **Orbit Camera**: A simple camera script allows orbit, pan and zoom with the mouse.
- **Bootstrap**: An `EcosystemBootstrap` script sets up the terrain, plants, simulation manager, spawner, camera and lighting when the scene is first loaded.

## Usage

1. Clone this repository and open it in Unity (2019.4 or newer). The Unity project resides in this directory.
2. Create a new scene or open an existing one. Add an empty GameObject named `Bootstrap` to the hierarchy.
3. Attach the `EcosystemBootstrap` component to the `Bootstrap` object. At runtime, this script will create all necessary objects (terrain, ecosystem manager, spawner UI, camera and light).
4. Press Play. The simulation will generate a terrain and populate it with plants and animals. Use the UI window to spawn more creatures or change gene parameters.

## Folder Structure

```
gptEcosystemiHopeProject/
├── Assets/
│   ├── Scripts/                # C# scripts used by the game
│   └── Scenes/                # Scene assets (this folder contains a .gitkeep placeholder)
├── .gitignore                 # Ignore Unity-generated files and folders
└── README.md                  # This file
```

## Scripts Overview

* `EcosystemBootstrap.cs` — The entry point; sets up the scene objects and runs initial population spawning.
* `TerrainGenerator.cs` — Generates a procedural terrain mesh and applies it to a `MeshFilter` and `MeshCollider`.
* `PlantField.cs` — Manages plant biomass in grid cells and handles growth over time.
* `GeneUtils.cs` — Helper struct and methods for genetic traits and inheritance.
* `Animal.cs` — Base class for animals with shared properties and methods.
* `Herbivore.cs` — Herbivores seek plant-rich areas, eat plants, pay energy costs, and reproduce.
* `Predator.cs` — Predators hunt herbivores within their sight radius, gain energy from captures, and reproduce.
* `SimulationManager.cs` — Central manager that coordinates the simulation: updates plants, ticks animals, handles births/deaths, and sampling functions.
* `Spawner.cs` — UI and spawning logic allowing users to spawn animals via a window or mouse click. Contains presets for common species.
* `SimpleOrbitCamera.cs` — A camera controller to orbit around the scene, zoom and pan.

This project is designed to be a starting point for experiments in artificial life, ecology and evolution. Feel free to extend it with additional species, behaviours, environmental effects and UI features.