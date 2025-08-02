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
        //return string.Format("{0}   {1}   {2}", speaker, text, emotion);
        return $"{speaker} {text} {emotion}";
    }
}

public static class DialogueManager
{
    public static List<Dialogue> LoadDialogue()
    {
        List<Dialogue> dialogues = new List<Dialogue>();
        JObject dialogueData = JObject.Parse(File.ReadAllText("./assets/OpeningDialogue.json"));
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
    public static int maxLineChar = textBoxWidth/8;

    public static bool hoverOnArrow;
    public static int arrowBoxWidth = 130;
    public static int arrowBoxHeight = 115;
    public static int arrowBoxX = 550;
    public static int arrowBoxY = 360;

    public static readonly Rectangle StartBtnBox = new(arrowBoxX, arrowBoxY, arrowBoxWidth, arrowBoxHeight);


    public static void Update()
    {

        Vec2 mousePos = Raylib.GetMousePosition();
        Rectangle mouseHbox = new(mousePos.X, mousePos.Y, 4, 4);

        Raylib.DrawRectangleLinesEx(StartBtnBox,2, Color.Red);

        hoverOnArrow = Raylib.CheckCollisionRecs(mouseHbox, StartBtnBox);

        if (hoverOnArrow && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {  
            GlobalGameState.IncrementDI();   
        }

        //word wrapping ref: https://www.raylib.com/examples/text/loader.html?name=text_rectangle_bounds
        Raylib.DrawText(
            GlobalGameState.dialogue[GlobalGameState.dialogueIndex].speaker,
            textPosX,
            343,
            20,
            Draw.textCol
        );


        List<String> dialogue = CutDialogue(GlobalGameState.dialogue[GlobalGameState.dialogueIndex].text.ToString());

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


    public static List<String> CutDialogue(String dialogue)
    {

        List<String> cutDialogue = new List<String>();

        char[] seperators = new char[] { ' ', '.', ',', '!', '?', ';', ':', '—'};

        int index = 0;
          String line = "";

        if (dialogue.Length <= maxLineChar)
        {
            cutDialogue.Add(dialogue);
            return cutDialogue;
          }
     
      while (index < dialogue.Length)
        {



            if ((index + maxLineChar) >= dialogue.Length)
            {
                line = dialogue.Substring(index);
            }
            else
            {

                line = dialogue.Substring(index, index + maxLineChar); //cut up to 30~ characters

                char lastChar = line[line.Length - 1];

                //if it is a letter, we need to add a hyphen
                if (!seperators.Contains(lastChar))
                {
                    line += "-"; //hyphenate if we are cut in the middle of a sentence
                }


            }
            index += maxLineChar;
            Console.WriteLine(line);
            cutDialogue.Add(line);
        }
           

        return cutDialogue;
    }

       
    

 
}
