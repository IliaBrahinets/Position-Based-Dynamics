using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PositionBasedDynamics.ShaderHelpers;

namespace PositionBasedDynamics.Collisions
{
    public sealed class BitonicSorterPrepearerWithBoundaryGPU {
        #region  consts
        private const uint BLOCK_SIZE = 512;
        #endregion
        public ComputeBuffer Prepeared;
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
        public void PrepareData(ComputeBuffer matterParticles, ComputeBuffer boundaryParticles){

            int numAll = matterParticles.count + boundaryParticles.count;
            
            int numMatterParticles = matterParticles.count;
            SortDataPrepearerShader.SetInt("NumMatterParticles",numMatterParticles);

            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "InputMatterParticles", matterParticles);
            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "InputBoundaryParticles", boundaryParticles);
            
            if(Prepeared == null){
                Prepeared = new ComputeBuffer(numAll, Particle.SIZE);
            }

            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "Output", Prepeared);
            SortDataPrepearerShader.Dispatch(KERNEL_ID_CONVERT, ShaderHelper.GetNumberOfDispatchGroups(numAll,(int)BLOCK_SIZE), 1 , 1);
        }

    }
}