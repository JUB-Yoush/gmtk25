using Puzzles;
using Raylib_cs;
using Title;
using DialogueParser;

namespace Gmtk;

class Program
{

    public static void Main()
    {
        Raylib.InitWindow(Draw.screenWidth, Draw.screenHeight, "Hello World");
        Raylib.SetTargetFPS(60);

        GlobalGameState.currentState = GameStates.TITLE;//BUGTEST: Set global var
        

        Puzzle g = PuzzleLoader.LoadPuzzle();
        List<Dialogue> dialogues =  DialogueManager.LoadDialogue(); //LoadDialogue() produces a dialogue list
       
        
        Draw.SetupRenderer();
        Draw.LoadTextures();
       

        while (!Raylib.WindowShouldClose())
        {

            switch (GlobalGameState.currentState)
            {
                case GameStates.TITLE:
                    
                    TitleHandler.Update();
                    Draw.DrawFrame(g);
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

                case GameStates.SETTINGS:

                    break;
                case GameStates.OUTRO:
                    break;
            }



        }


        Raylib.CloseWindow();
    }
}
