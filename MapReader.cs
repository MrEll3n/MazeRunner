using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace ZPG
{
    public class MapReader
    {
        private readonly List<Wall> walls = new();
        private readonly Shader shader;
        private readonly int wallLength = 2;
        public Vector3 playerStartPosition { get; private set; }

        public MapReader(Shader shader)
        {
            this.shader = shader;
        }

        public void ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Soubor nenalezen: " + filePath);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                {
                    Console.WriteLine("Soubor neobsahuje dostatek řádků.");
                    return;
                }

                // Získání rozměrů z prvního řádku (např. "24x17")
                string[] dimensions = lines[0].Split('x');
                int width = int.Parse(dimensions[0]);
                int height = int.Parse(dimensions[1]);

                // Kontrola, že máme dostatek dat
                if (lines.Length - 1 < height)
                {
                    Console.WriteLine("Mapový soubor neobsahuje očekávaný počet řádků.");
                    return;
                }

                for (int z = 0; z < height; z++)
                {
                    string line = lines[z + 1];
                    for (int x = 0; x < width; x++)
                    {
                        if (x >= line.Length)
                            continue; // ochrana proti kratším řádkům

                        char c = line[x];
                        Vector3 position = new Vector3(x * wallLength, 0, z * wallLength);

                        if (IsWall(c))
                        {
                            Wall wall = new()
                            {
                                Shader = shader,
                                Position = position
                            };
                            walls.Add(wall);
                        }
                        else if (IsStartPosition(c))
                        {
                            playerStartPosition = position;
                        }
                        else if (IsLight(c))
                        {
                            // TODO: Přidání světla
                        }
                        else if (IsDoor(c))
                        {
                            // TODO: Dveře
                        }
                        else if (IsSolidObject(c))
                        {
                            // TODO: Pevný objekt
                        }
                        else if (IsEnemy(c))
                        {
                            // TODO: Nepřítel
                        }
                        else if (IsItem(c))
                        {
                            // TODO: Item
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Nepodařilo se načíst soubor:");
                Console.WriteLine(e.Message);
            }
        }

        public List<Wall> GetWalls()
        {
            return walls;
        }

        public Vector3 GetPlayerStartPosition()
        {
            return playerStartPosition;
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
