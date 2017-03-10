using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
	public GameObject ChunkPrefab;
	public GameObject MaterialsList;

	private Texture2DArray MaterialsArray;

	public int resolution = 16;
	public static SE.OpenSimplexNoise gen = new SE.OpenSimplexNoise();

	// Use this for initialization
	void Start () {
		float r = 0.3f;
		MaterialsArray = MaterialsList.GetComponent<MaterialsList>().GetArray();
		CreateChunk(0, 0, 0, (Vector3 position) => {
			return 10.0f - position.y + 
			(float)gen.Evaluate((float)position.x*r, (float)position.y*r, (float)position.z*r) * 3; });
	}
	
	void CreateChunk(int x, int y, int z, SE.Sample fn) {
		Vector3 offset = new Vector3(x, y, z);
		DT.Chunk c = new DT.Chunk(resolution, (Vector3 position) => { return fn(position + offset); });
		SE.Mesh m = SE.MarchingCubes.March(resolution, 0f, c.Density, true);
		SetMesh(m, offset);
	}

	private void SetMesh(SE.Mesh m, Vector3 offset) {
		Debug.Log("SetMesh called");
		GameObject isosurfaceMesh = Instantiate(ChunkPrefab, offset, Quaternion.identity);

		Material mat = isosurfaceMesh.GetComponent<Renderer>().materials[0];
		MeshFilter mf = isosurfaceMesh.GetComponent<MeshFilter>();
		MeshCollider mc = isosurfaceMesh.GetComponent<MeshCollider>();

		//mat.SetTexture("_MaterialGrid", materialGrid);
		mat.SetTexture("_Materials", MaterialsArray);
		mat.SetFloat("_Resolution", resolution);

		mf.mesh.vertices = m.vertices;
		mf.mesh.triangles = m.triangles;
		mf.mesh.uv = getUVs(m);
		mf.mesh.colors = getColors(m);
		mc.sharedMesh = mf.mesh;
		if(m.normals != null) mf.mesh.normals = m.normals;
		else mf.mesh.RecalculateNormals();
		mf.mesh.RecalculateBounds();
	}

	private Vector2[] getUVs(SE.Mesh m) {
		List<Vector2> uvs = new List<Vector2>();
		for(int i = 0;  i < m.vertices.Length; i += 3) {
			uvs.Add(new Vector2(-1, -1));
			uvs.Add(new Vector2(0, 1));
			uvs.Add(new Vector2(1, -1));
		}
		return uvs.ToArray();
	}

	private Color[] getColors(SE.Mesh m) {
		List<Color> colors = new List<Color>();
		for(int i = 0;  i < m.vertices.Length; i+=3) {
			int[] mats = 

			colors.Add(new Color(1f, 0f, 0f))
		}
		return colors.ToArray();
	}

	// Update is called once per frame
	void Update () {
		
	}
}
