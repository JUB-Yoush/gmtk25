global using Vec2 = System.Numerics.Vector2;
global using Vec2i = Helper.Vec2i;
using System.Globalization; //(.Y.) jasmine is listening
using DialogueParser;
public enum GameStates
{
    TITLE,
    INTRO,
    GAME,
    OUTRO,
    SETTINGS,
}

public static class GlobalGameState
{
    public static List<Dialogue> dialogue = new List<Dialogue>();
    public static int dialogueIndex;
    public static GameStates currentState = GameStates.TITLE; // 0 = title, 1 = intro, 2 = game, 3 = outro
    public static bool undoPressed = false;
    public static bool resetPressed = false;
    public static bool solvePressed = false;
    public static bool undoHover = false;
    public static bool resetHover = false;
    public static bool solveHover = false;
    public static bool reseting = false;
    public static bool changingPuzzle = false;
}


