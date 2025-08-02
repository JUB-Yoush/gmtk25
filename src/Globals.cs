global using Vec2 = System.Numerics.Vector2;
global using Vec2i = Helper.Vec2i;
using System.Globalization; //(.Y.) jasmine is listening
using System.Security.Cryptography.X509Certificates;
using DialogueParser;
public enum GameStates
{
    TITLE,
    INTRO,
    GAME,
    OUTRO,
    SETTINGS,
}

public enum SpeakerEmotion
{
    ANGRY = 0,
    HAPPY = 1,
    NEUTRAL = 2,
    SAD = 3,
}
public enum CurrentSpeaker
{
    MAC = 1,
    ASTRID = 0,
}

public static class GlobalGameState
{
    public static List<Dialogue> dialogue = new List<Dialogue>();
    public static int dialogueIndex;
    //18 lines total for the intro
    public static void IncrementDI()
    {

        if (dialogueIndex < dialogue.Count)
        {
            dialogueIndex++;
        
       
      
            
        }
    
    }

    public static void UpdateSpeaker()
    {
            string whoTalking = dialogue[dialogueIndex].speaker.ToString();
            switch (whoTalking)
            {
                case "Mac":
                    currSpeaker = CurrentSpeaker.MAC;
                    break;
                case "Astrid":
                    currSpeaker = CurrentSpeaker.ASTRID;
                    break;
            } 
    }

    public static void UpdateEmotion()
    {
        string whatEmotion = dialogue[dialogueIndex].emotion.ToString();
            switch (whatEmotion)
            {
                case "happy":
                    currEmotion = SpeakerEmotion.HAPPY;
                    break;
                case "angry":
                    currEmotion = SpeakerEmotion.ANGRY;
                    break;
                case "sad":
                    currEmotion = SpeakerEmotion.SAD;
                    break;
                case "neutral":
                    currEmotion = SpeakerEmotion.NEUTRAL;
                    break;
            }
    }
    public static SpeakerEmotion currEmotion = SpeakerEmotion.ANGRY;
    public static CurrentSpeaker currSpeaker = CurrentSpeaker.MAC;
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


