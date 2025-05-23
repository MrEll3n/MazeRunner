using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Responsible for reading a map definition file and generating game entities
    /// such as walls, collectibles, teleporters, and the player's starting position.
    /// </summary>
    public class MapReader
    {
        private readonly List<Wall> walls = new();
        private readonly List<Model> renderables = new();
        private readonly Dictionary<Vector2i, ITriggerZone> triggerMap = new();

        private readonly Dictionary<char, List<Vector2i>> teleportTilePositions = new();
        private readonly Shader shader;
        private readonly int wallLength = 2;

        /// <summary>
        /// Gets the player's starting position in the map.
        /// </summary>
        public Vector3 PlayerStartPosition { get; private set; }
        /// <summary>
        /// Gets the list of teleport connections generated from the map.
        /// </summary>
        public List<Teleport> Teleports { get; private set; } = new();

        /// <summary>
        /// Returns a list of wall objects generated from the map file.
        /// </summary>
        public List<Wall> GetWalls() => walls;
        /// <summary>
        /// Returns a list of all renderable models created from the map.
        /// </summary>
        public List<Model> GetRenderables() => renderables;
        /// <summary>
        /// Returns the player's starting position.
        /// </summary>
        public Vector3 GetPlayerStartPosition() => PlayerStartPosition;
        /// <summary>
        /// Gets the map of trigger zones indexed by tile position.
        /// </summary>
        public Dictionary<Vector2i, ITriggerZone> GetTriggerMap() => triggerMap;

        /// <summary>
        /// Returns a list of all teleport triggers present in the renderables list.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MapReader"/> class using the specified shader.
        /// </summary>
        /// <param name="shader">The shader to apply to generated models.</param>
        public MapReader(Shader shader)
        {
            this.shader = shader;
            Console.WriteLine("[MapReader] Inicializován");
        }

        /// <summary>
        /// Returns a list of all collectibles found in the renderables list.
        /// </summary>
        public List<Collectible> GetCollectibles()
        {
            List<Collectible> collectibles = new();
            foreach (var model in renderables)
            {
                if (model is Collectible collectible)
                    collectibles.Add(collectible);
            }
            return collectibles;
        }

        /// <summary>
        /// Reads and parses the map definition file at the given path and generates the corresponding game objects.
        /// </summary>
        /// <param name="filePath">The path to the map file.</param>
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

                int? startX = null;
                int? startZ = null;

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
                            startX = x;
                            startZ = z;
                        }
                        else if (IsLight(c)) { }
                        else if (IsDoor(c)) { }
                        else if (IsSolidObject(c)) { }
                        else if (IsEnemy(c)) { }
                        else if (IsItem(c)) 
                        {
                            
                            Vector3 collectiblePosition = new Vector3(x * wallLength, 1.0f, z * wallLength);
                            Vector2 collectibleSize = new Vector2(1.2f, 1.2f); // např. velikost mince

                            var collectible = new Collectible(collectiblePosition, collectibleSize)
                            {
                                Shader = shader,
                                TextureID = TextureLoader.LoadTexture("textures/paper.png"),
                                OnCollected = () => Console.WriteLine($"[Collectible] Player collected item at ({x}, {z})")
                            };

                            renderables.Add(collectible);
                            
                        }
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

                // Zpracování teleportů
                ProcessTeleportPositions();

                Console.WriteLine($"[DEBUG] Celkem načteno teleportů: {Teleports.Count}");
                foreach (var t in Teleports)
                {
                    Console.WriteLine($"  - {t.Id}: {t.SourcePosition} -> {t.TargetPosition}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read map file:");
                Console.WriteLine(e.Message);
            }
        }

        private void ProcessTeleportPositions()
        {
            Console.WriteLine($"[MapReader] Zpracovávám {teleportTilePositions.Count} různých teleportů");
            
            foreach (var pair in teleportTilePositions)
            {
                var id = pair.Key;
                var tiles = pair.Value;

                Console.WriteLine($"[MapReader] Teleport '{id}' má {tiles.Count} pozic");

                // Pokud teleport má aspoň 2 pozice, vytvoříme obousměrný teleport
                if (tiles.Count >= 2)
                {
                    // Pro každou pozici vytvoříme teleport na další pozici v kruhu
                    for (int i = 0; i < tiles.Count; i++)
                    {
                        Vector2i sourceTile = tiles[i];
                        // Target je další pozice v kruhu (nebo první, pokud jsme na konci)
                        Vector2i targetTile = tiles[(i + 1) % tiles.Count];

                        // Nastavení pozic - umístěno nad podlahu pro lepší viditelnost
                        Vector3 sourcePosition = new Vector3(sourceTile.X * wallLength, 1.5f, sourceTile.Y * wallLength);
                        Vector3 targetPosition = new Vector3(targetTile.X * wallLength, 1.5f, targetTile.Y * wallLength);

                        // Vytvoření teleportu s vlastními parametry
                        var trigger = new TeleportTrigger()
                        {
                            Id = id,
                            Shader = shader,
                            TargetPosition = targetPosition,
                            DelayBeforeTeleport = 1.0f, // Nastavení delay na 1 sekundu
                            CooldownAfterTeleport = 2.0f // Nastavení cooldownu na 2 sekundy
                        };
                        
                        // Nastavíme pozici a uložíme základní výšku
                        trigger.SetBasePosition(sourcePosition);
                        
                        // Načtení textury
                        if (trigger.TextureID == 0)
                        {
                            Console.WriteLine($"[Warning] Teleport '{id}' nemá texturu! Zkontroluj textures/teleport.jpg");
                        }

                        Console.WriteLine($"[Teleport] Vytvořen teleport ID: {trigger.Id}, Pozice: {trigger.Position} -> {trigger.TargetPosition}, TextureID: {trigger.TextureID}");

                        // Přidáme teleport do seznamu a mapy
                        triggerMap[sourceTile] = trigger;
                        renderables.Add(trigger);
                        Teleports.Add(new Teleport(id, sourcePosition, targetPosition));
                    }
                }
                else
                {
                    Console.WriteLine($"[Error] Teleport '{id}' má méně než 2 body, což je nedostatečné.");
                }
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