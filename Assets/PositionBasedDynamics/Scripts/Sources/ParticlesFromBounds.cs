using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;

namespace PositionBasedDynamics.Sources
{

    public class ParticlesFromBounds : ParticleSource
    {

        public Box3f Bounds { get; private set; }

        public ParticlesFromBounds(float spacing, Box3f bounds) : base(spacing)
        {
            Bounds = bounds;
            CreateParticles();
        }

        public ParticlesFromBounds(float spacing, Box3f bounds, Box3f exclusion) : base(spacing)
        {
            Bounds = bounds;
            CreateParticles(exclusion);
        }

        private void CreateParticles()
        {

            int numX = (int)(Bounds.Width / Diameter);
            int numY = (int)(Bounds.Height / Diameter);
            int numZ = (int)(Bounds.Depth / Diameter);

            Positions = new List<Vector3f>(numX * numY * numZ);

            for (int z = 0; z < numZ; z++)
            {
                for (int y = 0; y < numY; y++)
                {
                    for (int x = 0; x < numX; x++)
                    {
                        Vector3f pos = new Vector3f();
                        pos.x = Diameter * x + Bounds.Min.x + Spacing;
                        pos.y = Diameter * y + Bounds.Min.y + Spacing;
                        pos.z = Diameter * z + Bounds.Min.z + Spacing;

                        Positions.Add(pos);
                    }
                }
            }

        }

        private void CreateParticles(Box3f exclusion)
        {

            int numX = (int)(Bounds.Width / Diameter);
            int numY = (int)(Bounds.Height / Diameter);
            int numZ = (int)(Bounds.Depth / Diameter);

            Positions = new List<Vector3f>();

            for (int z = 0; z < numZ; z++)
            {
                for (int y = 0; y < numY; y++)
                {
                    for (int x = 0; x < numX; x++)
                    {
                        Vector3f pos = new Vector3f();
                        pos.x = Diameter * x + Bounds.Min.x + Spacing;
                        pos.y = Diameter * y + Bounds.Min.y + Spacing;
                        pos.z = Diameter * z + Bounds.Min.z + Spacing;

                        if(!exclusion.Contains(pos))
                            Positions.Add(pos);

                    }
                }
            }

        }



    }

}