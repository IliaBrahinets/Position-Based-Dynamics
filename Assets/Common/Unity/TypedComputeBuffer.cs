using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ComputeBuffer<T> where T : struct {

	private ComputeBuffer ActualBuffer;
	
	public ComputeBuffer(T[] items) {
		if(items == null){
			throw new ArgumentNullException("items is null");
		}
		ConstructWithLength(items.Length);
		SetData(items);
	}

	public ComputeBuffer(int lenght){
		ConstructWithLength(lenght);
	}

	private void ConstructWithLength(int lenght){
		if(lenght < 0){
			throw new ArgumentOutOfRangeException("length must not be less than zero");
		}
		ItemSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
		ActualBuffer = new ComputeBuffer(lenght, ItemSize);
	}

	public int ItemSize { get; private set; }

	public int Count {
		get {
			return ActualBuffer.count;
		}
	}

	public void SetData(T[] items){
		ActualBuffer.SetData(items);
	}

	public void GetData(T[] items){
		ActualBuffer.GetData(items);
	}

	public static implicit operator ComputeBuffer(ComputeBuffer<T> buffer){
		return buffer.ActualBuffer;
	}	

}
