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

class Tile { }

enum TileType
{
    EMPTY,
    WIRE,
    NODE,
    POSITIVE,
    NEGATIVE,
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
    public bool solved = false;
    public List<Vec2i> route = [];
}

class Program
{
    public static readonly Vec2i GRID_START_POS = new(100, 100);
    public static readonly Vec2i BTN_START_POS = new(100 - 24, 100 - 24);

    public static void Main()
    {
        Raylib.InitWindow(800, 480, "Hello World");
        Raylib.SetTargetFPS(60);
        Game g = new();
        int[,] board =
        {
            { 4, 3, 1, 0 },
            { 1, 0, 1, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 1, 0 },
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
                    //Console.WriteLine(btn.Value.ToString());
                }
            }
        }
        g.solved = isConnected(g.board).circuit;
        g.route = isConnected(g.board).visited;
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
            int[] row = JLib.GetRow(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int x = 1; x < 4; x++)
            {
                g.board[x, overlapping.index] = row[x - 1];
            }
            g.board[0, overlapping.index] = row[3];
        }

        if (overlapping.type == RowOrCol.ROW && overlapping.dir == -1)
        {
            int[] row = JLib.GetRow(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int x = 0; x < 3; x++)
            {
                g.board[x, overlapping.index] = row[x + 1];
            }
            g.board[3, overlapping.index] = row[0];
        }

        if (overlapping.type == RowOrCol.COL && overlapping.dir == 1)
        {
            int[] col = JLib.GetCol(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int y = 1; y < 4; y++)
            {
                g.board[overlapping.index, y] = col[y - 1];
            }
            g.board[overlapping.index, 0] = col[3];
        }

        if (overlapping.type == RowOrCol.COL && overlapping.dir == -1)
        {
            int[] col = JLib.GetCol(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int y = 0; y < 3; y++)
            {
                g.board[overlapping.index, y] = col[y + 1];
            }
            g.board[overlapping.index, 3] = col[0];
        }
    }

    public static (bool circuit, int nodeCount, List<Vec2i> visited) isConnected(int[,] board)
    {
        Vec2i positiveTilePos = GetTile(TileType.POSITIVE, board);
        (bool circuit, int nodeCount, List<Vec2i> visited) dfs(
            int[,] board,
            Vec2i curr,
            int nodeCount,
            List<Vec2i> visited
        )
        {
            if (Raylib.IsKeyPressed(KeyboardKey.D))
            {
                Console.WriteLine("debug catcher");
            }
            foreach (Direction dir in JLib.DIR_ARRAY)
            {
                //Console.WriteLine(curr.X.ToString(), curr.Y.ToString());
                Vec2i newDir = curr + dir;
                if (
                    Vec2i.Clamp(newDir, new(0, 0), new(3, 3)) != newDir
                    || board[newDir.X, newDir.Y] == (int)TileType.EMPTY
                    || visited.Contains(newDir)
                )
                {
                    continue;
                }
                if (board[newDir.X, newDir.Y] == (int)TileType.NEGATIVE)
                {
                    if (visited.Count == 1)
                    {
                        continue;
                    }
                    visited.Remove(positiveTilePos);
                }
                if (board[newDir.X, newDir.Y] == (int)TileType.POSITIVE)
                {
                    visited.Add(newDir);
                    return (true, nodeCount, visited);
                }
                visited.Add(newDir);
                return dfs(board, newDir, nodeCount, visited);
            }
            return (false, 0, visited);
        }
        return dfs(board, positiveTilePos, 0, [positiveTilePos]);
    }

    public static Vec2i GetTile(TileType tileType, int[,] board)
    {
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == (int)tileType)
                {
                    return new(x, y);
                }
            }
        }
        return new(0, 0); //every board should have one, should never reach
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
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Gray);
                        break;
                    case (int)TileType.WIRE:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Green);
                        break;
                    case (int)TileType.NODE:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Pink);
                        break;
                    case (int)TileType.POSITIVE:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Red);
                        break;
                    case (int)TileType.NEGATIVE:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Blue);
                        break;
                }
            }
        }
        foreach (var pos in g.route)
        {
            Vec2i position = GRID_START_POS + new Vec2i(pos.X * gap, pos.Y * gap);
            Raylib.DrawRectangle(position.X, position.Y, 8, 8, Color.Pink);
        }

        Raylib.DrawRectangle((int)g.mouseHitbox.X - 4, (int)g.mouseHitbox.Y - 4, 8, 8, Color.Green);
        if (g.solved)
        {
            Raylib.DrawText("solved", 10, 10, 20, Color.Black);
            Raylib.DrawText($"{g.route.Count}", 10, 30, 20, Color.Black);
        }
        Raylib.EndDrawing();
    }
}
