using Puzzles;
using Raylib_cs;

namespace Gmtk;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(Draw.screenWidth, Draw.screenHeight, "Hello World");
        Raylib.SetTargetFPS(60);

        GlobalGameState.currentState = GameStates.GAME; //BUGTEST: Set global var

        Puzzle g = PuzzleLoader.LoadPuzzle();
        Draw.SetupRenderer();
        Draw.LoadTextures();

        while (!Raylib.WindowShouldClose())
        {
            switch (GlobalGameState.currentState)
            {
                case GameStates.TITLE:
                    break;
                case GameStates.INTRO:
                    Puzzle.Update(g);
                    Draw.DrawFrame(g);
                    break;
                case GameStates.GAME:

                    Puzzle.Update(g);
                    Draw.DrawFrame(g);
                    if (Raylib.IsKeyPressed(KeyboardKey.R) || GlobalGameState.reseting)
                    {
                        g = PuzzleLoader.LoadPuzzle();
                        GlobalGameState.reseting = false;
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.One) || GlobalGameState.changingPuzzle)
                    {
                        PuzzleLoader.puzzleIndex++;
                        g = PuzzleLoader.LoadPuzzle();
                        GlobalGameState.changingPuzzle = false;
                    }

                    break;
                case GameStates.OUTRO:
                    break;
            }
        }

        Raylib.CloseWindow();
    }
}
