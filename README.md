# Examples
Repository with code examples.

In this repository you will find examples of code, ui elements, multiplayer input management, custom shaders and shader effects.

This repository also includes an internal submodule with [Core](https://github.com/Daniel1x/Core) scripts and tools that I created and use across multiple projects.

Example class:

[ExampleBehaviourClass](https://github.com/Daniel1x/Examples/blob/main/Assets/Code/ExampleBehaviourClass.cs)

Tools and editor extensions ([Core](https://github.com/Daniel1x/Core)):

[TextureSaver](https://github.com/Daniel1x/Core/blob/main/Tools/TextureSaver/TextureSaver.cs) Runtime/editor utility for capturing Texture / RenderTexture data and writing it to disk (e.g. PNG/EXR) with optional resizing, color-space handling and format selection. Provides a simple high-level entry point for export operations.

[TextureSaverUtilities](https://github.com/Daniel1x/Core/blob/main/Tools/TextureSaver/Editor/TextureSaverUtilities.cs) Helper/static functions used by TextureSaver (e.g. path resolution, temporary RT creation, format conversion, CPU read-back safety, filename sanitizing). Keeps low-level implementation details decoupled from the main saver logic.

[TextureSaverEditor](https://github.com/Daniel1x/Core/blob/main/Tools/TextureSaver/Editor/TextureSaverEditor.cs) Custom editor/window tools that provide a user interface for selecting textures, configuring output settings, and triggering save/export actions within the editor. Adds quality-enhancing features such as saving textures rendered from an image component and 9-slice optimization.

[AnimationExtensions](https://github.com/Daniel1x/Core/blob/main/Extensions/AnimationExtensions.cs)
[RenderTextureExtensions](https://github.com/Daniel1x/Core/blob/main/Extensions/RenderTextureExtensions.cs)

Gameplay implementation example and graphics preview:

Characters, weapons, potions, textures, and effects were created by me. The scripts in this repository include the input management system, character controls, equipment management, the pickup system, character stat display, and much more.

<img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview9.png" alt="Zdjęcie 9" width="500"/>

Utilities example:

[AssetSpawner](https://github.com/Daniel1x/Core/blob/main/Tools/AssetSpawner/AssetSpawner.cs) is a lightweight Addressables-aware placeholder component used to represent a dynamic prefab reference in both Edit Mode and Play Mode. In the Editor, this component displays a preview of the assigned resource object in its current transformation.

[AssetSelectionRedirector](https://github.com/Daniel1x/Core/blob/main/Tools/AssetSpawner/Editor/AssetSelectionRedirector.cs) is a Editor utility that intercepts Scene/Hierarchy selection of transient preview GameObjects and redirects it to their owning AssetSpawner, ensuring consistent editing focus. It safely replaces the Unity Selection on the next editor loop to avoid flicker or recursion.

[AssetSelectionBase](https://github.com/Daniel1x/Core/blob/main/Tools/AssetSpawner/AssetSelectionBase.cs) is a lightweight component attached to a preview instance that stores a redirection target (the real owner) so selection logic can map the visual proxy back to the spawner. It serves purely as a metadata bridge and is not included in builds.

Working class example:

[PlayerBasicInputs](https://github.com/Daniel1x/Examples/blob/main/Assets/Code/PlayerBasicInputs.cs) wraps Unity Input System actions for a single player, normalizing raw input (movement, look, actions) and exposing a clean API / events to gameplay systems. It centralizes per-frame state (pressed / held / axis) so other components remain input‑agnostic.

[PlayerInputManagerProvider](https://github.com/Daniel1x/Examples/blob/main/Assets/Code/Player/PlayerInputManagerProvider.cs) manages dynamic player joining/removal and distributes input devices (gamepads / keyboard) to newly spawned player objects. It maintains a registry of active players and broadcasts lifecycle events (join/leave) to dependent systems.

[PlayerControllerSelectionMenu](https://github.com/Daniel1x/Examples/blob/main/Assets/Code/UI/Menus/PlayerControllerSelectionMenu.cs) UI flow that lists available input devices or player slots and lets the user assign / reassign controllers before starting gameplay. It feeds the selection results back into the player management layer (e.g. spawning or enabling players).

[Multiplayer implementation example video](https://www.youtube.com/watch?v=VnKeMXx4P2M)

These scripts allow you to create multiple players and assign them controllers, as shown in the screenshots below.

<img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview6.png" alt="Zdjęcie 6" height="250"/><img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview7.png" alt="Zdjęcie 7" height="250"/>

Multiplayer game example with custom levels and characters:

[Multiplayer implementation example video](https://www.youtube.com/watch?v=VnKeMXx4P2M)

<img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview8.png" alt="Zdjęcie 8" height="250"/>

Example terrain shader with scanning effect and multiple height based textures with added noise.
Basic weather system with directional light rotation and cloud animation.

[Terrain shader example video](https://youtu.be/8upf36Ew92E)

<img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview5.png" alt="Zdjęcie 5" height="250"/><img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview2.png" alt="Zdjęcie 2" height="250"/>

# Projects I have worked on:

## The House Of The Dead: Remake

<img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview3.png" alt="Zdjęcie 3" width="500"/>

## Front Mission 3: Remake

<img src="https://github.com/Daniel1x/Examples/blob/main/Assets/Content/Preview/t_Preview4.png" alt="Zdjęcie 4" width="500"/>
