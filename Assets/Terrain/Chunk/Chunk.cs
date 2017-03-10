using UnityEngine;

namespace DT {
	public struct Voxel {
		public float density;
		public int material;
	}

	public class Chunk {
		public int size;
		public Voxel[,,] grid;

		public Chunk(int size) {
			this.size = size;
			grid = new Voxel[size, size, size];
		}

		public Chunk(int size, SE.Sample sample) {
			this.size = size;
			grid = new Voxel[size, size, size];
			for(int x = 0; x < size; x++) {
				for(int y = 0; y < size; y++) {
					for(int z = 0; z < size; z++) {
						grid[x, y, z].density = sample(new Vector3(x, y, z));
					}
				}
			}
		}

		public float Density(Vector3 position) {
			position.x = Mathf.Clamp(position.x, 0, size - 1);
			position.y = Mathf.Clamp(position.y, 0, size - 1);
			position.z = Mathf.Clamp(position.z, 0, size - 1);

			return grid[(int)position.x, (int)position.y, (int)position.z].density;			
		}
	}
}
