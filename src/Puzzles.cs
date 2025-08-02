using System.ComponentModel;
using System.Diagnostics;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Helper;
using Raylib_cs;

namespace Puzzles;

public enum TileType
{
    EMPTY,

    WIRE_UD = 1,
    WIRE_LR = 2,
    WIRE_RD = 3,
    WIRE_RU = 4,
    WIRE_LD = 5,
    WIRE_LU = 6,

    NODE = 7,
    POSITIVE = 8,
    NEGATIVE = 9,
    BOX = 10,
    DISABLED,
    ROCK,
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
    public static Vec2i GRID_START_POS = new(145, 130);
    public static int TILESIZE = 47;
    public static int TILEGAP = 49;
    public static int BUTTONGAP = 49;
    public static int BUTTONGAPREAL = 20;
    public static Vec2i BTN_START_POS = new(
        GRID_START_POS.X - BUTTONGAP,
        GRID_START_POS.Y - BUTTONGAP
    );

    public static Rectangle CurrentFaceRect = new(0, 0, Draw.FACE_SIZE.X, Draw.FACE_SIZE.Y);
    public static float faceTimer = 5;

    public static void ChangeFace()
    {
        Random rng = new();
        CurrentFaceRect.X = rng.Next(0, 2) * Draw.FACE_SIZE.X;
        CurrentFaceRect.Y = rng.Next(0, 5) * Draw.FACE_SIZE.Y;
        faceTimer = 5;
    }

    public Stack<MoveBtn> undoStack = [];

    public Vec2i puzzleSize = size;
    public TileType[,] board = board;
    public Rectangle mouseHitbox = new();
    public Dictionary<Rectangle, MoveBtn> moveBtnMap = populateBtnMap(size);
    public bool solved = false;
    public List<Vec2i> route = [];
    public bool playedSFX = false;
    public int ComponentCount = GetComponentCount(board);

