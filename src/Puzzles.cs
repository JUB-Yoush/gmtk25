using System.Diagnostics;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Helper;
using Raylib_cs;

namespace Puzzles;

class Tile(TileType type)
{
    TileType type = type;

    //public List<Direction> connectors = connectors;
    public bool lit = false;
}

public enum TileType
{
    EMPTY,

    WIRE_UD,
    WIRE_LR,
    WIRE_RD,
    WIRE_RU,
    WIRE_LD,
    WIRE_LU,

    NODE,
    POSITIVE,
    NEGATIVE,
    BOX,
}

public enum RowOrCol
{
    ROW,
    COL,
}

public class MoveBtn(RowOrCol type, int dir, int index)
{
    public RowOrCol type = type;
    public int dir = dir;
    public int index = index;

    public override string ToString()
    {
        return $"{type}|{dir}|{index}";
    }
}

public class Puzzle(TileType[,] board, Vec2i size)
{
    public static readonly Vec2i GRID_START_POS = new(100, 100);
    public static int TILESIZE = 24;
    public static int TILEGAP = 26;
    public static readonly Vec2i BTN_START_POS = new(
        GRID_START_POS.X - TILESIZE,
        GRID_START_POS.Y - TILESIZE
    );

    public Vec2i puzzleSize = size;
    public TileType[,] board = board;
    public Rectangle mouseHitbox = new();
    public Dictionary<Rectangle, MoveBtn> moveBtnMap = populateBtnMap(size);
    public bool solved = false;
    public List<Vec2i> route = [];

