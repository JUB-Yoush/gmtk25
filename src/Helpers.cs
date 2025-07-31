namespace Helper;

using System.Globalization;
using System.Numerics;
using System.Runtime;
using Helper;

public enum Direction
{
    UP = 0,
    DOWN = 1,
    LEFT = 2,
    RIGHT = 3,
}

public struct Vec2i(int x, int y)
{
    public int X = x;
    public int Y = y;

    public static readonly Vec2i ZERO = new(0, 0);

    public static Vec2i operator +(Vec2i v1, Vec2i v2) => new(v1.X + v2.X, v1.Y + v2.Y);

    public static Vec2i operator -(Vec2i v1, Vec2i v2) => new(v1.X - v2.X, v1.Y - v2.Y);

    public static Vec2i operator *(Vec2i v1, Vec2i v2) => new(v1.X * v2.X, v1.Y * v2.Y);

    public static Vec2i operator *(Vec2i v1, int a) => new(v1.X * a, v1.Y * a);

    public static bool operator ==(Vec2i v1, Vec2i v2) => v1.X == v2.X && v1.Y == v2.Y;

    public static bool operator !=(Vec2i v1, Vec2i v2) => v1.X != v2.X || v1.Y != v2.Y;

    public static implicit operator Vec2(Vec2i v) => new Vec2(v.X, v.Y);

    public override string ToString()
    {
        return $"{this.X},{this.Y}";
    }

    public static Vec2i operator +(Vec2i v1, Direction dir)
    {
        return dir switch
        {
            Direction.UP => v1 + new Vec2i(0, -1),
            Direction.DOWN => v1 + new Vec2i(0, 1),
            Direction.LEFT => v1 + new Vec2i(-1, 0),
            Direction.RIGHT => v1 + new Vec2i(1, 0),
            _ => new(0, -1),
        };
    }

    public static Vec2i operator -(Vec2i v1, Direction dir)
    {
        return dir switch
        {
            Direction.UP => v1 - new Vec2i(0, -1),
            Direction.DOWN => v1 - new Vec2i(0, 1),
            Direction.LEFT => v1 - new Vec2i(-1, 0),
            Direction.RIGHT => v1 - new Vec2i(1, 0),
            _ => new(0, -1),
        };
    }

    public readonly Vec2 toVec2()
    {
        return new(X, Y);
    }

    public static Vec2i Clamp(Vec2i v, Vec2i min, Vec2i max)
    {
        int x = Math.Clamp(v.X, min.X, max.X);
        int y = Math.Clamp(v.Y, min.Y, max.Y);
        return new(x, y);
    }

    public static Vector2 Vec2(Vec2i v1) => new(v1.X, v1.Y);
}

public static class JLib
{
    public static readonly Vec2i V2_UP = new(0, -1);
    public static readonly Vec2i V2_DOWN = new(0, 1);
    public static readonly Vec2i V2_LEFT = new(-1, 0);
    public static readonly Vec2i V2_RIGHT = new(1, 0);
    public static readonly Direction[] DIR_ARRAY =
    [
        Direction.UP,
        Direction.DOWN,
        Direction.LEFT,
        Direction.RIGHT,
    ];
    public static readonly Vec2i[] EIGHT_DIR_VEC_ARRAY =
    [
        new(0, -1),
        new(0, 1),
        new(1, 0),
        new(-1, 0),
        new(1, -1),
        new(-1, -1),
        new(1, 1),
        new(-1, 1),
    ];

    public static double Wrap(double x, double min, double max)
    {
        return x - (max - min) * Math.Floor((x - min) / (max - min));
    }

    public static float Wrap(float x, float min, float max)
    {
        return (float)(x - (max - min) * Math.Floor((x - min) / (max - min)));
    }

    public static int Wrap(int x, int min, int max)
    {
        return (int)(x - (max - min) * Math.Floor((double)(x - min) / (max - min)));
    }

    public static Vec2i V2Scale(Vec2i a, Vec2i b)
    {
        return new(a.X * b.X, a.Y * b.Y);
    }

    public static Vec2i V2ScaleF(Vec2i a, int b)
    {
        return new(a.X * b, a.Y * b);
    }

    public static bool Coinflip()
    {
        return new Random().Next(0, 2) == 1;
    }

    public static int Get2dIndex(int[] data, Vec2i position, Vec2i bounds)
    {
        return data[position.Y * bounds.X + position.X];
    }

    public static void Set2dIndex(int[] data, Vec2i position, Vec2i bounds, int new_value)
    {
        data[position.Y * bounds.X + position.X] = new_value;
    }

    public static int Get2dIndex(int[] data, int x, int y, Vec2i bounds)
    {
        return Get2dIndex(data, new(x, y), bounds);
    }

    public static int[] Make1D(int[,] data)
    {
        int[] single = new int[data.Length];
        int i = 0;
        //Buffer.BlockCopy(data, 0, single, 0, data.Length);
        for (int y = 0; y <= data.GetUpperBound(1); y++)
        {
            for (int x = 0; x <= data.GetUpperBound(0); x++)
            {
                single[i] = data[x, y];
                i++;
            }
        }
        return single;
    }

    public static int[,] Make2D(int[] data, Vec2i size)
    {
        int[,] multi = new int[size.X, size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                multi[x, y] = Get2dIndex(data, new(x, y), size);
            }
        }
        return multi;
    }

    public static T[] GetRow<T>(T[,] matrix, int rowIndex)
    {
        return Enumerable.Range(0, matrix.GetLength(0)).Select(x => matrix[x, rowIndex]).ToArray();
    }

    public static T[] GetCol<T>(T[,] matrix, int colIndex)
    {
        return Enumerable.Range(0, matrix.GetLength(1)).Select(x => matrix[colIndex, x]).ToArray();
    }

    public static T Get<T>(T[,] data, Vec2i pos)
    {
        return data[pos.X, pos.Y];
    }

    public static void Set<T>(T[,] data, Vec2i pos, T val)
    {
        data[pos.X, pos.Y] = val;
    }

    public static Vec2i V2Wrap(Vec2i value, Vec2i min, Vec2i max)
    {
        return new(Wrap(value.X, min.X, max.X), Wrap(value.Y, min.Y, max.Y));
    }

    public static void V2Wrap(this Vec2 value, Vec2 min, Vec2 max) =>
        value = new(Wrap(value.X, min.X, max.X), Wrap(value.Y, min.Y, max.Y));

    public static int[] IntersectRanges(int[] r1, int[] r2)
    {
        // return the larger start and smaller end
        return [Math.Max(r1[0], r2[0]), Math.Min(r1[1], r2[1])];
    }

    public static Direction[] DirArray()
    {
        return [Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT];
    }

    public static Vec2i Dir2Vec(Direction dir)
    {
        return dir switch
        {
            Direction.UP => new(0, -1),
            Direction.DOWN => new(0, 1),
            Direction.LEFT => new(-1, 0),
            Direction.RIGHT => new(1, 0),
            _ => new(0, -1),
        };
    }

    public static Direction InvertDir(Direction dir)
    {
        return dir switch
        {
            Direction.UP => Direction.DOWN,
            Direction.DOWN => Direction.UP,
            Direction.LEFT => Direction.RIGHT,
            Direction.RIGHT => Direction.LEFT,
            _ => Direction.UP,
        };
    }

    // public static int[,] Clone(int[,] data)
    // {
    //     Vec2i bounds = new(data.GetLength(0) + 1, data.GetLength(1) + 1);
    //     int[] single = Make1D(data);
    //     return Make2D(single.Clone())
    // }
}
