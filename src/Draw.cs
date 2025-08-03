using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using Helper;
using Puzzles;
using RayGUI_cs;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

public static class Draw
{
    public static RenderTexture2D renderTarget;
    public const int V_SCREEN_X = 720;
    public const int V_SCREEN_Y = 512;

    public static float vScale = 1f;
    public static int screenWidth = (int)(V_SCREEN_X * vScale);
    public static int screenHeight = (int)(V_SCREEN_Y * vScale);

    public static Rectangle sourceRec;
    public static Rectangle destRec;

    public static void SetupRenderer()
    {
        screenWidth = (int)(V_SCREEN_X * vScale);
        screenHeight = (int)(V_SCREEN_Y * vScale);
        renderTarget = Raylib.LoadRenderTexture(V_SCREEN_X, V_SCREEN_Y);
        sourceRec = new(0, 0, renderTarget.Texture.Width, -renderTarget.Texture.Height);
        destRec = new(-vScale, -vScale, screenWidth + (vScale * 2), screenHeight + (vScale * 2));
        Raylib.SetMouseScale(1 / Draw.vScale, 1 / Draw.vScale);
    }

    /*
     * misc rendering issues
     * figure out how to scale tiles or make border npatch or just make it blank
     * render static elements
     * set up puzzle ui buttons (reset,undo)
     * vn stuff?
     * render buttons as images to get hover working
     * set up virtual screen size
     */
    //use emuns dofus

    public static Font font;
    public static Color textCol = new Color(187, 255, 90);
    public static readonly Dictionary<string, string> texturePaths = new()
    {
        { "puzzleTiles", "./assets/images/hiresTiles.png" },
        { "gameScreen", "./assets/images/gameScreen.png" },
        { "talkFrame", "./assets/images/TalkFrame.png" },
        { "icons", "./assets/images/ui_spritesheet.png" },
        { "girls", "./assets/images/girls.png" },
        { "phone", "./assets/images/callSprites.png" },
    };

    public static Rectangle SolveHitbox = new(530, 403, 147, 65);
    public static Rectangle UndoHitbox = new(620, 256, 58, 107);
    public static Rectangle ResetHitbox = new(527, 255, 58, 107);

    public const int ICON_SIZE = 48;
    public static readonly Vec2 FACE_SIZE = new(146, 125);

    public static Dictionary<string, Texture2D> textures = [];

    public static Texture2D GetTexture(string id)
    {
        return textures[id];
    }

    public static void LoadTextures()
    {
        font = Raylib.LoadFont("./assets/fonts/verdana.ttf");
        foreach (var asset in texturePaths)
        {
            textures.Add(asset.Key, Raylib.LoadTexture(asset.Value));
        }
    }

    public static void DrawPuzzleBg(Puzzle g)
    {
        Color solveColor = g.solved ? Color.Green : Color.White;
        Color undoColor = GlobalGameState.undoHover ? Color.Green : Color.White;
        Color resetColor = GlobalGameState.resetHover ? Color.Green : Color.White;

        Raylib.DrawTexture(GetTexture("gameScreen"), 0, 0, Color.White);
        Raylib.DrawTextureRec(
            GetTexture("icons"),
            new(2 * ICON_SIZE, 0, ICON_SIZE, ICON_SIZE),
            new(585, 410),
            solveColor
        );
        Raylib.DrawTextureRec(
            GetTexture("icons"),
            new(3 * ICON_SIZE, 0, ICON_SIZE, ICON_SIZE),
            new(532, 285),
            resetColor
        );
        Raylib.DrawTextureRec(
            GetTexture("icons"),
            new(4 * ICON_SIZE, 0, ICON_SIZE, ICON_SIZE),
            new(625, 286),
            undoColor
        );
    }

    //TODO: Jasmine
    public static void DrawVNBg()
    {
        Raylib.DrawTexture(GetTexture("talkFrame"), 0, 0, Color.White);
    }

    public static void VNDialogue()
    {
        //word wrapping ref: https://www.raylib.com/examples/text/loader.html?name=text_rectangle_bounds
        Raylib.DrawText(
            GlobalGameState.dialogue[GlobalGameState.dialogueIndex].speaker,
            61,
            343,
            20,
            textCol
        );
        Raylib.DrawText(
            GlobalGameState.dialogue[GlobalGameState.dialogueIndex].text.ToString(),
            61,
            400,
            20,
            textCol
        );
    }

