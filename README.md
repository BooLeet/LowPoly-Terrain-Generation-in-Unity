# LowPoly-Terrain-Generation-in-Unity
Generates a LowPoly terrain based on a "height map" and fills it with given GameObjects based on a "detail map". Written in C#

Height map should not be lower than 64x64 pixels in resolution, because the script generates terrain in chunks of 64x64 "quads".