    public static Dictionary<Rectangle, MoveBtn> populateBtnMap(Vec2i size)
    {
        Dictionary<Rectangle, MoveBtn> res = new();

        BTN_START_POS = new(GRID_START_POS.X - BUTTONGAP, GRID_START_POS.Y - BUTTONGAP);

        int gap = BUTTONGAP;
        for (int x = 1; x < size.X + 1; x++)
        {
            Vec2i position = BTN_START_POS + new Vec2i(x * gap, 0);
            position.Y -= BUTTONGAPREAL;
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.COL, -1, x - 1));
        }

        for (int x = 1; x < size.X + 1; x++)
        {
            Vec2i position = BTN_START_POS + new Vec2i(x * gap, (size.Y + 1) * gap);
            position.Y += BUTTONGAPREAL;
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.COL, 1, x - 1));
        }

        for (int y = 1; y < size.Y + 1; y++)
        {
            Vec2i position = BTN_START_POS + new Vec2i(0, y * gap);
            position.X -= BUTTONGAPREAL;
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.ROW, -1, y - 1));
        }

        for (int y = 1; y < size.Y + 1; y++)
        {
            Vec2i position = BTN_START_POS + new Vec2i((size.X + 1) * gap, y * gap);
            position.X += BUTTONGAPREAL;
            res.Add(new(position.toVec2(), TILESIZE, TILESIZE), new(RowOrCol.ROW, 1, y - 1));
        }

        return res;
    }

    public static void Update(Puzzle g)
    {
        //PrintRoute(g.route);

        // Console.WriteLine(
        //     $"{getCircuitStatus(g.board).circuit} {g.route.Count}, {g.ComponentCount}"
        // );

        g.solved = getCircuitStatus(g.board).circuit && g.route.Count == g.ComponentCount;

        if (g.solved)
        {
            if (!g.playedSFX)
            {
                g.playedSFX = true;
                AudioManager.playSFX("solve");
            }
        }
        else
        {
            g.playedSFX = false;
        }

        Vec2 mousePos = Raylib.GetMousePosition();
        g.mouseHitbox.X = mousePos.X + TILESIZE / 2;
        g.mouseHitbox.Y = mousePos.Y + TILESIZE / 2;

        Rectangle mouseUiHitbox = new(mousePos.X, mousePos.Y, 8, 8);

        g.route = getCircuitStatus(g.board).visited;

        MoveBtn? overlapping = null;

        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            foreach (var btn in g.moveBtnMap)
            {
                Rectangle r = btn.Key;
                if (Raylib.CheckCollisionRecs(r, g.mouseHitbox))
                {
                    overlapping = btn.Value;
                }
            }
        }
        //who would ever write something this afwul

        GlobalGameState.resetHover = Raylib.CheckCollisionRecs(Draw.ResetHitbox, mouseUiHitbox);
        GlobalGameState.undoHover = Raylib.CheckCollisionRecs(Draw.UndoHitbox, mouseUiHitbox);
        GlobalGameState.solveHover = Raylib.CheckCollisionRecs(Draw.SolveHitbox, mouseUiHitbox);

        GlobalGameState.resetPressed =
            Raylib.CheckCollisionRecs(Draw.ResetHitbox, mouseUiHitbox)
            && Raylib.IsMouseButtonPressed(MouseButton.Left);

        GlobalGameState.undoPressed =
            Raylib.CheckCollisionRecs(Draw.UndoHitbox, mouseUiHitbox)
            && Raylib.IsMouseButtonPressed(MouseButton.Left);

        GlobalGameState.solvePressed =
            Raylib.CheckCollisionRecs(Draw.SolveHitbox, mouseUiHitbox)
            && Raylib.IsMouseButtonPressed(MouseButton.Left);

        GlobalGameState.changingPuzzle = GlobalGameState.solvePressed && g.solved;

        GlobalGameState.reseting = GlobalGameState.resetPressed;

        if (GlobalGameState.undoPressed)
        {
            UndoSlide(g);
        }

        if (overlapping == null)
        {
            return;
        }

        Slide(g, overlapping);

        //g.solved = getCircuitStatus(g.board).circuit && g.route.Count == g.ComponentCount;
    }

    public static void UndoSlide(Puzzle g)
    {
        if (g.undoStack.Count == 0)
        {
            return;
        }
        MoveBtn lastMove = g.undoStack.Pop();
        MoveBtn oppositeBtn = new(lastMove.type, lastMove.dir * -1, lastMove.index);
        Slide(g, oppositeBtn, true);
    }

    public static void Slide(Puzzle g, MoveBtn overlapping, bool undoing = false)
    {
        bool nomoved = false;

        Vec2i size = g.puzzleSize;

        if (overlapping.type == RowOrCol.ROW && overlapping.dir == 1)
        {
            if (g.board[size.X - 1, overlapping.index] == TileType.BOX)
            {
                nomoved = true;
                AudioManager.playSFX("nomove");
                return;
            }
            //shift right
            TileType[] row = JLib.GetRow(g.board, overlapping.index);
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
                nomoved = true;
                AudioManager.playSFX("nomove");
                return;
            }
            TileType[] row = JLib.GetRow(g.board, overlapping.index);
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
                nomoved = true;
                AudioManager.playSFX("nomove");
                return;
            }
            TileType[] col = JLib.GetCol(g.board, overlapping.index);
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
                nomoved = true;
                AudioManager.playSFX("nomove");
                return;
            }
            TileType[] col = JLib.GetCol(g.board, overlapping.index);
            for (int y = 0; y < size.Y - 1; y++)
            {
                g.board[overlapping.index, y] = col[y + 1];
            }
            g.board[overlapping.index, size.Y - 1] = col[0];
        }

        if (!undoing)
        {
            g.undoStack.Push(overlapping);
            AudioManager.playSFX("shift");
        }
        else
        {
            if (nomoved)
            {
                AudioManager.playSFX("nomove");
            }
            else
            {
                AudioManager.playSFX("click");
            }
        }
    }

    public static int CanConnect(Vec2i t1Pos, Vec2i t2Pos, TileType[,] board, Direction checkDir)
    {
        TileType t1 = board[t1Pos.X, t1Pos.Y];
        TileType t2 = board[t2Pos.X, t2Pos.Y];
        bool t1Good = getConnections(t1).Contains(checkDir);
        bool t2Good = getConnections(t2).Contains(JLib.InvertDir(checkDir));
        if (t1Good && t2Good)
        {
            return 1;
        }

        return 0;
    }

    public static (bool circuit, int nodeCount, List<Vec2i> visited) getCircuitStatus(
        TileType[,] board
    )
    {
        List<Vec2i> longestRoute = [];
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
            if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                Console.WriteLine("debug catcher");
            }
            foreach (Direction dir in getConnections(board[curr.X, curr.Y]))
            {
                Vec2i newDir = curr + dir;
                bool wrapped = newDir == JLib.V2Wrap(newDir, new(0, 0), size);

                //wrap around
                newDir = JLib.V2Wrap(newDir, new(0, 0), size);

                if (
                    board[newDir.X, newDir.Y] == TileType.EMPTY
                    || board[newDir.X, newDir.Y] == TileType.BOX
                    || board[newDir.X, newDir.Y] == TileType.ROCK
                    || CanConnect(curr, newDir, board, dir) == 0
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
                    longestRoute =
                        longestRoute.Count < visited.Count ? JLib.CloneList(visited) : longestRoute;
                    return (true, nodeCount, longestRoute);
                }
                visited.Add(newDir);
                return dfs(board, newDir, nodeCount, JLib.CloneList(visited));
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

    public static int GetComponentCount(TileType[,] board)
    {
        int res = 0;
        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] != TileType.EMPTY && board[x, y] != TileType.BOX)
                {
                    res += 1;
                }
            }
        }
        return res;
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

    public static void PrintRoute(List<Vec2i> route)
    {
        foreach (var item in route)
        {
            Console.Write(item.ToString());
            Console.Write("|");
        }
        Console.WriteLine("---");
    }
}

public static class PuzzleLoader
{
    public static int puzzleIndex = 0;

