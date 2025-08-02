using System.Data.Common;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Helper;
using Raylib_cs;

//Namespace -- break up code so it can be easier to understand and maintain within itself
//avoid conflicts w/ names in our code

public static class TitleHandler
{
    public static int phoneFrame = 0;
    public static bool hoverOnPhone = false;
    public static readonly Rectangle StartBtnBox = new(111, 55, 320, 255);

    public static int frame = 0;
    public static double frametimer = 0;
    public const int ANIM_FRAMES = 3;
    public const int ENTITY_TILE_SIZE = 16;
    public const int FPS = 10;

    public static void Update()
    {
        Vec2 mousePos = Raylib.GetMousePosition();
        Rectangle mouseHbox = new(mousePos.X, mousePos.Y, 4, 4);

        hoverOnPhone = Raylib.CheckCollisionRecs(mouseHbox, StartBtnBox);
        if (
            Raylib.CheckCollisionRecs(mouseHbox, StartBtnBox)
            && Raylib.IsMouseButtonPressed(MouseButton.Left)
        )
        {
            GlobalGameState.currentState = GameStates.INTRO;
        }

        //animate phone
        //
        frametimer = JLib.Wrap(
            frametimer + (FPS * Raylib.GetFrameTime()),
            0,
            Math.Pow(ANIM_FRAMES, 2.0)
        );
        phoneFrame = (int)JLib.Wrap(Math.Floor(frametimer / ANIM_FRAMES), 0, ANIM_FRAMES);
    }
}
