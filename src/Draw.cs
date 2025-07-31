using Raylib_cs;

public static class Draw
{
    public static void DrawFrame(Game g)
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
                Vec2i position = Puzzle.GRID_START_POS + new Vec2i(x * gap, y * gap);
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
                    case (int)TileType.BOX:
                        Raylib.DrawRectangle(position.X, position.Y, 24, 24, Color.Black);
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
}
