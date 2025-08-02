using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Helper;
using Raylib_cs;

//Namespace -- break up code so it can be easier to understand and maintain within itself
//avoid conflicts w/ names in our code

public static class TitleHandler
{
    public static readonly Rectangle StartBtnBox = new(244, 301, 272, 112);

    public static void Update()
    {
        Vec2 mousePos = Raylib.GetMousePosition();
        Rectangle mouseHbox = new(mousePos.X, mousePos.Y, 4, 4);
        if (
            Raylib.CheckCollisionRecs(mouseHbox, StartBtnBox)
            && Raylib.IsMouseButtonPressed(MouseButton.Left)
        )
        {
            GlobalGameState.currentState = GameStates.INTRO;
        }
    }
}
