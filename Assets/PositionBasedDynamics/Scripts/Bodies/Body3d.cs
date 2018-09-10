using System;
using System.Collections.Generic;

using UnityEngine;

using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;

using PositionBasedDynamics.Constraints;


namespace PositionBasedDynamics.Bodies
{

    public abstract class Body3d
    {
        public int NumParticles { get { return Positions.Length; } }

        public int NumConstraints { get { return Constraints.Count; } }

        public float Dampning { get; set; }

        public float ParticleRadius { get; protected set; }

        public float ParticleDiameter { get { return ParticleRadius * 2.0f; } }

        public float ParticleMass { get; protected set; }

        public Vector3f[] Positions { get; private set; }

        public ComputeBuffer GPUPositions { get; private set; }

        public Vector3f[] Predicted { get; private set; }

        public ComputeBuffer GPUPredicted { get; private set; }

        public Vector3f[] Velocities { get; private set; }

        public ComputeBuffer GPUVelocities { get; private set; }

        public Box3f Bounds { get; private set; }

        public List<Constraint3d> Constraints { get; private set; }

        private List<StaticConstraint3d> StaticConstraints { get; set; }
        
        #region BetweenGPU&CPUDataTransfer

        public void PositionsToGpu(){
            GPUPositions.SetData(Positions);
        }
        public void PredictedToGpu(){
            GPUPredicted.SetData(Predicted);
        }
        public void VelocitiesToGpu(){
            GPUVelocities.SetData(Velocities);
        }

        public void PositionsToCpu(){
            GPUPositions.GetData(Positions);
        }
        public void PredictedToCpu(){
            GPUPredicted.GetData(Predicted);
        }
        public void VelocitiesToCpu(){
            GPUVelocities.GetData(Velocities);
        }
        #endregion

        public Body3d(int numParticles, float radius, float mass)
        {
            Positions = new Vector3f[numParticles];
            GPUPositions = new ComputeBuffer(numParticles, sizeof(float) * 3);
            Predicted = new Vector3f[numParticles];
            GPUPredicted = new ComputeBuffer(numParticles, sizeof(float) * 3);
            Velocities = new Vector3f[numParticles];
            GPUVelocities = new ComputeBuffer(numParticles, sizeof(float) * 3);
            Constraints = new List<Constraint3d>();
            StaticConstraints = new List<StaticConstraint3d>();

            ParticleRadius = radius;
            ParticleMass = mass;
            Dampning = 1;

            if (ParticleMass <= 0)
                throw new ArgumentException("Particles mass <= 0");

            if (ParticleRadius <= 0)
                throw new ArgumentException("Particles radius <= 0");
        }

        internal void ConstrainPositions(double di)
        {
            for (int i = 0; i < Constraints.Count; i++)
            {
                Constraints[i].ConstrainPositions(di);
            }

            for (int i = 0; i < StaticConstraints.Count; i++)
            {
                StaticConstraints[i].ConstrainPositions(di);
            }
        }

        internal void ConstrainVelocities()
        {

            for (int i = 0; i < Constraints.Count; i++)
            {
                Constraints[i].ConstrainVelocities();
            }

            for (int i = 0; i < StaticConstraints.Count; i++)
            {
                StaticConstraints[i].ConstrainVelocities();
            }

        }

        public void RandomizePositions(System.Random rnd, float amount)
        {
            PositionsToCpu();
            for(int i = 0; i < NumParticles; i++)
            {
                float rx = (float)(rnd.NextDouble() * 2.0 - 1.0);
                float ry = (float)(rnd.NextDouble() * 2.0 - 1.0);
                float rz = (float)(rnd.NextDouble() * 2.0 - 1.0);

                Positions[i] += new Vector3f(rx, ry, rz) * amount;
            }
            PositionsToGpu();
        }

        public void RandomizeConstraintOrder(System.Random rnd)
        {
            int count = Constraints.Count;
            if (count <= 1) return;

            List<Constraint3d> tmp = new List<Constraint3d>();

            while (tmp.Count != count)
            {
                int i = rnd.Next(0, Constraints.Count - 1);

                tmp.Add(Constraints[i]);
                Constraints.RemoveAt(i);
            }

            Constraints = tmp;
        }

        public void MarkAsStatic(Box3f bounds)
        {
            for (int i = 0; i < NumParticles; i++)
            {
                if (bounds.Contains(Positions[i]))
                {
                    StaticConstraints.Add(new StaticConstraint3d(this, i));
                }
            }
        }

        public void UpdateBounds()
        {
            Vector3f min = new Vector3f(float.PositiveInfinity);
            Vector3f max = new Vector3f(float.NegativeInfinity);

            for (int i = 0; i < NumParticles; i++)
            {
                min.Min(Positions[i]);
                max.Max(Positions[i]);
            }

            min -= ParticleRadius;
            max += ParticleRadius;

            Bounds = new Box3f(min, max);
        }

    }

}