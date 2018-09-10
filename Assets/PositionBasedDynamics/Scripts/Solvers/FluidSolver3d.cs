using System;
using System.Collections.Generic;

using Common.Geometry.Shapes;
using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Bodies;
using PositionBasedDynamics.Bodies.Fluids;
using UnityEngine;

namespace PositionBasedDynamics.Solvers
{

    public class FluidSolver3d
    {

        private FluidBody3d Body { get; set; }

        private List<ExternalForce3d> Forces { get; set; }

        public FluidSolver3d(FluidBody3d body)
        {
            Body = body;
            Forces = new List<ExternalForce3d>();
        }

        public void AddForce(ExternalForce3d force)
        {
            Forces.Add(force);
        }

        public void StepPhysics(float dt)
        {

            if (dt == 0.0) return;

            AppyExternalForces(dt);

            EstimatePositions(dt);

            //the highest priority to parallel it
            UpdateConstraint();

            UpdateVelocities(dt);

            //the second after the highest priority to parallel it
            Body.ComputeViscosity();

            UpdatePositions();
        }

        private void AppyExternalForces(float dt)
        {
            
            //for (int i = 0; i < Body.NumParticles; i++)
            //{
              //  Body.Velocities[i] -= (Body.Velocities[i] * Body.Dampning) * dt;
            //}

            for (int i = 0; i < Forces.Count; i++)
            {
                Forces[i].ApplyForce(dt,Body);
            }

        }

        private void EstimatePositions(float dt)
        {
            for (int i = 0; i < Body.NumParticles; i++)
            {
                Body.Predicted[i] = Body.Positions[i] + dt * Body.Velocities[i];
            }
        }

        private void UpdateConstraint()
        {
            Body.ConstrainPositions(1);
        }

        private void UpdateVelocities(float dt)
        {
            float inv_dt = 1.0f / dt;

            for (int i = 0; i < Body.NumParticles; i++)
            {
                Vector3f d = Body.Predicted[i] - Body.Positions[i];
                Body.Velocities[i] = d * inv_dt;
            }
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < Body.NumParticles; i++)
            {
                Body.Positions[i] = Body.Predicted[i];
            }
        }


    }

}