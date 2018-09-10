using System.Collections.Generic;
using UnityEngine;
using Common.Mathematics.LinearAlgebra;
using System;
using PositionBasedDynamics.Bodies.Fluids;

namespace PositionBasedDynamics
{
    public class ShaderContext:MonoBehaviour
    {
        public static ShaderContext Instance { 
            get {
                return GameObject.FindObjectOfType<ShaderContext>();
            }
        }

        [SerializeField]private List<string> shadersName;
        [SerializeField]private List<ComputeShader> shaders;

        
        public ComputeShader GetComputeShader(string shaderName)
        {
            int index = shadersName.IndexOf(shaderName);

            if(index == -1)
            {
                return null;
            }

            return shaders[index];
        }

    }

}