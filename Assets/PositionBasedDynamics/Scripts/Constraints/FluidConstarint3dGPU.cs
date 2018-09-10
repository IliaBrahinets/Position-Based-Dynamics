using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;
using PositionBasedDynamics.Bodies;
using PositionBasedDynamics.Bodies.Fluids;
using PositionBasedDynamics.Collisions;
using UnityEngine;

using PositionBasedDynamics.ShaderHelpers;

namespace PositionBasedDynamics.Constraints
{
    public class FluidConstraint3dGPU : Constraint3d
    {
        private const int BLOCK_SIZE = 512;
        private FluidBoundary3d Boundary { get; set; }

        private int Iterations { get; set; }

        #region GPU
        private ComputeShader CurrentShader;

        private int KERNEL_ID_PRESTEP;
        private int KERNEL_DENSITYCONSTRAINT;

        void InitCurrentShader(){
            CurrentShader = ShaderContext.Instance.GetComputeShader("FluidConstraintShader");

            KERNEL_ID_PRESTEP = CurrentShader.FindKernel("PreStepForSolveDensityConstraint");
            KERNEL_DENSITYCONSTRAINT = CurrentShader.FindKernel("SolveDensityConstraint");
        }

        private ParticleHashTest testSearcher;
        #endregion
        internal FluidConstraint3dGPU(FluidBody3d body, FluidBoundary3d boundary) : base(body)
        {
            Iterations = 5;
            Boundary = boundary;

            InitCurrentShader();

            body.Kernel.InitCubicKernel3dGPU(CurrentShader);
           
            InitConstShaderData();
        }

        private void InitConstShaderData()
        {
            FluidBody3d fluid = Body as FluidBody3d;

            CurrentShader.SetInt("Iterations",Iterations);

            CurrentShader.SetFloat("FluidParticleMass",fluid.ParticleMass);
            CurrentShader.SetFloat("FluidDensity",fluid.Density);
            
        }

        internal override void ConstrainPositions(double di)
        {
            FluidBody3d fluid = Body as FluidBody3d;

            fluid.NeighboursSearcher.NeighbourhoodSearch(fluid.GPUPredicted, Boundary.GPUPositions);

            CurrentShader.SetInt("FluidNumParticles",fluid.NumParticles);
            //pre step(Calcuclate Density, Calculate Lambda Coeffs)
            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"NeighboursMap",fluid.NeighboursSearcher.NeighboursMap);
            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"NumNeighbours",fluid.NeighboursSearcher.NumNeighbours);          

            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"FluidPositions",fluid.GPUPositions);
            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"FluidPredicted",fluid.GPUPredicted);
            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"FluidDensities",fluid.GPUDensities);
            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"FluidLambda",fluid.GPULambda);

            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"BoundaryPositions",Boundary.GPUPositions);
            CurrentShader.SetBuffer(KERNEL_ID_PRESTEP,"BoundaryPsi",Boundary.GPUPsi);
                        
            //constraint
            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"NeighboursMap",fluid.NeighboursSearcher.NeighboursMap);
            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"NumNeighbours",fluid.NeighboursSearcher.NumNeighbours);          

            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"FluidPositions",fluid.GPUPositions);
            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"FluidPredicted",fluid.GPUPredicted);
            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"FluidDensities",fluid.GPUDensities);
            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"FluidLambda",fluid.GPULambda);

            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"BoundaryPositions",Boundary.GPUPositions);
            CurrentShader.SetBuffer(KERNEL_DENSITYCONSTRAINT,"BoundaryPsi",Boundary.GPUPsi);

            int iter = 0;
            while(iter < Iterations){
                //pre step(Calcuclate Density, Calculate Lambda Coeffs)
                CurrentShader.Dispatch(KERNEL_ID_PRESTEP,ShaderHelper.GetNumberOfDispatchGroups(fluid.NumParticles,BLOCK_SIZE),1,1);
    
                //constraint
                CurrentShader.Dispatch(KERNEL_DENSITYCONSTRAINT,ShaderHelper.GetNumberOfDispatchGroups(fluid.NumParticles,BLOCK_SIZE),1,1);

                iter++;
            }
        }


    }
}
