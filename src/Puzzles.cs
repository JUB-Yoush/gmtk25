using Helper;
using Raylib_cs;

class Tile { }

public enum TileType
{
    EMPTY,
    WIRE,
    NODE,
    POSITIVE,
    NEGATIVE,
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

public class Game
{
    public int[,] board = new int[4, 4];
    public Rectangle mouseHitbox = new();
    public Dictionary<Rectangle, MoveBtn> moveBtnMap = new();
    public bool solved = false;
    public List<Vec2i> route = [];

    public Game()
    {
        int[,] board =
        {
            { 4, 1, 3, 1 },
            { 1, 1, 1, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 0, 0 },
        };
        this.board = board;
        this.moveBtnMap = Puzzle.populateBtnMap();
    }
}

public class Puzzle()
{
    public static readonly Vec2i GRID_START_POS = new(100, 100);
    public static readonly Vec2i BTN_START_POS = new(100 - 24, 100 - 24);

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
}
