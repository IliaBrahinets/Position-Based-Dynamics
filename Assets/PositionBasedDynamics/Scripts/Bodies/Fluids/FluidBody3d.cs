using System;
using System.Collections.Generic;

using Common.Geometry.Shapes;
using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Collisions;
using PositionBasedDynamics.Sources;
using PositionBasedDynamics.Constraints;
using UnityEngine;
using PositionBasedDynamics.ShaderHelpers;
namespace PositionBasedDynamics.Bodies.Fluids
{

    public class FluidBody3d : Body3d
    {

        public float Density { get; set; }

        public float Viscosity { get; set; }

        public float[] Lambda { get; private set; }

        public ComputeBuffer<float> GPULambda { get; private set; }

        internal CubicKernel3dGPU Kernel { get; private set; }

        internal ParticleNeighboursSearcherGPU NeighboursSearcher { get; private set; }

        internal float[] Densities { get; private set; }

        internal ComputeBuffer<float> GPUDensities { get; private set; }

        private ComputeBuffer GPUVelocitiesDelta;

        #region GPU
        private const int BLOCK_SIZE = 512;
        private ComputeShader CurrentShader;
        private void InitCurrentShader(){
            CurrentShader = ShaderContext.Instance.GetComputeShader("FluidBodyShader");
            KERNEL_ID_ComputeViscosityVelocityGradStep = CurrentShader.FindKernel("ComputeViscosityVelocityGradStep");
            KERNEL_ID_ComputeViscosityUpdateVelocityStep = CurrentShader.FindKernel("ComputeViscosityUpdateVelocityStep");

        }

        private void InitShaderConsts()
        {
            CurrentShader.SetInt("NumMatterParticles", NumParticles);
            CurrentShader.SetFloat("ViscosityMulMass",Viscosity*ParticleMass);
        }
        private int KERNEL_ID_ComputeViscosityVelocityGradStep; 
        private int KERNEL_ID_ComputeViscosityUpdateVelocityStep;

        #endregion

        #region BetweenGPU&CPUDataTransfer

        public void DensitiesToGpu(){
            GPUDensities.SetData(Densities);
        }
        public void LambdaToGpu(){
            GPULambda.SetData(Lambda);
        }
        public void DensitiesToCpu(){
            GPUDensities.GetData(Densities);
        }
        public void LambaToCpu(){
            GPULambda.GetData(Lambda);
        }
        #endregion
        public FluidBody3d(ParticleSource source, float radius, float density, Matrix4x4f RTS)
            : base(source.NumParticles, radius, 1.0f)
        {
            Density = density;
            Viscosity = 0.02f;
            Dampning = 1;

            float d = ParticleDiameter;
            ParticleMass = 0.8f * d * d * d * Density;

            CreateParticles(source, RTS);

            float cellSize = ParticleRadius * 4.0f;
            
            Kernel = new CubicKernel3dGPU(cellSize);

            NeighboursSearcher = new ParticleNeighboursSearcherGPU(cellSize);

            Lambda = new float[NumParticles];
            GPULambda = new ComputeBuffer<float>(NumParticles);
            Densities = new float[NumParticles];
            GPUDensities = new ComputeBuffer<float>(NumParticles);

            InitCurrentShader();
            InitShaderConsts();
            Kernel.InitCubicKernel3dGPU(CurrentShader);
        }

        public void AddBoundry(FluidBoundary3d boundry)
        {
            FluidConstraint3dGPU constraint = new FluidConstraint3dGPU(this, boundry);
            Constraints.Add(constraint);
        }

        internal void Reset()
        {
            for (int i = 0; i < NumParticles; i++)
            {
                Lambda[i] = 0.0f;
                Densities[i] = 0.0f;
            }
            LambdaToGpu();
            DensitiesToGpu();
        }

        public void RandomizePositionOrder(System.Random rnd)
        {
            for(int i = 0; i < NumParticles; i++)
            {
                Vector3f tmp = Positions[i];
                int idx = rnd.Next(0, NumParticles - 1);
                Positions[i] = Positions[idx];
                Positions[idx] = tmp;
            }

            Array.Copy(Positions, Predicted, NumParticles);

            PositionsToGpu();
            PredictedToGpu();
        }

        internal void ComputeViscosity()
        {
            //calc grad
            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityVelocityGradStep,"NeighboursMap",NeighboursSearcher.NeighboursMap);
            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityVelocityGradStep,"NumNeighbours",NeighboursSearcher.NumNeighbours);     
            
            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityVelocityGradStep,"Predicted", GPUPredicted);

            if(GPUVelocitiesDelta == null){
                GPUVelocitiesDelta = new ComputeBuffer(GPUVelocities.Count,GPUVelocities.ItemSize);
            }

            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityVelocityGradStep,"Velocities", GPUVelocities);
            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityVelocityGradStep,"VelocitiesDelta", GPUVelocitiesDelta);

            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityVelocityGradStep,"Densities", GPUDensities);

            CurrentShader.Dispatch(KERNEL_ID_ComputeViscosityVelocityGradStep, ShaderHelper.GetNumberOfDispatchGroups(NumParticles,BLOCK_SIZE),1,1);

            //upd velocities using calculated grad
            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityUpdateVelocityStep,"Velocities", GPUVelocities);
            CurrentShader.SetBuffer(KERNEL_ID_ComputeViscosityUpdateVelocityStep,"VelocitiesDelta", GPUVelocitiesDelta);

            CurrentShader.Dispatch(KERNEL_ID_ComputeViscosityUpdateVelocityStep, ShaderHelper.GetNumberOfDispatchGroups(NumParticles,BLOCK_SIZE),1,1);

        }

        private void CreateParticles(ParticleSource source, Matrix4x4f RTS)
        {

            for (int i = 0; i < NumParticles; i++)
            {
                Vector4f pos = RTS * source.Positions[i].xyz1;
                Positions[i] = new Vector3f(pos.x, pos.y, pos.z);
                Predicted[i] = Positions[i];
            }

            PositionsToGpu();
            PredictedToGpu();
        }

    }


}