    public static Puzzle LoadPuzzle()
    {
        int[,] p0 =
        {
            { 8, 0, 9, 0, 0, 0 },
            { 5, 6, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
        };

        int[,] p1 =
        {
            { 8, 0, 9, 0, 0, 0 },
            { 5, 6, 1, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
        };

        int[,] p2 =
        {
            { 0, 3, 5, 5, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 8, 0, 0, 1, 0 },
            { 0, 0, 4, 0, 0, 0 },
            { 0, 2, 0, 0, 0, 0 },
            { 0, 0, 0, 9, 0, 0 },
        };

        int[,] p3 =
        {
            { 0, 0, 2, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 6 },
            { 0, 3, 4, 0, 0, 0 },
            { 0, 0, 4, 0, 0, 0 },
            { 8, 0, 0, 0, 9, 0 },
            { 0, 0, 5, 0, 0, 0 },
        };

        int[,] p4 =
        {
            { 0, 0, 10, 10, 0, 0 },
            { 5, 0, 10, 10, 8, 0 },
            { 0, 0, 10, 10, 9, 0 },
            { 0, 0, 10, 10, 0, 0 },
            { 0, 6, 10, 10, 0, 0 },
            { 0, 0, 10, 10, 0, 0 },
        };

        int[,] p5 =
        {
            { 0, 0, 10, 10, 0, 0 },
            { 5, 0, 10, 10, 0, 0 },
            { 0, 10, 8, 0, 10, 0 },
            { 0, 10, 0, 9, 10, 0 },
            { 0, 6, 10, 10, 0, 0 },
            { 0, 0, 10, 10, 0, 0 },
        };

        int[,] p6 =
        {
            { 10, 10, 0, 0, 10, 10 },
            { 5, 0, 0, 2, 8, 0 },
            { 0, 0, 0, 0, 9, 0 },
            { 0, 2, 0, 0, 0, 0 },
            { 0, 6, 0, 0, 0, 0 },
            { 10, 0, 0, 10, 10, 10 },
        };
        int[,] p7 =
        {
            { 5, 0, 0, 4, 0, 0 },
            { 0, 0, 2, 0, 1, 0 },
            { 0, 2, 10, 10, 2, 0 },
            { 0, 8, 10, 10, 9, 0 },
            { 1, 0, 0, 1, 0, 1 },
            { 0, 2, 0, 0, 0, 0 },
        };
        int[,] p7b =
        {
            { 10, 0, 2, 0, 0, 10 },
            { 0, 10, 2, 6, 10, 0 },
            { 0, 0, 0, 5, 2, 0 },
            { 0, 8, 0, 0, 9, 0 },
            { 0, 10, 0, 2, 10, 0 },
            { 10, 0, 0, 0, 0, 10 },
        };

        int[,] p8 =
        {
            { 5, 0, 0, 4, 0, 0 },
            { 0, 0, 2, 0, 1, 0 },
            { 10, 2, 10, 10, 2, 10 },
            { 10, 8, 10, 10, 9, 10 },
            { 1, 0, 0, 1, 0, 1 },
            { 0, 2, 0, 0, 0, 0 },
        };
        int[,] p9 =
        {
            { 8, 2, 2, 2, 2, 5 },
            { 2, 2, 2, 2, 5, 4 },
            { 2, 2, 2, 5, 4, 2 },
            { 2, 2, 5, 4, 2, 2 },
            { 2, 5, 4, 2, 2, 2 },
            { 0, 4, 2, 2, 2, 9 },
        };

        int[,] p10 =
        {
            { 8, 2, 2, 2, 2, 5 },
            { 9, 2, 2, 2, 5, 1 },
            { 5, 0, 0, 0, 1, 4 },
            { 1, 3, 2, 2, 6, 0 },
            { 1, 4, 2, 2, 2, 5 },
            { 4, 2, 2, 2, 2, 6 },
        };

        int[,] p11sol =
        {
            { 8, 2, 2, 2, 2, 5 },
            { 9, 2, 2, 2, 5, 1 },
            { 5, 0, 0, 0, 1, 4 },
            { 4, 2, 2, 2, 6, 0 },
            { 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0 },
        };

        int[,] p11 =
        {
            { 0, 0, 0, 0, 0, 0 },
            { 2, 2, 2, 2, 2, 5 },
            { 1, 5, 1, 9, 2, 0 },
            { 0, 8, 4, 4, 5, 0 },
            { 2, 2, 2, 6, 2, 0 },
            { 0, 0, 0, 0, 0, 0 },
        };

        int[][,] puzzleList = [p0, p1, p2, p3, p4, p5, p6, p7b, p7, p8, p11];

        if (puzzleIndex >= puzzleList.Length)
        {
            GlobalGameState.currentState = GameStates.OUTRO;
            return null;
        }

        int[,] currentPuzzle = puzzleList[puzzleIndex];
        Puzzle p = new(
            ParsePuzzleData(currentPuzzle),
            new(currentPuzzle.GetLength(0), currentPuzzle.GetLength(1))
        );
        return p;
    }

    public static TileType[,] ParsePuzzleData(int[,] board)
    {
        TileType[,] res = new TileType[board.GetLength(1), board.GetLength(0)];

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                res[x, y] = (TileType)board[y, x];
            }
        }
        return res;
    }
}
