using System;
using System.Collections.Generic;

using Common.Geometry.Shapes;
using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Bodies;
using PositionBasedDynamics.Bodies.Fluids;
using UnityEngine;

using PositionBasedDynamics.ShaderHelpers;

namespace PositionBasedDynamics.Solvers
{

    public class GPUFluidSolver3d
    {

        private FluidBody3d Body;

        private List<ExternalForce3d> Forces;

        #region GPU
        #region  consts
        private const int BLOCK_SIZE = 512;

        #endregion
        private ComputeShader CurrentShader;

        #endregion

        #region GPUInit
        private void InitCurrentShader(){
            string shaderName = "FluidSolverShader";

            CurrentShader = ShaderContext.Instance.GetComputeShader(shaderName);
        }
        #endregion
        public GPUFluidSolver3d(FluidBody3d body)
        {
            InitCurrentShader();

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
            
            CurrentShader.SetFloat("dt",(float)dt);
            CurrentShader.SetInt("MatterParticles", Body.NumParticles);
            
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
            int _kernel = CurrentShader.FindKernel("EstimatePositions");

            CurrentShader.SetBuffer(_kernel, "Velocities", Body.GPUVelocities);
            CurrentShader.SetBuffer(_kernel, "Positions",Body.GPUPositions);
            CurrentShader.SetBuffer(_kernel, "Predicted", Body.GPUPredicted);

            CurrentShader.Dispatch(_kernel, ShaderHelper.GetNumberOfDispatchGroups(Body.NumParticles,BLOCK_SIZE), 1, 1);
        }
        


        private void UpdateConstraint()
        {
            Body.ConstrainPositions(1);
        }

        private void UpdateVelocities(float dt)
        {
            int _kernel = CurrentShader.FindKernel("UpdateVelocities");

            CurrentShader.SetBuffer(_kernel, "Velocities", Body.GPUVelocities);
            CurrentShader.SetBuffer(_kernel, "Positions", Body.GPUPositions);
            CurrentShader.SetBuffer(_kernel, "Predicted", Body.GPUPredicted);

            CurrentShader.Dispatch(_kernel, ShaderHelper.GetNumberOfDispatchGroups(Body.NumParticles,BLOCK_SIZE), 1, 1);
            
        }

        private void UpdatePositions()
        {
            //for (int i = 0; i < Body.NumParticles; i++)
            //{
              //  Body.Positions[i] = Body.Predicted[i];
            //}

            Body.GPUPredicted.GetData(Body.Positions);
            Body.GPUPositions.SetData(Body.Positions);

            Body.VelocitiesToCpu();

            Body.DensitiesToCpu();
            Body.LambaToCpu();
        }


    }

}