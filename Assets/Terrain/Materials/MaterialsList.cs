using UnityEngine;

public class MaterialsList : MonoBehaviour {
	public Texture2D GrassAlbedo;
	public Texture2D GrassAO;
	public Texture2D GrassHeight;
	public Texture2D GrassMetalness;
	public Texture2D GrassNormal;

	public Texture2D Dirt;
	public Texture2D Wood;


	public Texture2DArray Albedos;
	public Texture2DArray AOs;
	public Texture2DArray Heights;
	public Texture2DArray Metalnesses;
	public Texture2DArray Normals;



	public Texture2D TestTexture;


	public void Start() {
		Albedos = new Texture2DArray(2048, 2048, 3, TextureFormat.RGBA32, true);
		AOs = new Texture2DArray(2048, 2048, 3, TextureFormat.RGBA32, true);
		Heights = new Texture2DArray(2048, 2048, 3, TextureFormat.RGBA32, true);
		Metalnesses = new Texture2DArray(2048, 2048, 3, TextureFormat.RGBA32, true);
		Normals = new Texture2DArray(2048, 2048, 3, TextureFormat.RGBA32, true);

		int mips = GrassAlbedo.mipmapCount;

		for(int i = 0; i < mips; i++) {
			Graphics.CopyTexture(UncompressTexture(GrassAlbedo), 0, i, Albedos, 0, i);
			Graphics.CopyTexture(UncompressTexture(GrassAO), 0, i, Albedos, 0, i);
			Graphics.CopyTexture(UncompressTexture(GrassHeight), 0, i, Albedos, 0, i);
			Graphics.CopyTexture(UncompressTexture(GrassMetalness), 0, i, Albedos, 0, i);
			Graphics.CopyTexture(UncompressTexture(GrassNormal), 0, i, Albedos, 0, i);
		}

		TestTexture = new Texture2D(2048, 2048);
		//Graphics.CopyTexture(Albedos, )
	}

	public Texture2D UncompressTexture(Texture2D Compressed) {
		Texture2D Uncompressed = new Texture2D(Compressed.width, Compressed.height);

		Uncompressed.SetPixels(Compressed.GetPixels());

		return Uncompressed;
	}
}
