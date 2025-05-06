using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace ZPG
{
    public class MapReader
    {
        private readonly List<Wall> walls = new();
        private readonly List<Model> renderables = new();
        private readonly Dictionary<Vector2i, ITriggerZone> triggerMap = new();

        private readonly Dictionary<char, List<Vector2i>> teleportTilePositions = new();
        private readonly Shader shader;
        private readonly int wallLength = 2;

        public Vector3 PlayerStartPosition { get; private set; }
        public List<Teleport> Teleports { get; private set; } = new();

        public List<Wall> GetWalls() => walls;
        public List<Model> GetRenderables() => renderables;
        public Vector3 GetPlayerStartPosition() => PlayerStartPosition;
        public Dictionary<Vector2i, ITriggerZone> GetTriggerMap() => triggerMap;

        public List<TeleportTrigger> GetTeleportTriggers()
        {
            List<TeleportTrigger> triggers = new();
            foreach (var model in renderables)
            {
                if (model is TeleportTrigger trigger)
                    triggers.Add(trigger);
            }
            return triggers;
        }

        public MapReader(Shader shader)
        {
            this.shader = shader;
        }

        public void ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                {
                    Console.WriteLine("File doesn't contain enough lines.");
                    return;
                }

                string[] dimensions = lines[0].Split('x');
                int width = int.Parse(dimensions[0]);
                int height = int.Parse(dimensions[1]);

                if (lines.Length - 1 < height)
                {
                    Console.WriteLine("Map file does not contain expected number of rows.");
                    return;
                }

                for (int z = 0; z < height; z++)
                {
                    string line = lines[z + 1];
                    for (int x = 0; x < width; x++)
                    {
                        if (x >= line.Length)
                            continue;

                        char c = line[x];
                        Vector3 position = new Vector3(x * wallLength, 0, z * wallLength);

                        if (IsWall(c))
                        {
                            Wall wall = new() { Shader = shader, Position = position };
                            walls.Add(wall);
                            renderables.Add(wall);
                        }
                        else if (IsStartPosition(c))
                        {
                            PlayerStartPosition = position;
                        }
                        else if (IsLight(c)) { }
                        else if (IsDoor(c)) { }
                        else if (IsSolidObject(c)) { }
                        else if (IsEnemy(c)) { }
                        else if (IsItem(c)) { }
                        else if (char.IsDigit(c))
                        {
                            Vector2i tile = new(x, z);
                            if (!teleportTilePositions.ContainsKey(c))
                                teleportTilePositions[c] = new List<Vector2i>();

                            teleportTilePositions[c].Add(tile);
                            Console.WriteLine($"Found teleport '{c}' at tile ({x}, {z})");
                        }
                    }
                }

                foreach (var pair in teleportTilePositions)
                {
                    var id = pair.Key;
                    var tiles = pair.Value;

                    if (tiles.Count >= 2)
                    {
                        Vector2i sourceTile = tiles[0];
                        Vector2i targetTile = tiles[1];
                        Vector3 sourcePosition = new Vector3(sourceTile.X * wallLength, 0, sourceTile.Y * wallLength);
                        Vector3 targetPosition = new Vector3(targetTile.X * wallLength, 0, targetTile.Y * wallLength);

                        var trigger = new TeleportTrigger
                        {
                            Id = id,
                            Position = sourcePosition,
                            TargetPosition = targetPosition,
                            Shader = shader
                        };

                        Vector2i tile = new Vector2i(sourceTile.X, sourceTile.Y);
                        triggerMap[tile] = trigger;
                        renderables.Add(trigger);
                        Teleports.Add(new Teleport(id, sourcePosition, targetPosition));

                        Console.WriteLine($"Teleport {id} at {sourcePosition}, target: {targetPosition}");

                        if (tiles.Count > 2)
                        {
                            Console.WriteLine($"Teleport '{id}' has more than 2 positions. Only the first 2 will be used.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Teleport '{id}' has less than 2 points.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read map file:");
                Console.WriteLine(e.Message);
            }
        }

        private bool IsWall(char c) => c >= 'o' && c <= 'z';
        private bool IsStartPosition(char c) => c == '@';
        private bool IsLight(char c) => c == '*' || c == '^' || c == '!';
        private bool IsDoor(char c) => c >= 'A' && c <= 'G';
        private bool IsSolidObject(char c) => c >= 'H' && c <= 'N';
        private bool IsEnemy(char c) => c >= 'O' && c <= 'R';
        private bool IsItem(char c) => c >= 'T' && c <= 'Z';
    }
}
