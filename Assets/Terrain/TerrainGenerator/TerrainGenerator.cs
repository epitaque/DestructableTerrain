using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;
using DT;

public class TerrainGenerator : MonoBehaviour {
	public GameObject ChunkPrefab;
	public GameObject MaterialsList;
	public GameObject Player;

	private Texture2DArray MaterialsArray;

	public int resolution = 16;
	public int radius = 2;
	public static SE.OpenSimplexNoise gen;

	public bool multiThreaded = true;
	// key: toString of min vector, value: Chunk
	private Hashtable Chunks = new Hashtable();
	private Vector3 startPos;
	private long seed;
	private MultiThreadedChunkGenerator threadedChunkGenerator;

	// Use this for initialization
	void Start () {
		seed = DateTime.Now.Ticks;
		gen = new SE.OpenSimplexNoise(seed);
		threadedChunkGenerator = new MultiThreadedChunkGenerator(4);
		float r = 0.3f;
		MaterialsArray = MaterialsList.GetComponent<MaterialsList>().GetArray();
		startPos = Vector3.zero;
		ManageChunks();
	}
	
	// Ensures there are chunks within set radius of player
	void ManageChunks() {
		print("ManageChunks called");
		this.gameObject.transform.position = Vector3.zero;

		float updateTime = Time.realtimeSinceStartup;

		int playerX = (int)(Mathf.Floor(Player.transform.position.x/resolution)*resolution);
		int playerZ = (int)(Mathf.Floor(Player.transform.position.z/resolution)*resolution);

		int hres = resolution/2;

		int i = 0;
		for(int x = -radius - 1; x <= radius + 1; x++) {
			for(int z = -radius - 1; z <= radius + 1; z++) {
				i++;
				Vector3 pos = new Vector3(((x * resolution) + playerX), 0, ((z * resolution) + playerZ));
				if(Vector3.Distance(new Vector3(pos.x + hres, 0, pos.z + hres), new Vector3(playerX + resolution, 0, playerZ + resolution)) > radius * resolution) continue;
				string key = pos.ToString();

				if(!Chunks.ContainsKey(key)) {
					CreateChunk((int)pos.x - resolution / 2, (int)pos.y, (int)pos.z - resolution / 2, key);
				}
				(Chunks[key] as Chunk).CreationTime = updateTime;
			}
		}

		Hashtable newTerrain = new Hashtable();
		foreach(Chunk c in Chunks.Values) {
			if(c.CreationTime != updateTime) {
				Destroy(c.Object);
			}
			else {
				newTerrain.Add(c.Key, c);
			}
		}
		Chunks = newTerrain;
		startPos = Player.transform.position;
	}

	void CheckIfNeedManageChunks() {
		float xMove = Player.transform.position.x - startPos.x;
		float zMove = Player.transform.position.z - startPos.z;

		if(Mathf.Abs(xMove) >= resolution || Mathf.Abs(zMove) >= resolution) {
			ManageChunks();
		}
	}

	private float maxZSampled = float.MinValue;

	void CreateChunk(int x, int y, int z, string key) {
		Chunk newChunk = new Chunk();
		newChunk.Size = resolution;
		newChunk.Min = new Vector3(x, y, z);
		newChunk.Object = InstantiateChunkGameObject(newChunk.Min);
		newChunk.Key = key;
		Chunks.Add(newChunk.Key, newChunk);

		if(multiThreaded) {
			ChunkProcessInput input = new ChunkProcessInput();
			input.key = key;
			input.seed = seed;
			input.min = newChunk.Min;
			input.size = resolution;
			threadedChunkGenerator.QueueChunk(input);
		}
		else {
			Stopwatch s = new Stopwatch();
			s.Start();

			BuildChunk(newChunk);
			s.Stop();
			print("creating chunk at " + newChunk.Min + " took " + s.ElapsedMilliseconds + "ms");
		}

	}

	// updates chunk's gameobject based on its densitychunk
	void BuildChunk(Chunk chunk) {
 		SE.Mesh m = SE.MarchingCubes.March(0f, chunk.Densities);
		UpdateChunk(chunk.Object, m);
	}

