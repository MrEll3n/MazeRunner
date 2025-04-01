using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace ZPG
{
    public class MapReader
    {
        private readonly List<Cube> walls = new();
        private readonly Shader shader;
        private readonly int wallLength = 2;
        public Vector3 playerStartPosition {get; private set;}

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
                int height = 0;

                foreach (var line in File.ReadLines(filePath))
                {
                    int width = 0;

                    foreach (char c in line)
                    {
                        Vector3 position = new Vector3(width * wallLength, 0, height * wallLength);

                        if (IsWall(c))
                        {
                            Cube wall = new()
                            {
                                Shader = shader,
                                Position = position
                            };
                            walls.Add(wall);
                        }
                        else if (IsStartPosition(c))
                        {
                            // TODO: Vytvoření hráče nebo startovního objektu na této pozici
                            playerStartPosition = position;
                        }
                        else if (IsLight(c))
                        {
                            // TODO: Přidání světelného objektu
                        }
                        else if (IsDoor(c))
                        {
                            // TODO: Přidání dveří
                        }
                        else if (IsSolidObject(c))
                        {
                            // TODO: Pevné objekty
                        }
                        else if (IsEnemy(c))
                        {
                            // TODO: Cizí postavy / protivníci
                        }
                        else if (IsItem(c))
                        {
                            // TODO: Předměty ke sběru
                        }

                        width++;
                    }

                    height++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Nepodařilo se načíst soubor:");
                Console.WriteLine(e.Message);
            }
        }

        public List<Cube> GetWalls()
        {
            return walls;
        }

        public Vector3 GetPlayerStartPosition()
        {
            return playerStartPosition;
        }

        // 🧱 Typové rozpoznávače znaků podle legendy:

        private bool IsWall(char c) => c >= 'o' && c <= 'z';

        private bool IsStartPosition(char c) => c == '@';

        private bool IsLight(char c) => c == '*' || c == '^' || c == '!';

        private bool IsDoor(char c) => c >= 'A' && c <= 'G';

        private bool IsSolidObject(char c) => c >= 'H' && c <= 'N';

        private bool IsEnemy(char c) => c >= 'O' && c <= 'R';

        private bool IsItem(char c) => c >= 'T' && c <= 'Z';
    }
}