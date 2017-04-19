using System.Collections.Generic;
using UnityEngine;

namespace SE {

public static class Utilities {
		public static void FixNormals(SE.Mesh m) {
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
}
	
}