    public static Dictionary<Rectangle, MoveBtn> populateBtnMap(Vec2i size)
    {
        Dictionary<Rectangle, MoveBtn> res = new();
        Console.WriteLine(size.ToString());

        int gap = TILEGAP;
        for (int x = 1; x < size.X + 1; x++)
        {
            Vec2i position = BTN_START_POS + new Vec2i(x * gap, 0);
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.COL, -1, x - 1));
        }

        for (int x = 1; x < size.X + 1; x++)
        {
            Vec2i position = BTN_START_POS + new Vec2i(x * gap, (size.Y + 1) * gap);
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.COL, 1, x - 1));
        }

        for (int y = 1; y < size.Y + 1; y++)
        {
            Vec2i position = BTN_START_POS + new Vec2i(0, y * gap);
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.ROW, -1, y - 1));
        }

        for (int y = 1; y < size.Y + 1; y++)
        {
            Vec2i position = BTN_START_POS + new Vec2i((size.X + 1) * gap, y * gap);
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.ROW, 1, y - 1));
        }

        return res;
    }

    public static void Update(Puzzle g)
    {
        Vec2 mousePos = Raylib.GetMousePosition();
        g.mouseHitbox.X = mousePos.X + TILESIZE / 2;
        g.mouseHitbox.Y = mousePos.Y + TILESIZE / 2;

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
        g.solved = getCircuitStatus(g.board).circuit;
        g.route = getCircuitStatus(g.board).visited;
        if (overlapping == null)
        {
            return;
        }
        Vec2i size = g.puzzleSize;

        // get relevant row or column
        // shift all values around (wrap first and last)
        if (overlapping.type == RowOrCol.ROW && overlapping.dir == 1)
        {
            if (g.board[size.X - 1, overlapping.index] == TileType.BOX)
            {
                return;
            }
            //shift right
            TileType[] row = JLib.GetRow(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int x = 1; x < size.X; x++)
            {
                g.board[x, overlapping.index] = row[x - 1];
            }
            g.board[0, overlapping.index] = row[size.X - 1];
        }

        if (overlapping.type == RowOrCol.ROW && overlapping.dir == -1)
        {
            if (g.board[0, overlapping.index] == TileType.BOX)
            {
                return;
            }
            TileType[] row = JLib.GetRow(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int x = 0; x < size.X - 1; x++)
            {
                g.board[x, overlapping.index] = row[x + 1];
            }
            g.board[size.X - 1, overlapping.index] = row[0];
        }

        if (overlapping.type == RowOrCol.COL && overlapping.dir == 1)
        {
            if (g.board[overlapping.index, size.Y - 1] == TileType.BOX)
            {
                return;
            }
            TileType[] col = JLib.GetCol(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int y = 1; y < size.Y; y++)
            {
                g.board[overlapping.index, y] = col[y - 1];
            }
            g.board[overlapping.index, 0] = col[size.Y - 1];
        }

        if (overlapping.type == RowOrCol.COL && overlapping.dir == -1)
        {
            if (g.board[overlapping.index, 0] == TileType.BOX)
            {
                return;
            }
            TileType[] col = JLib.GetCol(g.board, overlapping.index);
            //int last = g.board[3, overlapping.index];
            for (int y = 0; y < size.Y - 1; y++)
            {
                g.board[overlapping.index, y] = col[y + 1];
            }
            g.board[overlapping.index, size.Y - 1] = col[0];
        }
    }

    public static (bool circuit, int nodeCount, List<Vec2i> visited) getCircuitStatus(
        TileType[,] board
    )
    {
        Vec2i positiveTilePos = GetTile(TileType.POSITIVE, board);
        (bool circuit, int nodeCount, List<Vec2i> visited) dfs(
            TileType[,] board,
            Vec2i curr,
            int nodeCount,
            List<Vec2i> visited
        )
        {
            Vec2i size = new(board.GetLength(0), board.GetLength(1));
            Vec2i maxIndex = new(size.X - 1, size.Y - 1);
            if (Raylib.IsKeyPressed(KeyboardKey.D))
            {
                Console.WriteLine("debug catcher");
            }
            foreach (Direction dir in getConnections(board[curr.X, curr.Y]))
            {
                //Console.WriteLine(curr.X.ToString(), curr.Y.ToString());
                Vec2i newDir = curr + dir;
                if (
                    Vec2i.Clamp(newDir, new(0, 0), maxIndex) != newDir
                    || board[newDir.X, newDir.Y] == (int)TileType.EMPTY
                    || visited.Contains(newDir)
                )
                {
                    continue;
                }
                if (board[newDir.X, newDir.Y] == TileType.NEGATIVE)
                {
                    if (visited.Count == 1)
                    {
                        continue;
                    }
                    visited.Remove(positiveTilePos);
                }
                if (board[newDir.X, newDir.Y] == TileType.POSITIVE)
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

    public static Vec2i GetTile(TileType tileType, TileType[,] board)
    {
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == tileType)
                {
                    return new(x, y);
                }
            }
        }
        return new(0, 0); //every board should have one, should never reach
    }

    public static Direction[] getConnections(TileType type)
    {
        return type switch
        {
            TileType.EMPTY => [],
            TileType.BOX => [],
            TileType.NODE => JLib.DIR_ARRAY,
            TileType.POSITIVE => JLib.DIR_ARRAY,
            TileType.NEGATIVE => JLib.DIR_ARRAY,
            TileType.WIRE_LR => [Direction.LEFT, Direction.RIGHT],
            TileType.WIRE_UD => [Direction.UP, Direction.DOWN],
            TileType.WIRE_LD => [Direction.LEFT, Direction.DOWN],
            TileType.WIRE_LU => [Direction.LEFT, Direction.UP],
            TileType.WIRE_RD => [Direction.RIGHT, Direction.DOWN],
            TileType.WIRE_RU => [Direction.RIGHT, Direction.UP],
        };
    }
}

public static class PuzzleLoader
{
    public static Puzzle LoadPuzzle()
    {
        int[,] puzzledata =
        {
            { 8, 9, 10, 0, 0 },
            { 4, 6, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
        };

        Puzzle p = new(
            ParsePuzzleData(puzzledata),
            new(puzzledata.GetLength(0), puzzledata.GetLength(1))
        );
        return p;
    }

    public static TileType[,] ParsePuzzleData(int[,] board)
    {
        TileType[,] res = new TileType[board.GetLength(0), board.GetLength(1)];

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                res[x, y] = (TileType)board[x, y];
            }
        }
        return res;
    }
}
