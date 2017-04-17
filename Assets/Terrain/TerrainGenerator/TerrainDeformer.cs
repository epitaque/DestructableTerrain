using UnityEngine;

public static class TerrainDeformer {


	public static void SphericalDeform(Vector3 position, DT.Chunk c, int newMaterial, float radius, float hardness, bool additive) {
		int res = c.Size;
		Vector3 newPos = new Vector3();
		for(int x = 0; x <= res; x++) {
			newPos.x = x;
			for(int y = 0; y <= res; y++) {
				newPos.y = y;
				for(int z = 0; z <= res; z++) {
					newPos.z = z;
					//newPos += c.Min;
					float distance = Vector3.Distance(newPos + c.Min, position);
					if(distance > radius) {
						continue;
					}
					float p = distance / radius;
					float dd = 1 - Mathf.Pow(p, 2);
					if(additive) c.Densities.Grid[x, y, z].Density += dd;
					else c.Densities.Grid[x, y, z].Density -= dd;
					if(newMaterial != 0) c.Densities.Grid[x,y,z].Material = newMaterial;
				}
			}
		}
	}
}