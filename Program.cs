using System;

namespace MrEllen.ZPG
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(800, 600, "ZPG");
            game.Run();
        }
    }
}
