# Procedural 2D Level Generator Unity

A **Procedural Content Generation (PCG)** tool built in Unity using C# and Unity's Tilemap system.

This tool generates 2D dungeon layouts featuring randomly placed rooms, connected corridors, walls, and designated start and end rooms. It runs through a custom Unity Editor window, allowing levels to be generated and cleared directly within the editor.

I created this project to experiment with procedural generation and explore different approaches to algorithmic level design.

## Features

- **Procedural Room Generation** — generates rectangular rooms with random dimensions and positions while maintaining spacing between rooms.
- **MST Corridor Generation** — connects rooms using a Minimum Spanning Tree (MST) to create a connected dungeon layout.
- **Start & End Rooms** — automatically selects the largest room as the start and the room furthest from it as the end.
- **Procedural Walls** — generates walls around rooms and corridors, with additional cleanup to remove isolated wall tiles.
- **Multiple Map Sizes** — choose between Small, Medium, and Large layout presets.
- **Seed-Based Generation** — use random seeds for varied layouts or enter a specific seed to reproduce a generation.
- **Custom Sprites & Colours** — assign your own sprites and adjust the colours of room and corridor floors.
- **Custom Editor Window** — generate and clear levels directly from the Unity Editor.

## Built With

- Unity
- C#
- Unity Tilemap system
- Unity Editor scripting

## How It Works

The generator creates each level through several stages:

1. **Room Placement** — randomly generates rectangular rooms within the map boundaries, checking for overlaps and maintaining space between rooms.
2. **Room Connection** — calculates the distances between room centres and uses a Minimum Spanning Tree approach to connect the rooms.
3. **Corridor Generation** — carves paths between rooms using L-shaped corridors with randomised midpoints to create layout variation.
4. **Start & End Selection** — assigns the largest room as the starting room and the furthest room from it as the end room.
5. **Wall Generation** — places walls around floor tiles and corridors, then removes isolated wall tiles.
6. **Tilemap Rendering** — draws the generated layout onto the assigned Tilemap using the sprites and colours provided.

## Getting Started

### 1. Download the Repository

Download or clone this repository from GitHub and open your Unity project with the downloaded files included.

### 2. Create a Tilemap

In the Unity Hierarchy, create a **Grid** with a **Tilemap**, or use an existing Tilemap in your scene.

The generator uses the Tilemap you assign to draw the dungeon, so you don't need to create the rooms or corridors manually.

### 3. Open the Generator

In the Unity Editor menu, navigate to:

`Tools > Tilemap PCG Generator`

This opens the custom level generator window.

### 4. Assign Your Assets

In the generator window, assign your Tilemap and your own sprites for:

- Floor
- Wall
- Start room
- End room

**Note:** The generator code is provided, but the Tilemap and sprite assets must be set up by you.

### 5. Generate a Level

Choose a layout size, configure the corridor offset, and adjust the tile colours if desired.

Click **Generate** to create your dungeon, or **Clear** to remove the generated tiles from the assigned Tilemap.

## Generation Settings

| Setting | Description |
|---|---|
| Small | 25 × 25 map, targeting 3 rooms |
| Medium | 40 × 40 map, targeting 5 rooms |
| Large | 55 × 55 map, targeting 7 rooms |
| Corridor Offset | Controls the randomised midpoint offset used when connecting rooms |
| Random Seed | Generates a new random seed for each generation |
| Seed Value | Allows a specific seed to be entered manually |
| Room Floor Colour | Changes the tint of room floor tiles |
| Corridor Colour | Changes the tint of corridor floor tiles |

Room dimensions vary depending on the selected layout size. The generator also uses room padding to reduce overlap and maintain spacing between rooms.

*Note: The room counts are targets; the generator may place fewer rooms if it cannot find enough valid positions.*

## Customisation

The generator allows you to use your own visual assets, making it possible to adapt the generated levels to different art styles.

You can customise:

- Room floor sprites
- Wall sprites
- Start room sprites
- End room sprites
- Room floor tint
- Corridor floor tint

Corridors use the assigned floor sprite with a separate colour tint.

## Future Improvements

Possible extensions for the project include:

- Adding different room shapes and more complex layouts.
- Introducing additional corridor patterns and optional connections between rooms.
- Adding more control over room placement and dungeon density.
- Improving the visualisation of generated layouts.
- Expanding the generator with gameplay elements such as enemy, item, or loot placement.

## About

This project was created as an exploration of **Procedural Content Generation (PCG)** in game development. It demonstrates algorithmic room placement, graph-based connectivity, randomisation, tilemap rendering, and custom Unity Editor tooling.

<img width="1202" height="815" alt="image" src="https://github.com/user-attachments/assets/ec0a9fec-6420-4907-83ac-54daf896db48" />
<img width="1133" height="807" alt="image" src="https://github.com/user-attachments/assets/8999ceb2-bc8e-4ef6-b623-2804074d5d98" />
<img width="863" height="815" alt="image" src="https://github.com/user-attachments/assets/a53fbf66-3aa4-48b4-9a67-6902995b6555" />



