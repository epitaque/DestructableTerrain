using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace SE {

	public static class MarchingCubes {		
		public static readonly Vector3[] OFFSETS = {
			new Vector3(0f,0f,0f), new Vector3(1f,0f,0f), new Vector3(1f,1f,0f), new Vector3(0f,1f,0f), 
			new Vector3(0f,0f,1f), new Vector3(1f,0f,1f), new Vector3(1f,1f,1f), new Vector3(0f,1f,1f) };	
		public static readonly int[,] IntegerOFFSETS = {
			{0,0,0}, {1,0,0}, {1,1,0}, {0,1,0}, 
			{0,0,1}, {1,0,1}, {1,1,1}, {0,1,1} };	

		public static SE.Mesh March(int resolution, float isovalue, Sample sample, bool flatShading) {
			if(flatShading == true) return MarchFlatShading(resolution, isovalue, sample);
			else return MarchEfficient(resolution, isovalue, sample);
		}

		public static SE.Mesh March(float isovalue, DT.DensityChunk c) {
			return MarchDensityChunk(isovalue, c);
		}

		private static SE.Mesh MarchEfficient(int resolution, float isovalue, Sample sample) {
			int stepsXZPP = (resolution + 1) * (resolution + 1);
			int stepsXPP = resolution + 1;
			int stepsZPP = resolution + 1;

			Hashtable vertexIndexLookup = new Hashtable();
			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();

			float[] cachedIsoValues = new float[stepsXPP * stepsZPP * 2];

			int iNextIsoCacheLayer = 0;
			bool bFirstRun = true;

			Vector3 cellCornerPos = new Vector3();

			GridCell cell = new GridCell();
			cell.points = new Point[8];
			for(int i = 0; i < 8; i++) {
				cell.points[i] = new Point();
				cell.points[i].position = new Vector3();
			}

			List<Vector3> result = new List<Vector3>();

			// Loop over grid cells
			for (int y = 0; y < resolution; y++)
			{
				// Previous bottom (now top) layer, new bottom layer
				int[] iIsoCacheLayers = { (iNextIsoCacheLayer + 1) % 2, iNextIsoCacheLayer };

				// Overwrite previous bottom layer next time
				iNextIsoCacheLayer = iIsoCacheLayers[0];

				// Fill both layers during the first run, 
				for (int r = 0; r < 1 + (bFirstRun ? 1 : 0); r++)
				{
					// New bottom layer first, old layer afterwards, iff first run
					int iCacheBase = iIsoCacheLayers[1 - r] * stepsXZPP; // stepsYZPP = (resolution + 1) * (resolution + 1)
					int ypp = y + 1 - r;

					// Loop over all corner points in this layer
					for (int x = 0; x < stepsXPP; x++) // stepsYPP = resolution + 1
					{
						int iCacheBaseX = iCacheBase + x * stepsZPP;

						for (int z = 0; z < stepsZPP; z++) // stepsZPP = resolution + 1
						{
							cellCornerPos.x = (float)x; cellCornerPos.y = (float)ypp; cellCornerPos.z = (float)z;

							// Compute iso value once
							cachedIsoValues[ iCacheBaseX + z ] = sample(cellCornerPos);
						}
					}
				}

				for (int x = 0; x < resolution; ++x)
					for (int z = 0; z < resolution; ++z)
					{					
						// Loop over 8 cell corners
						for (int i = 0; i < 8; ++i)
						{
							// Compute cell corner position
							Vector3 intCornerSpace = new Vector3((float)x, (float)y, (float)z) + OFFSETS[i];
							cell.points[i].position = intCornerSpace;

							// Sample iso value
							int iCacheBase = iIsoCacheLayers[(int)OFFSETS[i].y] * stepsXZPP;
							int iCacheBaseY = iCacheBase + (int)intCornerSpace.x * stepsZPP;
							cell.points[i].density = cachedIsoValues[iCacheBaseY + (int)intCornerSpace.z];
						}

						// Triangulate cell
						Polyganizer.PolyganizeGridCell(cell, result, isovalue);

						// Loop over vertices
						for (int i = 0; i < result.Count; ++i)
						{
							string key = result[i].ToString();
							if(vertexIndexLookup.ContainsKey(key)) { // vertex already exists, retrieve its index and add it to triangles
								triangles.Add((int)vertexIndexLookup[key]);
							}
							else { // vertex doesn't exist, add it to vertex array and hashtable
								vertices.Add(result[i]);
								triangles.Add(vertices.Count - 1); // index of newest vertex
								vertexIndexLookup.Add(key, vertices.Count - 1);
							}
						}
						result.Clear();
					}

				// First run is over
				bFirstRun = false;
			}

			SE.Mesh mesh = new SE.Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			return mesh;
		}

		public static SE.Mesh MarchFlatShading(int resolution, float isovalue, Sample sample) {
			Mesh mesh = new Mesh();
			GridCell cell = new GridCell();
			cell.points = new Point[8];
			for(int i = 0; i < 8; i++) {
				cell.points[i] = new Point();
				cell.points[i].position = new Vector3();
			}

			List<Vector3> vertices = new List<Vector3>();

			Vector3[] OFFSETS = {
				new Vector3(0f,0f,0f), new Vector3(1f,0f,0f), new Vector3(1f,1f,0f), new Vector3(0f,1f,0f), 
				new Vector3(0f,0f,1f), new Vector3(1f,0f,1f), new Vector3(1f,1f,1f), new Vector3(0f,1f,1f) };	

			for(int x = 0; x < resolution; x++) {
				for(int y = 0; y < resolution; y++) {
					for(int z = 0; z < resolution; z++) {										
						for(int i = 0; i < 8; i++) {
							cell.points[i].position = new Vector3(x, y, z) + OFFSETS[i];
							cell.points[i].density = sample(cell.points[i].position);
						}
						Polyganizer.PolyganizeGridCell(cell, vertices, isovalue);

					}
				}
			}
			int[] triangles = new int[vertices.Count];
			for(int i = 0; i < vertices.Count; i ++) {
				triangles[i] = i;
			}

			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles;
			return mesh;
		}

		private static SE.Mesh MarchDensityChunk(float isovalue, DT.DensityChunk c) {
			Mesh mesh = new Mesh();
			GridCell cell = new GridCell();
			cell.points = new Point[8];
			for(int i = 0; i < 8; i++) {
				cell.points[i] = new Point();
				cell.points[i].position = new Vector3();
			}

			List<Vector3> vertices = new List<Vector3>();

			for(int x = 0; x < c.GridSize - 1; x++) {
				for(int y = 0; y < c.GridSize - 1; y++) {
					for(int z = 0; z < c.GridSize - 1; z++) {
						for(int i = 0; i < 8; i++) {
							cell.points[i].position.Set(x + OFFSETS[i].x, y + OFFSETS[i].y, z + OFFSETS[i].z);
							cell.points[i].density = c.Grid[IntegerOFFSETS[i,0] + x,IntegerOFFSETS[i,1] + y,IntegerOFFSETS[i,2] + z].Density;
						}
						Polyganizer.PolyganizeGridCell(cell, vertices, isovalue);
					}
				}
			}

			int[] triangles = new int[vertices.Count];
			for(int i = 0; i < vertices.Count; i ++) {
				triangles[i] = i;
			}

			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles;
			return mesh;
		}
	}

}
