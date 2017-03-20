using UnityEngine;

public class MaterialsList : MonoBehaviour {
	public Texture2D Grass;
	public Texture2D Dirt;
	public Texture2D Wood;

	public Texture2DArray GetArray() {
		Texture2DArray arr = new Texture2DArray(1024, 1024, 3, TextureFormat.RGBA32, true);

		Debug.Log("Grass width:" + Grass.width);
		Debug.Log("Arr width:" + arr.width);

		for(int i = 0; i < Grass.mipmapCount; i++) {
			Graphics.CopyTexture(Grass, 0, i, arr, 0, i);
			Graphics.CopyTexture(Dirt, 0, i, arr, 1, i);
			Graphics.CopyTexture(Wood, 0, i, arr, 2, i);
		}



		return arr;

	}
}
