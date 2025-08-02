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
