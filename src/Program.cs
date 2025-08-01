using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Mail;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Helper;
using Puzzles;
using Raylib_cs;

namespace Gmtk;

class Program
{

    public static void Main()
    {
        Raylib.InitWindow(720, 512, "Hello World");
        Raylib.SetTargetFPS(60);

        GlobalGameState.GameState = 2; //BUGTEST: Set global var

        Puzzle g = PuzzleLoader.LoadPuzzle();
        Draw.LoadTextures();

        while (!Raylib.WindowShouldClose())
        {

            switch (GlobalGameState.GameState)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:

                    Puzzle.Update(g);
                    Draw.DrawFrame(g);
                    if (Raylib.IsKeyPressed(KeyboardKey.R))
                    {
                        g = PuzzleLoader.LoadPuzzle();
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.One))
                    {
                        PuzzleLoader.puzzleIndex++;
                        g = PuzzleLoader.LoadPuzzle();
                    }


                    break;
                case 3:
                    break;
            }



        }


        Raylib.CloseWindow();
    }
}
