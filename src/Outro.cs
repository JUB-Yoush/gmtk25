using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raylib_cs;

namespace Outro;

public static class OutroHandler
{
    public static Color col = new Color(0, 0, 0, 0 );
    public static Rectangle rect = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    
    public static float fadeSpeed = 2.0f;

    public static bool fadeDone = false;

    public static float alphaCheck = 0;
    
    public static void Update()
    {

        if (!fadeDone)
        {
            Raylib.DrawRectangleRec(rect, col);
            if (alphaCheck < 255)
            {
                col.A += (byte)fadeSpeed;
                alphaCheck += fadeSpeed;
                if (alphaCheck >= 255)
                {

                    Console.WriteLine("The fade is most fading");
                    fadeDone = true;
                }

            }
        }
        else
        {
            Raylib.DrawRectangleRec(rect, Color.Black);
        }



    }
    }
 