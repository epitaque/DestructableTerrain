using UnityEngine;

namespace DT {
	public struct Voxel {
		public float Density;
		public int Material;
	}

	public class DensityChunk {
		public int Size;
		public int GridSize;
		public Voxel[,,] Grid;

		private SE.Sample ssample;

		public DensityChunk(int size) {
			this.Size = size;
			this.GridSize = size + 1;
			Grid = new Voxel[this.GridSize, this.GridSize, this.GridSize];
		}

		public DensityChunk(int size, SE.Sample sample) {
			ssample = sample;
			this.Size = size;
			this.GridSize = size + 1;
			Grid = new Voxel[this.GridSize, this.GridSize, this.GridSize];
			for(int x = 0; x < this.GridSize; x++) {
				for(int y = 0; y < this.GridSize; y++) {
					for(int z = 0; z < this.GridSize; z++) {
						Grid[x, y, z].Density = sample(new Vector3(x, y, z));
					}
				}
			}
		}

		public float Density(Vector3 position) {
			position.x = Mathf.Clamp(position.x, 0, Size);
			position.y = Mathf.Clamp(position.y, 0, Size);
			position.z = Mathf.Clamp(position.z, 0, Size);

			return Grid[(int)position.x, (int)position.y, (int)position.z].Density;			
		}
	}
}
