using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;

namespace PositionBasedDynamics.Sources
{

    public class FluidParticlesWithConstraint : ParticleSource
    {

        public Box3f Bounds { get; private set; }

        private int boundaryCount;

        public FluidParticlesWithConstraint(float spacing, Box3f bounds, int boundaryCount) : base(spacing)
        {
            Bounds = bounds;
            this.boundaryCount = boundaryCount;
            CreateParticles();
        }

        private void CreateParticles()
        {
            int numX = (int)(Bounds.Width / Diameter);
            int numY = (int)(Bounds.Height / Diameter);
            int numZ = (int)(Bounds.Depth / Diameter);

            bool CanFitConstraint = false;

            int needFluidCount = TryFitConstraint(numX*numY*numZ,boundaryCount,out CanFitConstraint);

            if(!CanFitConstraint){
                throw new ArgumentException(String.Format("Can't fit to the power of 2 constraint with the given parameters(Fluid:{0},Boundary:{1},NeedFluidCount:{2})",
                                                            numX*numY*numZ,
                                                            boundaryCount,
                                                            needFluidCount));
            }

            Positions = new List<Vector3f>(needFluidCount);

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

                        if(Positions.Count == needFluidCount){
                            return;
                        }
                    }
                }
            }

        }

        private int TryFitConstraint(int currentFluidCount, int boundaryCount, out bool Can){
            
            int closestPowerOf2 = 
                                    (int)Math.Round(
                                        Math.Log((double)(boundaryCount + currentFluidCount), (double)2));

            int needToFitConstraint = 1 << closestPowerOf2;

            if(currentFluidCount >= needToFitConstraint - boundaryCount){
                Can = true;
            }
            else
            {
                Can = false;
            }

            return needToFitConstraint - boundaryCount;
        }



    }

}