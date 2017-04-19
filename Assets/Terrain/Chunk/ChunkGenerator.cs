using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using SE;
using UnityEngine;

namespace DT {
public static class ChunkGenerator {
	public static ChunkProcessOutput CreateChunk(ChunkProcessInput input) {
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
		//FixNormals(m);
		output.mesh = m;
		s.Stop();	
		output.processingTime = s.ElapsedMilliseconds;
		return output;
	}

	public static List<Vector4> SetMeshUVs(SE.Mesh m) {
		System.Random RNG = new System.Random();

		if(m.triangles.Length != m.vertices.Length) {
			UnityEngine.Debug.LogError("Terrain mesh has an unequal number of triangle indices and vertices");
		}

		List<Vector4> UV1s = new List<Vector4>(m.vertices.Length);
		float intensity;

		for(int i = 0; i < m.vertices.Length; i += 3) {
			for(int j = 0; j < 3; j++) {
				intensity = 1f - Mathf.Clamp(Mathf.Pow(m.normals[i + j].y, 3), 0f, 1f);
				intensity = (float)RNG.Next(100) / 100f;
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
		float z = 0.03f;
		float result = 10.0f - position.y;
		result += (float)noise.Evaluate(position.x*r, position.y*r, position.z*r) * 0.1f;
		result += (float)noise.Evaluate(position.x*z, position.y*z, position.z*z) * 20f;
		return result;
	}
}
}