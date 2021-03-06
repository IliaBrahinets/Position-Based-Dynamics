using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PositionBasedDynamics.ShaderHelpers;
using Common.Mathematics.LinearAlgebra;

namespace PositionBasedDynamics.Collisions
{
    public sealed class BitonicSorterPrepearerWithBoundaryGPU {
        #region  consts
        private const uint BLOCK_SIZE = 512;
        #endregion
        public ComputeBuffer<Particle> Prepeared { get; private set; }
        private float cellSize;
        private float InvCellSize;
        private ComputeShader SortDataPrepearerShader;
        private int KERNEL_ID_CONVERT;

        public BitonicSorterPrepearerWithBoundaryGPU(float cellSize){
            this.cellSize = cellSize;
            this.InvCellSize = 1.0f / cellSize;
            
            SortDataPrepearerShader = ShaderContext.Instance.GetComputeShader("SortDataPrepearerWithBoundaryShader");
            KERNEL_ID_CONVERT = SortDataPrepearerShader.FindKernel("Convert");

            InitShaderConsts();
        }

        private void InitShaderConsts(){
            SortDataPrepearerShader.SetFloat("CellSize",cellSize);
            SortDataPrepearerShader.SetFloat("InvCellSize",InvCellSize);
        }
        public void PrepareData(ComputeBuffer<Vector3f> matterParticles, ComputeBuffer<Vector3f> boundaryParticles){

            int numAll = matterParticles.Count + boundaryParticles.Count;
            
            int numMatterParticles = matterParticles.Count;
            SortDataPrepearerShader.SetInt("NumMatterParticles",numMatterParticles);

            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "InputMatterParticles", matterParticles);
            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "InputBoundaryParticles", boundaryParticles);
            
            if(Prepeared == null){
                Prepeared = new ComputeBuffer<Particle>(numAll);
            }

            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "Output", Prepeared);
            SortDataPrepearerShader.Dispatch(KERNEL_ID_CONVERT, ShaderHelper.GetNumberOfDispatchGroups(numAll,(int)BLOCK_SIZE), 1 , 1);
        }

    }
}