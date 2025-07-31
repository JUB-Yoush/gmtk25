using System.Data;
using System.Dynamic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Raylib_cs;

namespace Gmtk;

class Tile { } //I think you need an enumerator for every tile type, you don't need one for every whatever you said im just reminding you <3

enum TileType
{
    EMPTY,
    FILLED,
}

enum RowOrCol
{
    ROW,
    COL,
}

class MoveBtn(RowOrCol type, int dir, int index)
{
    public RowOrCol type = type;
    public int dir = dir;
    public int index = index;

    public override string ToString()
    {
        return $"{type}|{dir}|{index}";
    }
}

class Game
{
    public int[,] board = new int[4, 4];
    public Rectangle mouseHitbox = new();
    public Dictionary<Rectangle, MoveBtn> moveBtnMap = new();
}

class Program
{
    public static readonly Vec2i GRID_START_POS = new(100, 100);
    public static readonly Vec2i BTN_START_POS = new(100 - 24, 100 - 24);

    public static void Main()
    {
        Raylib.InitWindow(800, 480, "Hello World");
        Game g = new();
        int[,] board =
        {
            { 1, 1, 1, 1 },
            { 1, 0, 0, 1 },
            { 1, 0, 0, 1 },
            { 1, 1, 1, 1 },
        };
        g.board = board;
        g.moveBtnMap = populateBtnMap();

        while (!Raylib.WindowShouldClose())
        {
            Update(g);
            Draw(g);
        }

        Raylib.CloseWindow();
    }

    public static Dictionary<Rectangle, MoveBtn> populateBtnMap()
    {
        Dictionary<Rectangle, MoveBtn> res = new();

        for (int x = 1; x < 5; x++)
        {
            int gap = 32;
            Vec2i position = BTN_START_POS + new Vec2i(x * gap, 0);
            //position.X = 24;
            res.Add(new(position.toVec2(), 24, 24), new(RowOrCol.COL, -1, x - 1));
        }

        for (int x = 1; x < 5; x++)
        {
            int gap = 32;
            Vec2i position = BTN_START_POS + new Vec2i(x * gap, 5 * gap);
            //position.X = 24;
            res.Add(new(position.toVec2(), 24, 24), new(RowOrCol.COL, 1, x - 1));
        }

        for (int y = 1; y < 5; y++)
        {
            int gap = 32;
            Vec2i position = BTN_START_POS + new Vec2i(0, y * gap);
            res.Add(new(position.toVec2(), 24, 24), new(RowOrCol.ROW, -1, y - 1));
        }

        for (int y = 1; y < 5; y++)
        {
            int gap = 32;
            Vec2i position = BTN_START_POS + new Vec2i(5 * gap, y * gap);
            res.Add(new(position.toVec2(), 24, 24), new(RowOrCol.ROW, 1, y - 1));
        }

        /*

        for (int y = 0; y < Room.ROOM_SIZE.Y; y++)
        {
            room.floorlayer[0, y] = (int)TileType.WALL;
        }
        for (int y = 0; y < Room.ROOM_SIZE.Y; y++)
        {
            room.floorlayer[Room.MAX_ROOM_INDEX.X, y] = (int)TileType.WALL;
        }
        */
        return res;
    }

    public static void Update(Game g)
    {
        Vec2 mousePos = Raylib.GetMousePosition();
        g.mouseHitbox.X = mousePos.X;
        g.mouseHitbox.Y = mousePos.Y;

        MoveBtn? overlapping = null;
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            foreach (var btn in g.moveBtnMap)
            {
                Rectangle r = btn.Key;
                if (Raylib.CheckCollisionRecs(r, g.mouseHitbox))
                {
                    overlapping = btn.Value;
                    Console.WriteLine(btn.Value.ToString());
                }
            }
        }
        if (overlapping == null)
        {
            return;
        }

        // get relevant row or column
        // shift all values around (wrap first and last)
        //update board if not done in place
        if (overlapping.type == RowOrCol.ROW && overlapping.dir == 1)
        {
            //shift right
            int[] row = GetRow(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int x = 1; x < 4; x++)
            {
                g.board[x, overlapping.index] = row[x - 1];
            }
            g.board[0, overlapping.index] = row[3];
        }

        if (overlapping.type == RowOrCol.ROW && overlapping.dir == -1)
        {
            int[] row = GetRow(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int x = 0; x < 3; x++)
            {
                g.board[x, overlapping.index] = row[x + 1];
            }
            g.board[3, overlapping.index] = row[0];
        }

        if (overlapping.type == RowOrCol.COL && overlapping.dir == 1)
        {
            int[] col = GetCol(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int y = 1; y < 4; y++)
            {
                g.board[overlapping.index, y] = col[y - 1];
            }
            g.board[overlapping.index, 0] = col[3];
        }

        if (overlapping.type == RowOrCol.COL && overlapping.dir == -1)
        {
            int[] col = GetCol(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int y = 0; y < 3; y++)
            {
                g.board[overlapping.index, y] = col[y + 1];
            }
            g.board[overlapping.index, 3] = col[0];
        }
    }

    public static int[] GetRow(int[,] matrix, int rowIndex)
    {
        return Enumerable.Range(0, matrix.GetLength(0)).Select(x => matrix[x, rowIndex]).ToArray();
    }

    public static int[] GetCol(int[,] matrix, int colIndex)
    {
        return Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[colIndex, x]).ToArray();
    }

    public static void Draw(Game g)
    {
        int[,] board = g.board;
        int gap = 32;
        Raylib.BeginDrawing();

        Raylib.ClearBackground(Color.White);

        foreach (var btn in g.moveBtnMap)
        {
            Rectangle r = btn.Key;
            Raylib.DrawRectangle((int)r.X, (int)r.Y, 24, 24, Color.Purple);
        }

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                Vec2i position = GRID_START_POS + new Vec2i(x * gap, y * gap);
                switch (board[x, y])
                {
                    case (int)TileType.EMPTY:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Red);
                        break;
                    case (int)TileType.FILLED:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Blue);
                        break;
                }
            }
        }
        Raylib.DrawRectangle((int)g.mouseHitbox.X - 4, (int)g.mouseHitbox.Y - 4, 8, 8, Color.Green);
        Raylib.EndDrawing();
    }
}
