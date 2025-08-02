using Raylib_cs;

public static class AudioManager
{
    public static Music IntroBGM;
    public static Music PuzzleBGM;
    public static Music CurrentBGM;

    public static Sound shiftSFX;
    public static Sound solveSFX;
    public static Sound clickSFX;
    public static Sound callSFX;
    public static Sound nomoveSFX;

    public static void LoadAudio()
    {
        Raylib.InitAudioDevice();
        shiftSFX = Raylib.LoadSound("./assets/audio/shift.wav");
        solveSFX = Raylib.LoadSound("./assets/audio/solve.wav");
        clickSFX = Raylib.LoadSound("./assets/audio/undo.wav");
        callSFX = Raylib.LoadSound("./assets/audio/call.wav");
        nomoveSFX = Raylib.LoadSound("./assets/audio/nomove.wav");

        IntroBGM = Raylib.LoadMusicStream("./assets/audio/breaking_the_ice.ogg");
        PuzzleBGM = Raylib.LoadMusicStream("./assets/audio/lost_in_space_with_you.ogg");
    }

    public static void playSFX(string id)
    {
        switch (id)
        {
            case ("shift"):
                Raylib.PlaySound(shiftSFX);
                break;
            case ("solve"):
                Raylib.PlaySound(solveSFX);
                break;
            case ("click"):
                Raylib.PlaySound(clickSFX);
                break;
            case ("call"):
                Raylib.PlaySound(callSFX);
                break;
            case ("nomove"):
                Raylib.PlaySound(nomoveSFX);
                break;
        }
    }

    public static void playBGM(string id)
    {
        switch (id)
        {
            case ("puzzle"):
                Raylib.StopMusicStream(PuzzleBGM);
                CurrentBGM = PuzzleBGM;
                Raylib.PlayMusicStream(PuzzleBGM);
                break;
            case ("intro"):
                Raylib.StopMusicStream(IntroBGM);
                CurrentBGM = IntroBGM;
                Raylib.PlayMusicStream(IntroBGM);
                break;
        }
    }

    public static void update()
    {
        Raylib.UpdateMusicStream(CurrentBGM);
    }
}
