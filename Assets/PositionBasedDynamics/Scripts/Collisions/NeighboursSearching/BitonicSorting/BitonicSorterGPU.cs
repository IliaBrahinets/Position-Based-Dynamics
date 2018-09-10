using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PositionBasedDynamics.Collisions
{
	public sealed class BitonicSorterGPU:IDisposable {

		#region consts
		private const uint BITONIC_BLOCK_SIZE = 256;
		private const uint TRANSPOSE_BLOCK_SIZE = 16;
		private const uint MIN_COUNT = BITONIC_BLOCK_SIZE * TRANSPOSE_BLOCK_SIZE;
		#endregion

		#region BitonicSort
		private ComputeShader bitonicShader;
		private ComputeBuffer tempBuffer;
		private int KERNEL_ID_BITONICSORT;
		private int KERNEL_ID_TRANSPOSE;
		#endregion

		public BitonicSorterGPU()
		{
			this.bitonicShader = ShaderContext.Instance.GetComputeShader("BitonicSorterShader");
			KERNEL_ID_BITONICSORT = bitonicShader.FindKernel("BitonicSort");
			KERNEL_ID_TRANSPOSE = bitonicShader.FindKernel("MatrixTranspose");
		}

		public void Sort(ComputeBuffer<Particle> elements)
		{
			uint NUM_ELEMENTS = (uint)elements.Count;

			SortDataValidation(NUM_ELEMENTS);
			
			uint MATRIX_WIDTH = BITONIC_BLOCK_SIZE;
			uint MATRIX_HEIGHT = NUM_ELEMENTS / BITONIC_BLOCK_SIZE;

			for (uint level = 2; level <= BITONIC_BLOCK_SIZE; level <<= 1)
			{
				SetGPUSortConstants(level, level, MATRIX_HEIGHT, MATRIX_WIDTH);

				// Sort the row data
				bitonicShader.SetBuffer(KERNEL_ID_BITONICSORT, "Data", elements);
				bitonicShader.Dispatch(KERNEL_ID_BITONICSORT, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
			}

			if(tempBuffer == null){
				tempBuffer = new ComputeBuffer((int)NUM_ELEMENTS, Particle.SIZE);
			}

			// Then sort the rows and columns for the levels > than the block size
			// Transpose. Sort the Columns. Transpose. Sort the Rows.
			for (uint level = (BITONIC_BLOCK_SIZE << 1); level <= NUM_ELEMENTS; level <<= 1)
			{
				// Transpose the data from buffer 1 into buffer 2
				SetGPUSortConstants(level / BITONIC_BLOCK_SIZE, (level & ~NUM_ELEMENTS) / BITONIC_BLOCK_SIZE, MATRIX_WIDTH, MATRIX_HEIGHT);
				bitonicShader.SetBuffer(KERNEL_ID_TRANSPOSE, "Input", elements);
				bitonicShader.SetBuffer(KERNEL_ID_TRANSPOSE, "Data", tempBuffer);
				bitonicShader.Dispatch(KERNEL_ID_TRANSPOSE, (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), 1);

				// Sort the transposed column data
				bitonicShader.SetBuffer(KERNEL_ID_BITONICSORT, "Data", tempBuffer);
				bitonicShader.Dispatch(KERNEL_ID_BITONICSORT, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);

				// Transpose the data from buffer 2 back into buffer 1
				SetGPUSortConstants(BITONIC_BLOCK_SIZE, level, MATRIX_HEIGHT, MATRIX_WIDTH);
				bitonicShader.SetBuffer(KERNEL_ID_TRANSPOSE, "Input", tempBuffer);
				bitonicShader.SetBuffer(KERNEL_ID_TRANSPOSE, "Data", elements);
				bitonicShader.Dispatch(KERNEL_ID_TRANSPOSE, (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), 1);

				// Sort the row data
				bitonicShader.SetBuffer(KERNEL_ID_BITONICSORT, "Data", elements);
				bitonicShader.Dispatch(KERNEL_ID_BITONICSORT, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
			}

		}

		private void SortDataValidation(uint count)
		{
			if(count < MIN_COUNT) {
				throw new ArgumentOutOfRangeException(string.Format("the mincount of the current configuration {0}, the current {1}", MIN_COUNT, count));
			}
		}

		private void SetGPUSortConstants(uint level, uint levelMask, uint width, uint height)
		{
			bitonicShader.SetInt("_Level", (int)level);
			bitonicShader.SetInt("_LevelMask", (int)levelMask);
			bitonicShader.SetInt("_Width", (int)width);
			bitonicShader.SetInt("_Height", (int)height);
		}

		#region IDisposable Support
		public void Dispose()
		{
			tempBuffer.Dispose();
		}
		#endregion


	}
}
