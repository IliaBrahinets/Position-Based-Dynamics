using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PositionBasedDynamics.ShaderHelpers;
using Common.Mathematics.LinearAlgebra;

namespace PositionBasedDynamics.Collisions
{
    public sealed class BitonicSorterPrepearerGPU {
        #region  consts
        private const uint BLOCK_SIZE = 512;
        #endregion
        public ComputeBuffer<Particle> Prepeared;

        private double cellSize;
        private double InvCellSize;
        private ComputeShader SortDataPrepearerShader;
        private int KERNEL_ID_CONVERT;

        public BitonicSorterPrepearerGPU(float cellSize){
            this.cellSize = cellSize;
            this.InvCellSize = 1.0 / cellSize;
            
            SortDataPrepearerShader = ShaderContext.Instance.GetComputeShader("SortDataPrepearerShader");
            KERNEL_ID_CONVERT = SortDataPrepearerShader.FindKernel("Convert");

            InitShaderConsts();
        }

        private void InitShaderConsts(){
            SortDataPrepearerShader.SetFloat("CellSize",(float)cellSize);
            SortDataPrepearerShader.SetFloat("InvCellSize",(float)InvCellSize);
        }
        public void PrepareData(ComputeBuffer<Vector3f> elements){

            int numAll = elements.Count;
            
            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "Input", elements);
            
            if(Prepeared == null){
                Prepeared = new ComputeBuffer<Particle>(numAll);
            }

            SortDataPrepearerShader.SetBuffer(KERNEL_ID_CONVERT, "Output", Prepeared);
            SortDataPrepearerShader.Dispatch(KERNEL_ID_CONVERT, ShaderHelper.GetNumberOfDispatchGroups(numAll,(int)BLOCK_SIZE), 1 , 1);
        }

    }
}