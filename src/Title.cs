using System.Data.Common;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Helper;
using Raylib_cs;

//Namespace -- break up code so it can be easier to understand and maintain within itself
//avoid conflicts w/ names in our code

namespace Title;

public class DrawTitle()
{
    static Image frame = Raylib.LoadImage("./assets/images/TalkFrame.png");
    Texture2D frameTex = Raylib.LoadTextureFromImage(frame);
    
}