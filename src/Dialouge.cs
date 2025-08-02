using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace DialogueParser;

class Dialogue
{

   public static void LoadDialogue()
   {

      JObject dialogueData = JObject.Parse(File.ReadAllText("./assets/OpeningDialogue.json"));
      JObject speakers = (JObject)dialogueData["speaker"];
      JObject dialogue = (JObject)dialogueData["text"];
      JObject emotion = (JObject)dialogueData["emotion"];

  
      
   }

  


}
