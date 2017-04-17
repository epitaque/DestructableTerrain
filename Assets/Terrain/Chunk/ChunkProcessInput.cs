using UnityEngine;

namespace DT {
public struct ChunkProcessInput {
	public int size;
	public int threadId;
	public long seed;
	public string key;
	public Vector3 min;
}
}
