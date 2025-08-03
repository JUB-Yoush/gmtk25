using DialogueParser;
using Puzzles;
using Raylib_cs;

namespace Gmtk;

class Program
{
    public static void Main()
    {
        Raylib.InitWindow(Draw.screenWidth, Draw.screenHeight, "Hello World");
        Raylib.SetTargetFPS(60);

        Puzzle g = PuzzleLoader.LoadPuzzle();
        GlobalGameState.dialogue = DialogueManager.LoadDialogue(); //LoadDialogue() produces a dialogue list

        AudioManager.LoadAudio();
        Draw.SetupRenderer();
        Draw.LoadTextures();
        AudioManager.playBGM("intro");

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsKeyPressed(KeyboardKey.F11))
            {
                Raylib.ToggleBorderlessWindowed();
                GlobalGameState.fullscreen = !GlobalGameState.fullscreen;
                if (GlobalGameState.fullscreen)
                {
                    Draw.vScale =
                        (float)Raylib.GetMonitorHeight(Raylib.GetCurrentMonitor())
                        / (float)Draw.V_SCREEN_Y;
                }
                else
                {
                    Draw.vScale = 1;
                }
                Draw.SetupRenderer();
            }
            switch (GlobalGameState.currentState)
            {
                case GameStates.TITLE:

                    TitleHandler.Update();
                    Draw.DrawFrame(g);
                    break;
                case GameStates.INTRO:
                    Draw.DrawFrame(g);

                    break;
                case GameStates.GAME:

                    Puzzle.Update(g);
                    Draw.DrawFrame(g);
                    if (Raylib.IsKeyPressed(KeyboardKey.R) || GlobalGameState.reseting)
                    {
                        AudioManager.playSFX("click");
                        g = PuzzleLoader.LoadPuzzle();
                        GlobalGameState.reseting = false;
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.One) || GlobalGameState.changingPuzzle)
                    {
                        PuzzleLoader.puzzleIndex++;
                        g = PuzzleLoader.LoadPuzzle();
                        GlobalGameState.changingPuzzle = false;
                        if (
                            PuzzleLoader.puzzleIndex == 1
                            || PuzzleLoader.puzzleIndex == 4
                            || PuzzleLoader.puzzleIndex == 10
                        )
                        {
                            AudioManager.playBGM("intro");
                            GlobalGameState.currentState = GameStates.INTRO;
                        }
                    }

                    break;

                case GameStates.SETTINGS:

                    break;
                case GameStates.OUTRO:
                    Draw.DrawFrame(g);
                    break;
            }
            AudioManager.update();
        }

        Raylib.CloseWindow();
    }
}
