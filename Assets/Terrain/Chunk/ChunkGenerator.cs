using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using SE;
using UnityEngine;

namespace DT {
public static class ChunkGenerator {
	public static ChunkProcessOutput CreateChunk(ChunkProcessInput input) {
		UnityEngine.Debug.Log("CreateChunk started");
		Stopwatch s = new Stopwatch();
		s.Start();

		SE.OpenSimplexNoise noise = new SE.OpenSimplexNoise(0);

		ChunkProcessOutput output = new ChunkProcessOutput();

		output.threadId = input.threadId;
		output.key = input.key;
		output.densities = new DT.DensityChunk(input.size, (Vector3 position) => sample(noise, position + input.min));
		SE.Mesh m = SE.MarchingCubes.March(0f, output.densities);
		m.normals = NormalSolver.RecalculateNormals(m.triangles, m.vertices, 60);
		m.uvs = SetMeshUVs(m);
		FixNormals(m);
		output.mesh = m;
		s.Stop();
		output.processingTime = s.ElapsedMilliseconds;
		return output;
	}

	public static List<Vector4> SetMeshUVs(SE.Mesh m) {
		if(m.triangles.Length != m.vertices.Length) {
			UnityEngine.Debug.LogError("Terrain mesh has an unequal number of triangle indices and vertices");
		}

		List<Vector4> UV1s = new List<Vector4>(m.vertices.Length);
		float intensity;

		for(int i = 0; i < m.vertices.Length; i += 3) {
			for(int j = 0; j < 3; j++) {
				intensity = 1f - Mathf.Clamp(Mathf.Pow(m.normals[i + j].y, 3), 0f, 1f);
				UV1s.Add(new Vector4(0f, 1f, intensity, 0));
			}
		}

		return UV1s;
	}
	private static void FixNormals(SE.Mesh m) {
		/*
			Normal storage structure
				List<Vertices> - no duplicates
					List<Normals>

		*/

		Dictionary<string, List<KeyValuePair<int, Vector3>>> verticesNormals = new Dictionary<string, List<KeyValuePair<int, Vector3>>>();
		
		for(int i = 0; i < m.vertices.Length; i++) {
			//print("#vertices: " + m.vertices.Length);
			string vertS = m.vertices[i].ToString();
			if(!verticesNormals.ContainsKey(vertS)) {
				//print("here 1");
				verticesNormals.Add(vertS, new List<KeyValuePair<int, Vector3>>());
			}
			verticesNormals[vertS].Add(new KeyValuePair<int, Vector3>(i, m.normals[i]));
		}
		Vector3[] newNormals = new Vector3[m.normals.Length];
		foreach(KeyValuePair<string, List<KeyValuePair<int, Vector3>>> l in verticesNormals) {
			//print("here 2" + l.Value.Count);

			Vector3 sum = new Vector3();
			for(int i = 0; i < l.Value.Count; i++) {
				sum += l.Value[i].Value;
			}
			sum /= l.Value.Count;
			for(int i = 0; i < l.Value.Count; i++) {
				newNormals[l.Value[i].Key] = sum;
			}
		}
		m.normals = newNormals;
	}

	private static float sample(SE.OpenSimplexNoise noise, Vector3 position) {
		float r = 0.3f;
		float result = 10.0f - position.y;
		result += (float)noise.Evaluate((float)position.x*r, (float)position.y*r, (float)position.z*r) * 3;
		return result;
	}
}



}


namespace SEM {

	public static class MarchingCubes {		
		public static readonly Vector3[] OFFSETS = {
			new Vector3(0f,0f,0f), new Vector3(1f,0f,0f), new Vector3(1f,1f,0f), new Vector3(0f,1f,0f), 
			new Vector3(0f,0f,1f), new Vector3(1f,0f,1f), new Vector3(1f,1f,1f), new Vector3(0f,1f,1f) };	

		public static SE.Mesh March(int resolution, float isovalue, Sample sample, bool flatShading) {
			if(flatShading == true) return MarchFlatShading(resolution, isovalue, sample);
			else return MarchEfficient(resolution, isovalue, sample);
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
			SE.Mesh mesh = new SE.Mesh();
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

	}

}
