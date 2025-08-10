\# Ecosystem Simulation Game — Unity Project



This repository contains all the C# scripts and configuration needed to run a simple ecosystem simulation in Unity. The game generates a procedural terrain, grows plants, and spawns herbivores and predators that interact through eating, reproduction and simple genetic inheritance.



\## Features



\- \*\*Procedural Terrain\*\*: Generates a mesh with Perlin noise for height, including edge falloff to create natural-looking islands or plains.

\- \*\*Dynamic Plant Growth\*\*: A `PlantField` component maintains biomass in grid cells and grows plants over time.

\- \*\*Animal AI\*\*: Two basic animal types — herbivores and predators — move, eat, reproduce, and evolve based on genetic traits (speed, sight, size and hue).

\- \*\*Genetic Inheritance\*\*: Offspring inherit traits from parents with slight mutation, allowing populations to evolve over time.

\- \*\*User Controls\*\*: Use the `Spawner` component to create animals at the mouse cursor or random positions. Adjust traits via UI sliders and choose between presets (Lapin, Cerf, Loup, Lynx) or a custom gene configuration.

\- \*\*Orbit Camera\*\*: A simple camera script allows orbit, pan and zoom with the mouse.

\- \*\*Bootstrap\*\*: An `EcosystemBootstrap` script sets up the terrain, plants, simulation manager, spawner, camera and lighting when the scene is first loaded.



\## Usage



1\. Clone this repository into a folder on your machine.

2\. Open Unity Hub, choose \*\*Open project\*\*, and select the folder you cloned to. Unity will generate the project settings and allow you to open it.

3\. Open any scene (or create a new one), and add an empty GameObject called `Bootstrap` to the hierarchy.

4\. Attach the `EcosystemBootstrap` component to the `Bootstrap` object. At runtime, this script will create all necessary objects (terrain, ecosystem manager, spawner UI, camera and light).

5\. Press Play. The simulation will generate a terrain and populate it with plants and animals. Use the UI window to spawn more creatures or change gene parameters.



\## Folder Structure