    public static void DrawPuzzle(Puzzle g)
    {
        g.route = Puzzle.getCircuitStatus(g.board).visited;
        TileType[,] board = g.board;
        int gap = Puzzle.TILEGAP;
        foreach (var btn in g.moveBtnMap)
        {
            Rectangle r = btn.Key;
            MoveBtn b = btn.Value;
            float rot = 0;
            if (b.type == RowOrCol.COL && b.dir == -1)
            {
                rot = 0;
            }
            if (b.type == RowOrCol.COL && b.dir == 1)
            {
                rot = 180;
            }
            if (b.type == RowOrCol.ROW && b.dir == 1)
            {
                rot = 90;
            }
            if (b.type == RowOrCol.ROW && b.dir == -1)
            {
                rot = 270;
            }

            DrawTile(new(r.X, r.Y), new(5, 0), rot);
        }

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                Vec2i position = Puzzle.GRID_START_POS + new Vec2i(x * gap, y * gap);
                switch (board[x, y])
                {
                    case TileType.EMPTY:
                        DrawTile(position, new(5, 1));
                        break;
                    case TileType.WIRE_UD:
                        DrawTile(position, getUV(x, y, new(3, 0), g.route));
                        break;
                    case TileType.WIRE_LR:
                        DrawTile(position, getUV(x, y, new(3, 0), g.route), 90);
                        break;
                    case TileType.WIRE_LD:
                        DrawTile(position, getUV(x, y, new(2, 0), g.route), 0);
                        break;
                    case TileType.WIRE_LU:
                        DrawTile(position, getUV(x, y, new(2, 0), g.route), 90);
                        break;
                    case TileType.WIRE_RU:
                        DrawTile(position, getUV(x, y, new(2, 0), g.route), 180);
                        break;
                    case TileType.WIRE_RD:
                        DrawTile(position, getUV(x, y, new(2, 0), g.route), 270);
                        break;
                    case TileType.NODE:
                        DrawTile(position, getUV(x, y, new(4, 0), g.route));
                        break;
                    case TileType.POSITIVE:
                        DrawTile(position, new(1, 0));
                        break;
                    case TileType.NEGATIVE:
                        DrawTile(position, new(0, 0));
                        break;
                    case TileType.BOX:
                        DrawTile(position, new(6, 0));
                        break;
                    case TileType.ROCK:
                        DrawTile(position, new(5, 1));
                        break;
                }
            }
        }

        // Raylib.DrawRectangle((int)g.mouseHitbox.X - 4, (int)g.mouseHitbox.Y - 4, 8, 8, Color.Green);
        // if (g.solved)
        // {
        //     Raylib.DrawText("solved", 10, 10, 20, Color.Black);
        // }
    }

    public static Vec2 getUV(int x, int y, Vec2i uv, List<Vec2i> elecRoute)
    {
        if (elecRoute.Contains(new(x, y)))
        {
            return (uv + Direction.DOWN).toVec2();
        }
        return uv;
    }

    public static void DrawTile(
        Vec2 position,
        Vec2 uv,
        float rotation = 0,
        bool flipx = false,
        bool flipy = false
    )
    {
        Rectangle sourceRect = new(
            uv.X * Puzzle.TILESIZE,
            uv.Y * Puzzle.TILESIZE,
            Puzzle.TILESIZE,
            Puzzle.TILESIZE
        );
        if (flipx)
        {
            sourceRect.Width *= -1;
        }
        if (flipy)
        {
            sourceRect.Height *= -1;
        }

        Rectangle destRect = new(position, Puzzle.TILESIZE, Puzzle.TILESIZE);
        Raylib.DrawTexturePro(
            GetTexture("puzzleTiles"),
            sourceRect,
            destRect,
            new Vec2(Puzzle.TILESIZE / 2, Puzzle.TILESIZE / 2),
            rotation,
            Color.White
        );
    }

    public static void DrawFace()
    {
        Raylib.DrawTextureRec(
            GetTexture("girls"),
            Puzzle.CurrentFaceRect,
            new Vec2(532, 202 - 125),
            Color.White
        );

        Puzzle.faceTimer -= Raylib.GetFrameTime();
        if (Puzzle.faceTimer <= 0)
        {
            Puzzle.ChangeFace();
        }
    }

    public static void DrawTitleScreen()
    {
        Color phoneColor = TitleHandler.hoverOnPhone ? Color.Green : Color.White;
        Raylib.DrawTexture(GetTexture("talkFrame"), 0, 0, Color.White);
        Raylib.DrawTextureRec(
            GetTexture("phone"),
            new(TitleHandler.phoneFrame * 320, 0, 320, 225),
            new(111, 55),
            phoneColor
        );
    }

    public static void DrawFrame(Puzzle g)
    {
        Raylib.BeginTextureMode(renderTarget);
        Raylib.ClearBackground(Color.Black);
        switch (GlobalGameState.currentState)
        {
            case GameStates.TITLE:
                DrawTitleScreen();
                break;
            case GameStates.GAME:
                DrawPuzzleBg(g);
                DrawFace();
                DrawPuzzle(g);
                break;
            case GameStates.INTRO:
                DrawVNBg();
                VNDialogue();
                break;
            case GameStates.OUTRO:
                DrawVNBg();
                VNDialogue();
                break;
            case GameStates.SETTINGS:
                break;
        }
        Raylib.EndTextureMode();
        Raylib.ClearBackground(Color.Black);
        Raylib.BeginDrawing();
        Raylib.DrawTexturePro(
            renderTarget.Texture,
            sourceRec,
            destRec,
            new Vec2(0, 0),
            0,
            Color.White
        );
        Raylib.EndDrawing();
    }
}
