using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassicBotter
{
    public class Location
    {

        public int X, Y, Z;

        public Location() { }

        public Location(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsAdjacentTo(Location loc, byte range)
        {
            return loc.Z == Z && Math.Max(Math.Abs(X - loc.X), Math.Abs(Y - loc.Y)) <= range;
        }

        public double DistanceTo(Location l)
        {
            int xDist = X - l.X;
            int yDist = Y - l.Y;

            return Math.Sqrt(xDist * xDist + yDist * yDist);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", X, Y, Z);
        }
    }
}
