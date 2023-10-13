# Island Generator

**Disclaimer** This project is still under development.

The aim of this project is the procedural generation of floating islands, with emphasis on creating stylized, good looking and detailed, yet still performant, environments.
Currently there are no real gameplay features yet, however, I may decide to add these in a future update.

### How to use

If you are interested in looking at the current state of the project or my code, feel free to clone the repository.
The Unity project uses version 2022.3.2f1, but should theoretically be compatible with any other version that doesn't change the fundamentals of Unity's HDRP.

Simply load the 'ShowcaseScene' in the editor and press play. To generate an island, find the "IslandGenerator" object in the Scene Hierarchy and press the "Generate Island" button in the inspector.
You can play around with any of the settings, but due to performance reasons I do recommend not increasing the tile size beyond 50x50. Please note that the generator uses a seed. This means that using identical settings (including the seed) twice, will reproduce the previous island, however, with varying placement of environmental objects. If you want to generate a completely new island everytime you press "Generate Island", please tick the "Force Random Seed?" box next to the button.

The camera will now orbit around the island. Press W or S (or use the Up/Down arrow keys) to change the orbit height. Orbiting too fast or too slow? Try changing the "Orbit Speed" property of the "ShowcaseManager".
You can also click tiles to select them. This has no practical use as of now, but will be used at a later point.