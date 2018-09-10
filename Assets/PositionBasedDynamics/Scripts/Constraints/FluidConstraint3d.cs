﻿using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies.Fluids;

namespace PositionBasedDynamics.Constraints
{
    public class FluidConstraint3d : Constraint3d
    {

        private FluidBoundary3d Boundary { get; set; }

        private int Iterations { get; set; }

        internal FluidConstraint3d(FluidBody3d body, FluidBoundary3d boundary) : base(body)
        {
            Iterations = 5;
            Boundary = boundary;
        }

        internal override void ConstrainPositions(double di)
        {
            /*
            FluidBody3d fluid = Body as FluidBody3d;
            if (fluid == null) return;

            //fluid.Hash.NeighborhoodSearch(fluid.Predicted, Boundary.Positions);

            //int[,] neighbors = fluid.Hash.Neighbors;
            //int[] numNeighbors = fluid.Hash.NumNeighbors;

            int iter = 0;
            while (iter < Iterations)
            {
                //Calculate lambda.
                for (int i = 0; i < fluid.NumParticles; i++)
                {
                    Vector3f pi = fluid.Predicted[i];
                    //Calculate density constraint. 
                    ComputePBFDensity(fluid, pi, i, numNeighbors[i], neighbors);
                    ComputePBFLagrangeMultiplier(fluid, pi, i, numNeighbors[i], neighbors);
                }

                //Update position.
                for (int i = 0; i < fluid.NumParticles; i++)
                {
                    Vector3f pi = fluid.Predicted[i];
                    fluid.Predicted[i] += SolveDensityConstraint(fluid, pi, i, numNeighbors[i], neighbors);
                }

                iter++;
            }

            fluid.Hash.IncrementTimeStamp(); //TODO - needs to move
            */
        }
/*
        private double ComputePBFDensity(FluidBody3d fluid, Vector3f pi, int i, int numNeighbors, int[,] neighbors)
        {

            //Density for Pi
            // Di = SUMj Mj * W(Pi - Pj, h)

            // Compute current density for particle i
            fluid.Densities[i] = fluid.ParticleMass * (float)fluid.Kernel.W_zero;

            for (int j = 0; j < numNeighbors; j++)
            {
                int neighborIndex = neighbors[i, j];
                if (neighborIndex < fluid.NumParticles) // Test if fluid particle
                {
                    Vector3f pn = fluid.Predicted[neighborIndex];
                    fluid.Densities[i] += fluid.ParticleMass * (float)fluid.Kernel.W(pi.x - pn.x, pi.y - pn.y, pi.z - pn.z);
                }
                else
                {
                    int k = neighborIndex - fluid.NumParticles;

                    Vector3f pn = Boundary.Positions[k];
                    fluid.Densities[i] += Boundary.Psi[k] * (float)fluid.Kernel.W(pi.x - pn.x, pi.y - pn.y, pi.z - pn.z);
                }
            }

            double maxDensity = fluid.Densities[i];
            if (fluid.Density > maxDensity) maxDensity = fluid.Density;

            return maxDensity - fluid.Density;
        }

        private void ComputePBFLagrangeMultiplier(FluidBody3d fluid, Vector3f pi, int i, int numNeighbors, int[,] neighbors)
        {

            float eps = 1.0e-6f;
            float InvDensity = 1.0f / fluid.Density;
            float MassMulInvDensity = fluid.ParticleMass * InvDensity;

            // Evaluate constraint function. Clamp to prevent particle clumping at surface.
            //Ci = Di / D0 - 1
            float C = fluid.Densities[i] * InvDensity - 1.0f;
            if (C < 0.0) C = 0.0f;

            if (C != 0.0)
            {
                //Compute gradients.

                //Constraint gradient for Pi
                //dPkCi = 1/D0 * SUMj dPk * W(Pi - Pj, h)

                float sum_grad_C2 = 0.0f;
                Vector3f gradC_i = Vector3f.Zero;

                for (int j = 0; j < numNeighbors; j++)
                {
                    int neighborIndex = neighbors[i, j];
                    if (neighborIndex < fluid.NumParticles) // Test if fluid particle
                    {
                        Vector3f pn = fluid.Predicted[neighborIndex];
                        Vector3d gradW = fluid.Kernel.GradW(pi.x - pn.x, pi.y - pn.y, pi.z - pn.z);

                        Vector3f gradC_j;
                        gradC_j.x = -MassMulInvDensity * (float)gradW.x;
                        gradC_j.y = -MassMulInvDensity * (float)gradW.y;
                        gradC_j.z = -MassMulInvDensity * (float)gradW.z;

                        sum_grad_C2 += gradC_j.x * gradC_j.x + gradC_j.y * gradC_j.y + gradC_j.z * gradC_j.z;

                        gradC_i.x -= gradC_j.x;
                        gradC_i.y -= gradC_j.y;
                        gradC_i.z -= gradC_j.z;
                    }
                    else
                    {
                        int k = neighborIndex - fluid.NumParticles;

                        Vector3f pn = Boundary.Positions[k];
                        Vector3d gradW = fluid.Kernel.GradW(pi.x - pn.x, pi.y - pn.y, pi.z - pn.z);

                        float psi = -Boundary.Psi[k] * InvDensity;

                        Vector3f gradC_j;
                        gradC_j.x = psi * (float)gradW.x;
                        gradC_j.y = psi * (float)gradW.y;
                        gradC_j.z = psi * (float)gradW.z;

                        sum_grad_C2 += gradC_j.x * gradC_j.x + gradC_j.y * gradC_j.y + gradC_j.z * gradC_j.z;

                        gradC_i.x -= gradC_j.x;
                        gradC_i.y -= gradC_j.y;
                        gradC_i.z -= gradC_j.z;
                    }
                }

                sum_grad_C2 += gradC_i.SqrMagnitude;

                //Lambda for Pi
                //Li = -Ci / SUM | dPk Ci |^ 2 + e

                // Compute lambda
                fluid.Lambda[i] = -C / (sum_grad_C2 + eps);
            }
            else
            {
                fluid.Lambda[i] = 0.0f;
            }

        }

        private Vector3f SolveDensityConstraint(FluidBody3d fluid, Vector3f pi, int i, int numNeighbors, int[,] neighbors)
        {

            //Total position update for Pi
            // dPi = 1 / D0 * SUMj (Li + Lj) * dW(Pi - Pj, h)

            Vector3f corr = Vector3f.Zero;
            float InvDensity = 1.0f / fluid.Density;
            float MassMulInvDensity = fluid.ParticleMass * InvDensity;

            for (int j = 0; j < numNeighbors; j++)
            {
                int neighborIndex = neighbors[i, j];
                if (neighborIndex < fluid.NumParticles) // Test if fluid particle
                {
                    Vector3f pn = fluid.Predicted[neighborIndex];

                    Vector3d gradW = fluid.Kernel.GradW(pi.x - pn.x, pi.y - pn.y, pi.z - pn.z);

                    float lambda = (fluid.Lambda[i] + fluid.Lambda[neighborIndex]) * -MassMulInvDensity;
                    corr.x -= lambda * (float)gradW.x;
                    corr.y -= lambda * (float)gradW.y;
                    corr.z -= lambda * (float)gradW.z;
                }
                else
                {
                    int k = neighborIndex - fluid.NumParticles;

                    Vector3f pn = Boundary.Positions[k];

                    Vector3d gradW = fluid.Kernel.GradW(pi.x - pn.x, pi.y - pn.y, pi.z - pn.z);

                    float lambda = fluid.Lambda[i] * -Boundary.Psi[k] * InvDensity;
                    corr.x -= lambda * (float)gradW.x;
                    corr.y -= lambda * (float)gradW.y;
                    corr.z -= lambda * (float)gradW.z;
                }
            }

            return corr;
        }
*/
    }
}
