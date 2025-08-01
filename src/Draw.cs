using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using Helper;
using Puzzles;
using Raylib_cs;

public static class Draw
{
    public static readonly Dictionary<string, string> texturePaths = new()
    {
        { "puzzleTiles", "./assets/images/tiles.png" },
    };

    // public static readonly Dictionary<TileType, Vec2> uvMap = new Dictionary<TileType, Vec2>()
    // {
    //     { TileType.EMPTY, new(1, 3) },
    //     { TileType.POSITIVE, new(0, 1) },
    //     { TileType.NEGATIVE, new(0, 1) },
    //     { TileType.NODE, new(1, 3) },
    //     { TileType.NODE, new(1, 3) },
    // };

    public static Dictionary<string, Texture2D> textures = [];

    public static Texture2D GetTexture(string id)
    {
        return textures[id];
    }

    public static void LoadTextures()
    {
        foreach (var asset in texturePaths)
        {
            textures.Add(asset.Key, Raylib.LoadTexture(asset.Value));
        }
    }

    public static void DrawFrame(Puzzle g)
    {
        g.route = Puzzle.getCircuitStatus(g.board).visited;
        TileType[,] board = g.board;
        int gap = Puzzle.TILEGAP;
        Raylib.BeginDrawing();

        Raylib.ClearBackground(Color.White);

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

            DrawTile(new(r.X, r.Y), new(4, 0), rot);
        }

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                Vec2i position = Puzzle.GRID_START_POS + new Vec2i(x * gap, y * gap);
                switch (board[x, y])
                {
                    case TileType.EMPTY:
                        DrawTile(position, new(3, 1));
                        break;
                    case TileType.WIRE_UD:
                        DrawTile(position, getUV(x, y, new(0, 1), g.route));
                        break;
                    case TileType.WIRE_LR:
                        DrawTile(position, getUV(x, y, new(0, 1), g.route), 90);
                        break;
                    case TileType.WIRE_LD:
                        DrawTile(position, getUV(x, y, new(1, 1), g.route), 0);
                        break;
                    case TileType.WIRE_LU:
                        DrawTile(position, getUV(x, y, new(1, 1), g.route), 90);
                        break;
                    case TileType.WIRE_RU:
                        DrawTile(position, getUV(x, y, new(1, 1), g.route), 180);
                        break;
                    case TileType.WIRE_RD:
                        DrawTile(position, getUV(x, y, new(1, 1), g.route), 270);
                        break;
                    case TileType.NODE:
                        DrawTile(position, getUV(x, y, new(2, 0), g.route));
                        break;
                    case TileType.POSITIVE:
                        DrawTile(position, new(1, 0));
                        break;
                    case TileType.NEGATIVE:
                        DrawTile(position, new(0, 0));
                        break;
                    case TileType.BOX:
                        DrawTile(position, new(3, 0));
                        break;
                }
            }
        }
        foreach (var pos in g.route)
        {
            Vec2i position = Puzzle.GRID_START_POS + new Vec2i(pos.X * gap, pos.Y * gap);
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

    public static Vec2 getUV(int x, int y, Vec2i uv, List<Vec2i> elecRoute)
    {
        if (elecRoute.Contains(new(x, y)))
        {
            return (uv + Direction.DOWN).toVec2();
        }
        return uv;
        // if (board[x, y] == TileType.NODE)
        // {
        //     if (elecRoute.Contains(new(x, y)))
        //     {
        //         return new(2, 1);
        //     }
        //     else
        //     {
        //         return new(2, 0);
        //     }
        // }
        // if (board[x, y] == TileType.WIRE_LR || board[x, y] == TileType.WIRE_UD)
        // {
        //     if (elecRoute.Contains(new(x, y)))
        //     {
        //         return new(0, 1);
        //     }
        //     else
        //     {
        //         return new(0, 2);
        //     }
        // }
        // if (
        //     (
        //         board[x, y] == TileType.WIRE_LD
        //         || board[x, y] == TileType.WIRE_LU
        //         || board[x, y] == TileType.WIRE_RD
        //         || board[x, y] == TileType.WIRE_RU
        //     ) && elecRoute.Contains(new(x, y))
        // )
        // {
        //     return new(1, 2);
        // }
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
            new(Puzzle.TILESIZE / 2, Puzzle.TILESIZE / 2),
            rotation,
            Color.White
        );
    }
}
