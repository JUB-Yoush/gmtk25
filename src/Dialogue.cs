using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace DialogueParser;

public class Dialogue
{
    public string speaker;
    public string text;
    public string emotion;

    public override string ToString()
    {
        return $"{speaker} {text} {emotion}";
    }
}

public static class DialogueManager
{
    public static List<Dialogue> LoadDialogue()
    {
        List<Dialogue> dialogues = new List<Dialogue>();
        JObject dialogueData = JObject.Parse(File.ReadAllText("./assets/Dialogue.json"));
        JArray dialogue = (JArray)dialogueData["dialogue"];

        foreach (JToken item in dialogue)
        {
            Dialogue obj = new Dialogue();
            obj.speaker = (string)item["speaker"];
            obj.text = (string)item["text"];
            obj.emotion = (string)item["emotion"];
            //Console.WriteLine(obj.ToString());
            dialogues.Add(obj);
        }

        return dialogues;
    }
}

public static class DialogueHandler
{
    public static int textBoxWidth = 440;
    public static int textBoxHeight = 90;

    public static int textPosX = 55;
    public static int textPosY = 380;

    public static int dialogueTextSize = 16;
    public static int maxLineChar = textBoxWidth / 8;

    public static bool hoverOnArrow;
    public static int arrowBoxWidth = 130;
    public static int arrowBoxHeight = 115;
    public static int arrowBoxX = 590;
    public static int arrowBoxY = 390;

    public static int speakerPosY = 60;
    public static int speakerPosX = 145;

    public static bool canDrawText = true;

    public static readonly Rectangle StartBtnBox = new(
        arrowBoxX,
        arrowBoxY,
        arrowBoxWidth,
        arrowBoxHeight
    );

    public static void Update()
    {
        Vec2 mousePos = Raylib.GetMousePosition();
        Rectangle mouseHbox = new(mousePos.X, mousePos.Y, 4, 4);

        hoverOnArrow = Raylib.CheckCollisionRecs(mouseHbox, StartBtnBox);

        if (hoverOnArrow && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            GlobalGameState.IncrementDI();
            canDrawText = GlobalGameState.dialogueIndex < GlobalGameState.dialogue.Count;
            if (canDrawText)
            {
                GlobalGameState.UpdateEmotion();
                GlobalGameState.UpdateSpeaker();
            }
            if (
                GlobalGameState.dialogueIndex == 18
                || GlobalGameState.dialogueIndex == 22
                || GlobalGameState.dialogueIndex == 30
            )
            {
                GlobalGameState.currentState = GameStates.GAME;
            }
            else if (GlobalGameState.dialogueIndex == 36)
            {
                AudioManager.StopBGM();
                GlobalGameState.currentState = GameStates.OUTRO;
            }
        }

        if (canDrawText)
        {
            //word wrapping ref: https://www.raylib.com/examples/text/loader.html?name=text_rectangle_bounds
            Raylib.DrawText(
                GlobalGameState.dialogue[GlobalGameState.dialogueIndex].speaker,
                textPosX,
                343,
                20,
                Draw.textCol
            );

            List<String> dialogue = CutDialogue(
                GlobalGameState.dialogue[GlobalGameState.dialogueIndex].text.ToString()
            );

            for (int i = 0; i < dialogue.Count; i++)
            {
                Raylib.DrawText(
                    dialogue[i],
                    textPosX,
                    textPosY + (i * dialogueTextSize),
                    dialogueTextSize,
                    Draw.textCol
                );
            }
        }
    }

    public static List<String> CutDialogue(String dialogue)
    {
        List<String> cutDialogue = new List<String>();

        char[] seperators = new char[] { ' ', '.', ',', '!', '?', ';', ':', '-' };

        String line = "";

        //the dialogue is already short to fit, pass it out of the function
        if (dialogue.Length <= maxLineChar)
        {
            cutDialogue.Add(dialogue);
            return cutDialogue;
        }

        for (int currChar = 0; currChar < dialogue.Length; currChar++)
        {
            line += dialogue[currChar];

            if (currChar > 0 && currChar % maxLineChar == 0)
            {
                if (!seperators.Contains(dialogue[currChar]))
                {
                    line += "-"; //hyphenate if we are cut in the middle of a sentence
                }

                cutDialogue.Add(line);
                line = "";
            }
            //Console.WriteLine($"{currChar}:{dialogue[currChar]}");
        }
        cutDialogue.Add(line);

        return cutDialogue;
    }
}
