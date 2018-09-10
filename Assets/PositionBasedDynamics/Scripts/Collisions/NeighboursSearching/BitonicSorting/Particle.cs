using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PositionBasedDynamics.Collisions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Particle:IComparable<Particle> {
        static public int SIZE = sizeof(uint) + 3*sizeof(int); 
        public uint number;
        public Vector3Int cell;

        public int CompareTo(Particle other)
        {

            //return (left.x == right.x) ? (left.y <= right.y) : (left.x <= right.x);
            //return left <= right;

            if (this.cell.x != other.cell.x)
            {
                return this.cell.x - other.cell.x;
            }

            if (this.cell.y != other.cell.y)
            {
                return this.cell.y - other.cell.y;
            }

            return this.cell.z - other.cell.z;

        }
    }
}