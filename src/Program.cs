using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Mail;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Helper;
using Raylib_cs;

namespace Gmtk;

class Program
{
    public static readonly Vec2i GRID_START_POS = new(100, 100);
    public static readonly Vec2i BTN_START_POS = new(100 - 24, 100 - 24);

    public static void Main()
    {
        Raylib.InitWindow(800, 480, "Hello World");
        Raylib.SetTargetFPS(60);
        Game g = new();

        while (!Raylib.WindowShouldClose())
        {
            Puzzle.Update(g);
            Draw.DrawFrame(g);
            if (Raylib.IsKeyPressed(KeyboardKey.R))
            {
                g = new();
            }
        }

        Raylib.CloseWindow();
    }
}
