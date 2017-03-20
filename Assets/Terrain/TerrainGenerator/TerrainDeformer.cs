using UnityEngine;

public static class TerrainDeformer {


	public static void SphericalDeform(Vector3 position, DT.Chunk c, int newMaterial, float radius, float hardness) {
		int res = c.Size;
		Vector3 newPos = new Vector3();
		for(int x = 0; x < res; x++) {
			newPos.x = x;
			for(int y = 0; y < res; y++) {
				newPos.y = y;
				for(int z = 0; z < res; z++) {
					newPos.z = z;
					float distance = Vector3.Distance(newPos, position);
					if(distance > radius) {
						continue;
					}
					float p = distance / radius;
					float dd = 1 - Mathf.Pow(p, 2);
					c.Densities.Grid[x, y, z].Density -= dd / 10f;
					if(newMaterial != 0) c.Densities.Grid[x,y,z].Material = newMaterial;
				}
			}
		}
	}
}