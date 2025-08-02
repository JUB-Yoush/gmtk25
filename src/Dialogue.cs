using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace DialogueParser;

class Dialogue
{
    public string speaker;
    public string text;
    public string emotion;

    public override string ToString()
    {
        return String.Format("{0}   {1}   {2}", speaker, text, emotion);
    }
}

static class DialogueManager
{
    public static List<Dialogue> LoadDialogue()
    {
        List<Dialogue> dialogues = new List<Dialogue>();
        JObject dialogueData = JObject.Parse(File.ReadAllText("./assets/OpeningDialogue.json"));
        JArray dialogue = (JArray)dialogueData["dialogue"];

        /*
          JValue speakers = (JValue)dialogue[0]["speaker"];
          JValue text = (JValue)dialogue[0]["text"];
          JValue emotion = (JValue)dialogue[0]["emotion"];
  
  
          Console.WriteLine(speakers.ToString(Formatting.Indented));
          */

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
