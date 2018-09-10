using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Constraints;
using PositionBasedDynamics.Bodies;
using PositionBasedDynamics.Collisions;

namespace PositionBasedDynamics.Solvers
{

    public class Solver3d
    {
        public int SolverIterations { get; set; }

        public int CollisionIterations { get; set; }

        public float SleepThreshold { get; set; }

        public List<Body3d> Bodies { get; private set; }

        private List<ExternalForce3d> Forces { get; set; }

        private List<Collision3d> Collisions { get; set; }

        public Solver3d()
        {
            SolverIterations = 4;
            CollisionIterations = 2;

            Forces = new List<ExternalForce3d>();
            Collisions = new List<Collision3d>();
            Bodies = new List<Body3d>();
        }

        public void AddForce(ExternalForce3d force)
        {
            if (Forces.Contains(force)) return;
            Forces.Add(force);
        }

        public void AddCollision(Collision3d collision)
        {
            if (Collisions.Contains(collision)) return;
            Collisions.Add(collision);
        }

        public void AddBody(Body3d body)
        {
            if (Bodies.Contains(body)) return;
            Bodies.Add(body);
        }

        public void StepPhysics(float dt)
        {
            if (dt == 0.0) return;

            AppyExternalForces(dt);

            EstimatePositions(dt);

            UpdateBounds();

            ResolveCollisions();

            ConstrainPositions();

            UpdateVelocities(dt);

            UpdatePositions();

            UpdateBounds();

        }

        private void AppyExternalForces(float dt)
        {

            for (int j = 0; j < Bodies.Count; j++)
            {
                Body3d body = Bodies[j];

                for (int i = 0; i < body.NumParticles; i++)
                {
                    body.Velocities[i] -= (body.Velocities[i] * body.Dampning) * dt;
                }

                for (int i = 0; i < Forces.Count; i++)
                {
                    Forces[i].ApplyForce(dt, body);
                }
            }
        }

        private void EstimatePositions(float dt)
        {
            for (int j = 0; j < Bodies.Count; j++)
            {
                Body3d body = Bodies[j];

                for (int i = 0; i < body.NumParticles; i++)
                {
                    body.Predicted[i] = body.Positions[i] + dt * body.Velocities[i];
                }
            }
        }

        private void UpdateBounds()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                Bodies[i].UpdateBounds();
            }
        }

        private void ResolveCollisions()
        {
            List<CollisionContact3d> contacts = new List<CollisionContact3d>();

            for (int i = 0; i < Collisions.Count; i++)
            {
                Collisions[i].FindContacts(Bodies, contacts);
            }

            float di = 1.0f / CollisionIterations;

            for(int i = 0; i < CollisionIterations; i++)
            {
                for (int j = 0; j < contacts.Count; j++)
                {
                    contacts[j].ResolveContact(di);
                }
            }
        }

        private void ConstrainPositions()
        {
            double di = 1.0 / SolverIterations;

            for (int i = 0; i < SolverIterations; i++)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    Bodies[j].ConstrainPositions(di);
                }
            }
        }

        private void UpdateVelocities(float dt)
        {
            float invDt = 1.0f / dt;
            float threshold2 = SleepThreshold * dt;
            threshold2 *= threshold2;

            for (int j = 0; j < Bodies.Count; j++)
            {
                Body3d body = Bodies[j];

                for (int i = 0; i < body.NumParticles; i++)
                {
                    Vector3f d = body.Predicted[i] - body.Positions[i];
                    body.Velocities[i] = d * invDt;

                    double m = body.Velocities[i].SqrMagnitude;
                    if (m < threshold2)
                        body.Velocities[i] = Vector3f.Zero;
                }
            }
        }

        private void ConstrainVelocities()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                Bodies[i].ConstrainVelocities();
            }
        }

        private void UpdatePositions()
        {
            for (int j = 0; j < Bodies.Count; j++)
            {
                Body3d body = Bodies[j];

                for (int i = 0; i < body.NumParticles; i++)
                {
                    body.Positions[i] = body.Predicted[i];
                }
            }
        }

    }

}