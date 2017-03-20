using UnityEngine;

namespace DT {
	public class Chunk {
		public DensityChunk Densities;
		public Vector3 Min; // coordinates of minimum x, y, and z voxel corner
		public int Size;
		public GameObject Object;
		public float CreationTime;
		public string Key; // key in hashmap
	}
}