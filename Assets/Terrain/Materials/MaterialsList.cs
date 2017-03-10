using UnityEngine;

public class MaterialsList : MonoBehaviour {
	public Texture2D Grass;
	public Texture2D Dirt;

	public Texture2DArray GetArray() {
		Texture2DArray arr = new Texture2DArray(1024, 1024, 2, TextureFormat.RGBA32, false);

		Debug.Log("Grass width:" + Grass.width);
		Debug.Log("Arr width:" + arr.width);

		Graphics.CopyTexture(Grass, 0, 0, arr, 0, 0);
		Graphics.CopyTexture(Dirt, 0, 0, arr, 1, 0);

		return arr;

	}
}
