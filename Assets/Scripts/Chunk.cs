using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chunk : MonoBehaviour {
	
	public GameObject worldGO;
	public int chunkSize=16;
	public int chunkX;
	public int chunkY;
	public int chunkZ;
	public int chunkId;
	public bool update;
	public bool highlight;
	
	private World world;
	private List<Vector3> newVertices = new List<Vector3>();
	private List<int> newTriangles = new List<int>();
	private List<Vector2> newUV = new List<Vector2>();
	
	private float tUnit = 0.25f;
	private Vector2 tAir = new Vector2 (3, 0);
	private Vector2 tStone = new Vector2 (3, 2);
	private Vector2 tGrass = new Vector2 (1, 1);
	private Vector2 tGrassTop = new Vector2 (0, 2);
	
	private Mesh mesh;
	private MeshCollider col;
	private float cubeSize = 1f;
	
	private int faceCount;
	// Use this for initialization
	void Start () {
		world=worldGO.GetComponent("World") as World;
		mesh = GetComponent<MeshFilter> ().mesh;
		col = GetComponent<MeshCollider> ();
		col.sharedMesh=null;
		col.sharedMesh=mesh;
		
		GenerateMesh();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	byte Block(int x, int y, int z){
		return world.Block(x+chunkX,y+chunkY,z+chunkZ);
	}
	bool IsSolid(int x, int y, int z){
		return world.Block(x+chunkX,y+chunkY,z+chunkZ)!=0;
	}

	Vector2 DecideTexture (int x, int y, int z, bool isTop)
	{
		Vector2 texturePos = new Vector2 (0, 0);
		if (Block (x,y,z) ==0)
		{
			texturePos = tAir;
		}
		if (Block (x, y, z) == 1) {
			texturePos = tStone;
		}
		else if (Block (x, y, z) == 2) {
			texturePos = isTop ? tGrassTop :tGrass;
		}
		return texturePos;
	}

	void CubeTop (int x, int y, int z, byte block) {
		newVertices.Add(new Vector3 (x,  y,  z + cubeSize));
		newVertices.Add(new Vector3 (x + cubeSize, y,  z + cubeSize));
		newVertices.Add(new Vector3 (x + cubeSize, y,  z ));
		newVertices.Add(new Vector3 (x,  y,  z ));
		
		var texturePos = DecideTexture (x, y, z, true);
		Cube (texturePos);
	}
	void Cube (Vector2 texturePos) {
		
		newTriangles.Add(faceCount * 4  ); //1
		newTriangles.Add(faceCount * 4 + 1 ); //2
		newTriangles.Add(faceCount * 4 + 2 ); //3
		newTriangles.Add(faceCount * 4  ); //1
		newTriangles.Add(faceCount * 4 + 2 ); //3
		newTriangles.Add(faceCount * 4 + 3 ); //4
		
		newUV.Add(new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y));
		newUV.Add(new Vector2 (tUnit * texturePos.x + tUnit, tUnit * texturePos.y + tUnit));
		newUV.Add(new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y + tUnit));
		newUV.Add(new Vector2 (tUnit * texturePos.x, tUnit * texturePos.y));
		
		faceCount++; // Add this line
	}
	
	void CubeNorth (int x, int y, int z, byte block) {
		newVertices.Add(new Vector3 (x + cubeSize, y-cubeSize, z + cubeSize));
		newVertices.Add(new Vector3 (x + cubeSize, y, z + cubeSize));
		newVertices.Add(new Vector3 (x, y, z + cubeSize));
		newVertices.Add(new Vector3 (x, y-cubeSize, z + cubeSize));
		
		var texturePos = DecideTexture (x, y, z, false);
		
		Cube (texturePos);
	}
	void CubeEast (int x, int y, int z, byte block) {
		newVertices.Add(new Vector3 (x + cubeSize, y - cubeSize, z));
		newVertices.Add(new Vector3 (x + cubeSize, y, z));
		newVertices.Add(new Vector3 (x + cubeSize, y, z + cubeSize));
		newVertices.Add(new Vector3 (x + cubeSize, y - cubeSize, z + cubeSize));
		
		var texturePos = DecideTexture (x, y, z, false);
		
		Cube (texturePos);
	}
	void CubeSouth (int x, int y, int z, byte block) {
		newVertices.Add(new Vector3 (x, y - cubeSize, z));
		newVertices.Add(new Vector3 (x, y, z));
		newVertices.Add(new Vector3 (x + cubeSize, y, z));
		newVertices.Add(new Vector3 (x + cubeSize, y - cubeSize, z));
		
		var texturePos = DecideTexture (x, y, z, false);
		
		Cube (texturePos);
	}
	void CubeWest (int x, int y, int z, byte block) {
		newVertices.Add(new Vector3 (x, y- cubeSize, z + cubeSize));
		newVertices.Add(new Vector3 (x, y, z + cubeSize));
		newVertices.Add(new Vector3 (x, y, z));
		newVertices.Add(new Vector3 (x, y - cubeSize, z));
		
		var texturePos = DecideTexture (x, y, z, false);
		
		Cube (texturePos);
	}
	void CubeBot (int x, int y, int z, byte block) {
		newVertices.Add(new Vector3 (x,  y-cubeSize,  z ));
		newVertices.Add(new Vector3 (x + cubeSize, y-cubeSize,  z ));
		newVertices.Add(new Vector3 (x + cubeSize, y-cubeSize,  z + cubeSize));
		newVertices.Add(new Vector3 (x,  y-cubeSize,  z + cubeSize));
		
		var texturePos = DecideTexture (x, y, z, false);
		
		Cube (texturePos);
	}
	public void GenerateMesh(){
		
		for (int x=0; x<chunkSize; x++){
			for (int y=0; y<chunkSize; y++){
				for (int z=0; z<chunkSize; z++){
					//This code will run for every block in the chunk
					
					if(IsSolid(x,y,z)){
						//If the block is solid
						
						if(!IsSolid(x,y+1,z)){
							//Block above is air
							CubeTop(x,y,z,Block(x,y,z));
						}
						
						if(!IsSolid(x,y-1,z)){
							//Block below is air
							CubeBot(x,y,z,Block(x,y,z));
							
						}
						
						if(!IsSolid(x+1,y,z)){
							//Block east is air
							CubeEast(x,y,z,Block(x,y,z));
							
						}
						
						if(!IsSolid(x-1,y,z)){
							//Block west is air
							CubeWest(x,y,z,Block(x,y,z));
							
						}
						
						if(!IsSolid(x,y,z+1)){
							//Block north is air
							CubeNorth(x,y,z,Block(x,y,z));
							
						}
						
						if(!IsSolid(x,y,z-1)){
							//Block south is air
							CubeSouth(x,y,z,Block(x,y,z));
							
						}
						
					}
					
				}
			}
		}
		
		UpdateMesh ();
	}
	void LateUpdate () {
		if(update){
			GenerateMesh();
			update=false;
        }
    }
	void UpdateMesh ()
	{
		
		mesh.Clear ();
		mesh.vertices = newVertices.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.Optimize ();
		mesh.RecalculateNormals ();
		
		col.sharedMesh=null;
		col.sharedMesh=mesh;
		
		newVertices.Clear();
		newUV.Clear();
		newTriangles.Clear();
		
		faceCount=0; //Fixed: Added this thanks to a bug pointed out by ratnushock!
		
	}
	void OnDrawGizmos()
	{
		if (highlight)
		{
		Gizmos.color = highlight ? Color.green : Color.white;
		Gizmos.DrawWireCube(new Vector3(transform.position.x+chunkSize/2, 
		                                transform.position.y+chunkSize/2, 
										transform.position.z+chunkSize/2)
							,new Vector3(chunkSize,chunkSize,chunkSize));	
		}
	}
}
