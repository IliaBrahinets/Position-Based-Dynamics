using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ComputeBuffer<T> where T : struct {

	private ComputeBuffer ActualBuffer;

	public ComputeBuffer(T[] items) {
		ItemSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
		ActualBuffer = new ComputeBuffer(items.Length, ItemSize);
	}

	public int ItemSize { get; private set;}

	public void SetData(T[] items){
		ActualBuffer.SetData(items);
	}

	public void GetData(T[] items){
		ActualBuffer.GetData(items);
	}

	public static explicit operator ComputeBuffer(ComputeBuffer<T> buffer){
		return buffer.ActualBuffer;
	}	

}
