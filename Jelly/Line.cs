using System;
using System.Runtime.Serialization;
using Jelly.Utilities;
using Microsoft.Xna.Framework;

namespace Jelly;

[DataContract]
public struct Line : IEquatable<Line>
{
    [DataMember]
    public Point P1 { get; set; }

    [DataMember]
    public Point P2 { get; set; }

    public int Thickness { get; set; }

    public static Line Empty => new();

    public Point this[int index]
    {
        readonly get
        {
            return index switch
            {
                0 => P1,
                1 => P2,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
        set
        {
            switch(index)
            {
                case 0: P1 = value; break;
                case 1: P2 = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    public Line(Point p1, Point p2, int thickness = 1)
    {
        this.P1 = p1;
        this.P2 = p2;
        this.Thickness = thickness;
    }

    public Line(int x1, int y1, int x2, int y2, int thickness = 1)
    {
        this.P1 = new(x1, y1);
        this.P2 = new(x2, y2);
        this.Thickness = thickness;
    }

    public readonly bool Intersects(Rectangle rectangle)
    {
        int minX = MathHelper.Min(P1.X, P2.X);
        int minY = MathHelper.Min(P1.Y, P2.Y);
        int maxX = MathHelper.Max(P1.X, P2.X);
        int maxY = MathHelper.Max(P1.Y, P2.Y);

        if(
            rectangle.X + rectangle.Width <= minX
            || rectangle.Y + rectangle.Height <= minY
            || rectangle.X >= maxX
            || rectangle.Y >= maxY
        ) return false;

        if(rectangle.Contains(P1) || rectangle.Contains(P2)) return true;

        // bottom
        if(this.Intersects(new Line(rectangle.Left, rectangle.Bottom - 1, rectangle.Right - 1, rectangle.Bottom - 1))) return true;

        // top
        if(this.Intersects(new Line(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Top))) return true;

        // left
        if(this.Intersects(new Line(rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom - 1))) return true;

        // right
        if(this.Intersects(new Line(rectangle.Right - 1, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1))) return true;

        // my old solution (does not work properly from below due to false positives)

        // int rx1;
        // int rx2;

        // if(P2.Y > P1.Y)
        // {
        //     rx1 = rectangle.Left - minX;
        //     rx2 = rectangle.Right - minX;
        // }
        // else
        // {
        //     rx1 = rectangle.Left - maxX;
        //     rx2 = rectangle.Right - maxX;
        // }

        // float y1 = (float)(P2.Y - P1.Y) / (P2.X - P1.X) * rx1;
        // float y2 = (float)(P2.Y - P1.Y) / (P2.X - P1.X) * rx2;

        // if(rectangle.Bottom - P1.Y < y1 && rectangle.Bottom - P2.Y > y2) return true;
        // if(rectangle.Bottom - P1.Y > y1 && rectangle.Bottom - P2.Y < y2) return true;

        return false;
    }

    public readonly bool Contains(Point point)
    {
        int val = MathUtil.Sqr(point.X - P1.X) + MathUtil.Sqr(point.Y - P1.Y) + MathUtil.Sqr(point.X - P2.X) + MathUtil.Sqr(point.Y - P2.Y);
        int len = MathUtil.Sqr(P1.X - P2.X) + MathUtil.Sqr(P1.Y - P1.Y);

        return val >= len - MathUtil.Sqr(Thickness / 2f) && val <= len + MathUtil.Sqr(Thickness / 2f);
    }

    // source: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/

    // Given three collinear points p, q, r, the function checks if point q lies on line segment 'pr'
    static bool OnSegment(Point p, Point q, Point r)
    {
        if(q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) && q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
        return true;

        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    static int GetOrientation(Point p, Point q, Point r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);

        if (val == 0) return 0; // collinear

        return (val > 0)? 1: 2; // clock or counterclock wise
    }

    // The main function that returns true if line segment 'p1q1' and 'p2q2' intersect.
    public readonly bool Intersects(Line line)
    {
        // Find the four orientations needed for general and special cases
        int o1 = GetOrientation(P1, P2, line.P1);
        int o2 = GetOrientation(P1, P2, line.P2);
        int o3 = GetOrientation(line.P1, line.P2, P1);
        int o4 = GetOrientation(line.P1, line.P2, P2);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && OnSegment(P1, line.P1, P2)) return true;

        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && OnSegment(P1, line.P2, P2)) return true;

        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && OnSegment(line.P1, P1, line.P2)) return true;

        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && OnSegment(line.P1, P2, line.P2)) return true;

        return false; // Doesn't fall in any of the above cases
    }

    public readonly Line Shift(Point offset)
    {
        return new Line(P1 + offset, P2 + offset, Thickness);
    }

    public readonly Line Shift(int x, int y) => Shift(new(x, y));

    public static bool operator ==(Line a, Line b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Line a, Line b)
    {
        return !a.Equals(b);
    }

    public readonly bool Equals(Line other)
    {
        return ((other.P1 == P1 && other.P2 == P2) || (other.P1 == P2 && other.P2 == P1)) && other.Thickness == Thickness;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is Line triangle && Equals(triangle);
    }
}
