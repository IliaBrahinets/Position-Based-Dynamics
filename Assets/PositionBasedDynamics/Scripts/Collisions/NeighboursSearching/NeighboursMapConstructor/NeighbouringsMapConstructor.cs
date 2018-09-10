using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Mathematics.LinearAlgebra;
using PositionBasedDynamics.ShaderHelpers;
namespace PositionBasedDynamics.Collisions
{
    public sealed class NeighboursMapConstructor {
        #region  consts
        public const uint MAX_NEIGHBOURS = 120;
        private const uint BLOCK_SIZE = 512;
        #endregion

        public ComputeBuffer<uint> NeighboursMap { get; private set; }
        public ComputeBuffer<uint> NumNeighbours { get; private set; }

        private ComputeShader NeighboursMapShader;
        private ComputeBuffer<Vector3i> NeighbourShifts;
        private float cellSize;

        private int KERNEL_ID_CONSTRUCT;

        public NeighboursMapConstructor(float cellSize){
            NeighboursMapShader = ShaderContext.Instance.GetComputeShader("NeighboursMapConstructorShader");
            KERNEL_ID_CONSTRUCT = NeighboursMapShader.FindKernel("ConstructMap");

            NeighbourShifts = new ComputeBuffer<Vector3i>(GetNeighbourShifts());

            this.cellSize = cellSize;
        }

         public void Construct(ComputeBuffer<Particle> sortedParticles, ComputeBuffer<Vector3f> matterParticles, ComputeBuffer<Vector3f> boundaryParticles){
            int numAllParticles = sortedParticles.Count;
            int numMatterParticles = matterParticles.Count;

            NeighboursMapShader.SetFloat("CellSize",(float)cellSize);
            NeighboursMapShader.SetFloat("InvCellSize",1f/cellSize);
            
            NeighboursMapShader.SetFloat("AcceptenceRadius", cellSize);
            NeighboursMapShader.SetInt("NumAllParticles", (int)numAllParticles);
            NeighboursMapShader.SetInt("NumMatterParticles", (int)numMatterParticles);

            NeighboursMapShader.SetBuffer(KERNEL_ID_CONSTRUCT,"NeighbourShifts",NeighbourShifts);

            NeighboursMapShader.SetBuffer(KERNEL_ID_CONSTRUCT,"MatterParticlesCoords",matterParticles);
            NeighboursMapShader.SetBuffer(KERNEL_ID_CONSTRUCT,"BoundaryParticlesCoords",boundaryParticles);
            NeighboursMapShader.SetBuffer(KERNEL_ID_CONSTRUCT,"SortedParticles",sortedParticles);

            if(NeighboursMap == null){
                NeighboursMap = new ComputeBuffer<uint>((int)MAX_NEIGHBOURS*numMatterParticles);
                NumNeighbours = new ComputeBuffer<uint>(numMatterParticles);
            }
            
            NeighboursMapShader.SetBuffer(KERNEL_ID_CONSTRUCT,"NeighboursMap",NeighboursMap);
            NeighboursMapShader.SetBuffer(KERNEL_ID_CONSTRUCT,"NumNeighbours",NumNeighbours);
          
            NeighboursMapShader.Dispatch(KERNEL_ID_CONSTRUCT,ShaderHelper.GetNumberOfDispatchGroups(numMatterParticles,(int)BLOCK_SIZE),1,1);;

         }

        private Vector3i[] GetNeighbourShifts(){

            var result = new List<Vector3i>();

            for(int i = 0; i < 3; i++)
            for(int j = 0; j < 3; j++)
            for(int k = 0; k < 3; k++){
                
                if(i == 1 && j == 1 && k == 1){
                    continue;
                }

                result.Add(new Vector3i(i-1,j-1,k-1));

            }

            return result.ToArray();
        }
    }
}