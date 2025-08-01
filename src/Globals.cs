global using Vec2 = System.Numerics.Vector2;
global using Vec2i = Helper.Vec2i; //(.Y.) jasmine is listening

public enum GameStates
{
    TITLE,
    INTRO,
    GAME,
    OUTRO,
}

public static class GlobalGameState
{
    public static GameStates currentState = GameStates.TITLE; // 0 = title, 1 = intro, 2 = game, 3 = outro
}
