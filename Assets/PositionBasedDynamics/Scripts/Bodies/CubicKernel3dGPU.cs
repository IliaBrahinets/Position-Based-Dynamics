using System;

using Common.Mathematics.LinearAlgebra;
using UnityEngine;

namespace PositionBasedDynamics.Bodies
{

    public class CubicKernel3dGPU
    {
        float K;
        float L;
        float W_zero;
        float radius;

        public CubicKernel3dGPU(float radius)
        {    
            float h3 = radius * radius * radius;

            float K = 8.0f / (float)(Math.PI * h3);
            float L = 48.0f / (float)(Math.PI * h3);

            float W_zero = (float)new CubicKernel3d((double)radius).W_zero;

            this.K = K;
            this.L = L;
            this.W_zero = W_zero;
            this.radius = radius;
            
        }

        public void InitCubicKernel3dGPU(ComputeShader shader){
            shader.SetFloat("Radius",radius);
            shader.SetFloat("InvRadius",(1.0f/radius));
            shader.SetFloat("W_zero",W_zero);
            shader.SetFloat("K",K);
            shader.SetFloat("L",L);
        }
 
    }
}