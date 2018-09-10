using Common.Mathematics.LinearAlgebra;
using UnityEngine;

namespace PositionBasedDynamics.Collisions
{
    class ParticleNeighboursSearcherGPU
    {
        private BitonicSorterPrepearerGPU Prepearer;
        private BitonicSorterPrepearerWithBoundaryGPU PrepearerWithBoundary;
        private BitonicSorterGPU Sorter;
        private NeighboursMapConstructor MapConstructor;

        public ComputeBuffer NeighboursMap;
        public ComputeBuffer NumNeighbours;

        public ParticleNeighboursSearcherGPU(float cellSize){
            Prepearer = new BitonicSorterPrepearerGPU(cellSize);
            PrepearerWithBoundary = new BitonicSorterPrepearerWithBoundaryGPU(cellSize);
            Sorter = new BitonicSorterGPU();
            MapConstructor = new NeighboursMapConstructor(cellSize);  
        }

        public void NeighbourhoodSearch(ComputeBuffer rawParticles){

            Prepearer.PrepareData(rawParticles);

            Sorter.Sort(Prepearer.Prepeared); 

            MapConstructor.Construct(Prepearer.Prepeared,rawParticles,new ComputeBuffer(1,1));      

            NeighboursMap = MapConstructor.NeighboursMap;
            NumNeighbours = MapConstructor.NumNeighbours;     
        }

        public void NeighbourhoodSearch(ComputeBuffer rawParticles, ComputeBuffer rawBoundaryParticle){

            PrepearerWithBoundary.PrepareData(rawParticles,rawBoundaryParticle);

            Sorter.Sort(PrepearerWithBoundary.Prepeared); 

            MapConstructor.Construct(PrepearerWithBoundary.Prepeared,rawParticles,rawBoundaryParticle);      

            NeighboursMap = MapConstructor.NeighboursMap;
            NumNeighbours = MapConstructor.NumNeighbours; 
        }
    }

}