	// update chunk's mesh
	void UpdateChunk(GameObject chunkObject, SE.Mesh m) {
		MeshFilter mf = chunkObject.GetComponent<MeshFilter>();
		MeshCollider mc = chunkObject.GetComponent<MeshCollider>();

		mf.mesh.Clear();

		mf.mesh.vertices = m.vertices;
		mf.mesh.triangles = m.triangles;
		if(m.normals != null) mf.mesh.normals = m.normals;
		else mf.mesh.RecalculateNormals();
		mc.sharedMesh = mf.mesh;
		mf.mesh.RecalculateBounds();
	}

	void InitializeChunkGameObject(GameObject gameObject) {
		Material mat = gameObject.GetComponent<Renderer>().materials[0];

		mat.SetTexture("_Materials", MaterialsArray);
		mat.SetFloat("_Resolution", resolution);
	}

	private GameObject InstantiateChunkGameObject(Vector3 offset) {
		print("InstantiateChunkGameObject called");

		GameObject gameObject = Instantiate(ChunkPrefab, offset, Quaternion.identity);
		gameObject.transform.SetParent(transform);

		InitializeChunkGameObject(gameObject);

		return gameObject;
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)) {
			deformTerrain(false);
		}
		if (Input.GetMouseButton(1)) {
			deformTerrain(true);
		}
		CheckIfChunksLoaded();
		CheckIfNeedManageChunks();
		threadedChunkGenerator.Update();
	}

	void CheckIfChunksLoaded() {
		if(threadedChunkGenerator.loadedChunks.Count > 0) {
			ChunkProcessOutput c = (ChunkProcessOutput)threadedChunkGenerator.loadedChunks.Dequeue(); // at most 1 per frame
			ProcessChunkOutput(c);
		}

	}

	void ProcessChunkOutput(ChunkProcessOutput output) {
		if(Chunks.Contains(output.key)) {
			print("Chunk generation time: " + output.processingTime);
			Chunk chunk = (Chunk)Chunks[output.key];
			chunk.Densities = output.densities;
			UpdateChunk(chunk.Object, output.mesh);
		}
	}

	void deformTerrain(bool additive) {
		int deformRadius = 2;
		float hardness = 0.5f;

        RaycastHit hit;
		int screenX = (Screen.width / 2);
		int screenY = (Screen.height / 2);
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenX, screenY, 0));

        if (!Physics.Raycast(ray, out hit, 500)) {
            return;
		}
		
		// generate list of affected chunks

		Vector3 hitPos = hit.point;

		Vector3 minPosition = hitPos - (float)deformRadius * Vector3.one;
		Vector3 maxPosition = hitPos + (float)deformRadius * Vector3.one;

		Vector3 flooredMinPos = new Vector3(Mathf.Floor(minPosition.x/resolution)*resolution, 
			Mathf.Floor(minPosition.y/resolution)*resolution, 
			Mathf.Floor(minPosition.z/resolution)*resolution);

		Vector3 ceiledMaxPos = new Vector3(Mathf.Ceil(minPosition.x/resolution)*resolution, 
			Mathf.Ceil(minPosition.y/resolution)*resolution, 
			Mathf.Ceil(minPosition.z/resolution)*resolution);

		Vector3 currentPosition;
		List<string> affectedChunks = new List<string>();

		for(int x = (int)flooredMinPos.x; x <= ceiledMaxPos.x; x += resolution) {
			for(int y = (int)flooredMinPos.y; y <= ceiledMaxPos.y; y += resolution) {
				for(int z = (int)flooredMinPos.z; z <= ceiledMaxPos.z; z+= resolution) {
					currentPosition = new Vector3(x, y, z);
					affectedChunks.Add(currentPosition.ToString());
				}
			}
		}

		foreach(string key in affectedChunks) {
			if(Chunks.Contains(key)) {
				TerrainDeformer.SphericalDeform(hit.point, (Chunk)Chunks[key], 0, deformRadius, hardness, additive);
				BuildChunk((Chunk)Chunks[key]);
			}
		}
	}
}
