using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace ZPG
{
    /// <summary>
    /// Slouží k načítání mapy ze souboru a převodu znakové reprezentace na 3D objekty.
    /// </summary>
    public class MapReader
    {
        private readonly List<Wall> walls = new();
        private List<Model> renderables = new();

        /// <summary>
        /// Vrací seznam všech renderovatelných objektů načtených z mapy.
        /// </summary>
        public List<Model> GetRenderables() => renderables;

        private readonly Shader shader;

        /// <summary>
        /// Délka jednoho "čtverce" ve světových jednotkách (např. 2 metry).
        /// </summary>
        private readonly int wallLength = 2;

        /// <summary>
        /// Výchozí pozice hráče podle mapového souboru.
        /// </summary>
        public Vector3 playerStartPosition { get; private set; }

        /// <summary>
        /// Inicializuje nový <see cref="MapReader"/> s daným shaderem, který se použije pro objekty.
        /// </summary>
        /// <param name="shader">Shader, který se přiřadí novým objektům ze souboru.</param>
        public MapReader(Shader shader)
        {
            this.shader = shader;
        }

        /// <summary>
        /// Načte mapu ze souboru a vytvoří 3D objekty podle znaků.
        /// </summary>
        /// <param name="filePath">Cesta k mapovému souboru.</param>
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

                // Kontrola, že soubor má dostatek řádků pro mapu
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

                        // Rozlišení podle typu znaku
                        if (IsWall(c))
                        {
                            Wall wall = new()
                            {
                                Shader = shader,
                                Position = position
                            };
                            walls.Add(wall);
                            renderables.Add(wall);
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
                            // TODO: Přidání dveří
                        }
                        else if (IsSolidObject(c))
                        {
                            // TODO: Pevný objekt (např. sloup, kvádr)
                        }
                        else if (IsEnemy(c))
                        {
                            // TODO: Nepřátelská jednotka
                        }
                        else if (IsItem(c))
                        {
                            // TODO: Sbíratelný předmět
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

        /// <summary>
        /// Vrací seznam všech stěn načtených z mapy.
        /// </summary>
        public List<Wall> GetWalls()
        {
            return walls;
        }

        /// <summary>
        /// Vrací výchozí pozici hráče definovanou v mapě.
        /// </summary>
        public Vector3 GetPlayerStartPosition()
        {
            return playerStartPosition;
        }

        // Metody pro rozpoznání typu znaku v mapovém souboru

        /// <summary>
        /// Vrací true, pokud znak reprezentuje stěnu.
        /// </summary>
        private bool IsWall(char c) => c >= 'o' && c <= 'z';

        /// <summary>
        /// Vrací true, pokud znak reprezentuje výchozí pozici hráče.
        /// </summary>
        private bool IsStartPosition(char c) => c == '@';

        /// <summary>
        /// Vrací true, pokud znak reprezentuje světelný zdroj.
        /// </summary>
        private bool IsLight(char c) => c == '*' || c == '^' || c == '!';

        /// <summary>
        /// Vrací true, pokud znak reprezentuje dveře.
        /// </summary>
        private bool IsDoor(char c) => c >= 'A' && c <= 'G';

        /// <summary>
        /// Vrací true, pokud znak reprezentuje nepohyblivý objekt.
        /// </summary>
        private bool IsSolidObject(char c) => c >= 'H' && c <= 'N';

        /// <summary>
        /// Vrací true, pokud znak reprezentuje nepřítele.
        /// </summary>
        private bool IsEnemy(char c) => c >= 'O' && c <= 'R';

        /// <summary>
        /// Vrací true, pokud znak reprezentuje předmět.
        /// </summary>
        private bool IsItem(char c) => c >= 'T' && c <= 'Z';
    }
}
