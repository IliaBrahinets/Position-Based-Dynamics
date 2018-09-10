using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;
using UnityEngine;

using PositionBasedDynamics.ShaderHelpers;

namespace PositionBasedDynamics.Forces
{

    public class GravitationalForce3d : ExternalForce3d
    {
        public static readonly Vector3f DefaultGravity = new Vector3f(0,-9.81f,0);
        public Vector3f Gravity { get; set; }

        #region GPU
        private ComputeShader CurrentShader;
        private int _kernel;

        private const int BLOCK_SIZE = 512;
        #endregion

        #region GPUInit

        private void InitGPU(){
            InitCurrentShader();
            InitKernel();
        }
        private void InitCurrentShader(){
            string shaderName = "GravitationalForceShader";

            CurrentShader = ShaderContext.Instance.GetComputeShader(shaderName);
        }

        private void InitKernel(){

            string kernelName = "CSMains";

            _kernel = CurrentShader.FindKernel(kernelName);
        }
        #endregion

        public GravitationalForce3d(Vector3f gravity)
        {
            InitGPU();
            Gravity = gravity;
        }

        public GravitationalForce3d()
        {
            InitGPU();
            Gravity = DefaultGravity;
        }

        public override void ApplyForce(double dt, Body3d body)
        {
            string BufferName = "Velocities";
            CurrentShader.SetBuffer(_kernel, BufferName, body.GPUVelocities);
            CurrentShader.SetFloat("Gravity",(float)Gravity.y);
            CurrentShader.SetFloat("dt", (float)dt);
            CurrentShader.SetInt("MatterParticles",body.NumParticles);

            CurrentShader.Dispatch(_kernel, ShaderHelper.GetNumberOfDispatchGroups(body.NumParticles,BLOCK_SIZE), 1, 1);

        }
    }

}
