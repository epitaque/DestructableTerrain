using UnityEngine;

namespace SE {
	public delegate float Sample(Vector3 position);

	public class Mesh {
		public Vector3[] vertices;
		public Vector3[] normals;
		public int[] triangles;
	}
	public struct GridCell {
		public Point[] points;
	}

	public class Point {
		public Vector3 position;
		public float density;
	}

}