using System;
using System.Collections.Generic;

using Common.Geometry.Shapes;
using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Collisions;
using PositionBasedDynamics.Sources;
using UnityEngine;

namespace PositionBasedDynamics.Bodies.Fluids
{

    public class FluidBoundary3d
    {
        public Vector3f[] Positions { get; private set; }
        
        public ComputeBuffer GPUPositions { get; set; }

        public float[] Psi { get; private set; } 

        public ComputeBuffer GPUPsi { get; set; }

        public double ParticleRadius { get; private set; }

        public double ParticleDiameter { get { return ParticleRadius * 2.0; } }

        public double Density { get; private set; }

        public int NumParticles { get; private set;  }

        public FluidBoundary3d(ParticleSource source, double radius, double density, Matrix4x4f RTS)
        {
            ParticleRadius = radius;
            Density = density;

            CreateParticles(source, RTS);
            CreateBoundryPsi();
        }

        private void CreateParticles(ParticleSource source, Matrix4x4f RTS)
        {
            NumParticles = source.NumParticles;

            Positions = new Vector3f[NumParticles];
  
            for (int i = 0; i < NumParticles; i++)
            {
                Vector4f pos = RTS * source.Positions[i].xyz1;
                Positions[i] = new Vector3f(pos.x, pos.y, pos.z);
            }

            GPUPositions = new ComputeBuffer(Positions.Length, sizeof(float) * 3);
            GPUPositions.SetData(Positions);

        }

        private void CreateBoundryPsi()
        {

            Psi = new float[NumParticles];

            double cellSize = ParticleRadius * 4;

            ParticleHash3d hash = new ParticleHash3d(NumParticles, cellSize);
            hash.NeighborhoodSearch(Positions);

            int[,] neighbors = hash.Neighbors;
            int[] numNeighbors = hash.NumNeighbors;

            CubicKernel3d kernel = new CubicKernel3d(cellSize);

            for (int i = 0; i < NumParticles; i++)
            {
                double delta = kernel.W_zero;

                for (int j = 0; j < numNeighbors[i]; j++)
                {
                    int neighborIndex = neighbors[i, j];

                    Vector3f p = Positions[i] - Positions[neighborIndex];

                    delta += kernel.W(p.x, p.y, p.z);
                }

                double volume = 1.0 / delta;

                Psi[i] = (float) (Density * volume);
            }

            GPUPsi = new ComputeBuffer(Psi.Length, sizeof(float));
            GPUPsi.SetData(Psi);

        }

    }

}