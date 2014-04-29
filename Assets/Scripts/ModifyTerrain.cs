using UnityEngine;
using System;
using System.Collections;

public class ModifyTerrain : MonoBehaviour {
	private World world;
	private GameObject cameraGO;
	public GameObject playerGO;
	public int distToLoad = 64;
	public int distToUnload = 96;
	public bool showInfo=false;
	public BlockInfo lookingAt = null;
	public BlockInfo lastLookingAt = null;
	// Use this for initialization
	void Start () {
		world=gameObject.GetComponent("World") as World;
		cameraGO=GameObject.FindGameObjectWithTag("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
		
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
		if(Input.GetMouseButtonDown(0)){
			Debug.DrawRay(ray.origin,ray.direction,Color.red,1);
			ReplaceBlockCursor(0);
		}
		
		if(Input.GetMouseButtonDown(1)){
			Debug.DrawRay(ray.origin,ray.direction,Color.green,1);
			AddBlockCursor(1);
		}
		
		if(Input.GetMouseButtonDown(2)){
			Debug.DrawRay(ray.origin,ray.direction,Color.green,1);
			AddBlockCursor(2);
		}
		if (Input.GetKeyDown(KeyCode.F3))
		{
			showInfo=!showInfo;
		}
		LoadChunks(playerGO.transform.position,distToLoad,distToUnload);
		if (showInfo)
		{
			lookingAt = GetBlockCursor(1000);
			if (lookingAt!=null)
			{
				lookingAt.Chunk.highlight=true;
			}
		}
		
		if (lastLookingAt!=null && (lookingAt==null || lastLookingAt.Chunk.chunkId!=lookingAt.Chunk.chunkId))
		{
			lastLookingAt.Chunk.highlight=false;
		}
		lastLookingAt = lookingAt;
    }
    public void ReplaceBlockCenter(float range, byte block){
		//Replaces the block directly in front of the player
		
		Ray ray = new Ray(cameraGO.transform.position, cameraGO.transform.forward);
		RaycastHit hit;
		
		Debug.DrawRay(ray.origin,ray.direction,Color.yellow);
		if (Physics.Raycast (ray, out hit)) {
			
			if(hit.distance<range){
				ReplaceBlockAt(hit, block);
			}
		}
		
	}
	public BlockInfo GetBlockCursor(int range){
		//Replaces the block directly in front of the player
		
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		Debug.DrawRay(ray.origin,ray.direction,Color.white);
		if (Physics.Raycast (ray, out hit)) {
			
			if(hit.distance<range){
				return GetBlockAt(hit.point);
			}
		}
		return null;
		
	}
	
	public void AddBlockCenter(float range, byte block){
		//Adds the block specified directly in front of the player
		
		Ray ray = new Ray(cameraGO.transform.position, cameraGO.transform.forward);
		RaycastHit hit;
		
		Debug.DrawRay(ray.origin,ray.direction,Color.yellow);
		if (Physics.Raycast (ray, out hit)) {
			
			if(hit.distance<range){
				AddBlockAt(hit,block);
			}
			Debug.DrawLine(ray.origin,ray.origin+( ray.direction*hit.distance),Color.green,2);
		}
		
	}
	
	public void ReplaceBlockCursor(byte block){
		//Replaces the block specified where the mouse cursor is pointing
		
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		Debug.DrawRay(ray.origin,ray.direction,Color.yellow);
		if (Physics.Raycast (ray, out hit)) {
			
			ReplaceBlockAt(hit, block);
			Debug.DrawLine(ray.origin,ray.origin+( ray.direction*hit.distance),
			               Color.green,2);
			
		}
		
	}
	
	public void AddBlockCursor( byte block){
		//Adds the block specified where the mouse cursor is pointing
		
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit,1000)) {
			Debug.DrawLine(ray.origin, hit.point, Color.green);
			AddBlockAt(hit, block);
		}
		else
		{
			Debug.DrawRay(ray.origin,ray.direction,Color.white,3, false);
		}
		
	}
	
	public void ReplaceBlockAt(RaycastHit hit, byte block) {
		//removes a block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position+=(hit.normal*-0.5f);
		
		SetBlockAt(position, block);
	}
	public BlockInfo GetBlockAt(RaycastHit hit) {
		//removes a block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position+=(hit.normal*-0.5f);
		
		return GetBlockAt(position);
	}
	
	public void AddBlockAt(RaycastHit hit, byte block) {
		//adds the specified block at these impact coordinates, you can raycast against the terrain and call this with the hit.point
		Vector3 position = hit.point;
		position+=(hit.normal*0.5f);
		
		SetBlockAt(position,block);
		
	}
	
	public BlockInfo GetBlockAt(Vector3 position) {
		//sets the specified block at these coordinates
		
		int x= Mathf.RoundToInt( position.x );
		int y= Mathf.RoundToInt( position.y );
		int z= Mathf.RoundToInt( position.z );
		
		return GetBlockAt(x,y,z);
	}
	
	public BlockInfo GetBlockAt(int x, int y, int z) {
		//adds the specified block at these coordinates
		byte block = world.data[x,y,z];
		Chunk chunk = GetChunkAt(x,y,z);
		return new BlockInfo()
		{
			X=x, 
			Y=y,
			Z=z,
			Block=block, 
			Chunk=chunk
		};
		
	}
		
		
	public void SetBlockAt(Vector3 position, byte block) {
		//sets the specified block at these coordinates
		
		int x= Mathf.RoundToInt( position.x );
		int y= Mathf.RoundToInt( position.y );
		int z= Mathf.RoundToInt( position.z );

		
		SetBlockAt(x,y,z,block);
	}
	
	public void SetBlockAt(int x, int y, int z, byte block) {
		//adds the specified block at these coordinates
		
		print("Adding: " + x + ", " + y + ", " + z);
		
		
		world.data[x,y,z]=block;
		UpdateChunkAt(x,y,z);
		
	}
	private Chunk GetChunkAt(int x, int y, int z)
	{
		int updateX= Mathf.FloorToInt( x/world.chunkSize);
		int updateY= Mathf.FloorToInt( y/world.chunkSize);
		int updateZ= Mathf.FloorToInt( z/world.chunkSize);
		return world.chunks[updateX,updateY, updateZ];
	}
	//To do: add a way to just flag the chunk for update then it update it in lateupdate
	public void UpdateChunkAt(int x, int y, int z){ 
		//Updates the chunk containing this block
		
		int updateX= Mathf.FloorToInt( x/world.chunkSize);
		int updateY= Mathf.FloorToInt( y/world.chunkSize);
		int updateZ= Mathf.FloorToInt( z/world.chunkSize);
		
		print("Updating: " + updateX + ", " + updateY + ", " + updateZ);
	  
		world.chunks[updateX,updateY, updateZ].update = true;
		if(x-(world.chunkSize*updateX)==0 && updateX!=0){
			world.chunks[updateX-1,updateY, updateZ].update=true;
		}
		
		if(x-(world.chunkSize*updateX)==15 && updateX!=world.chunks.GetLength(0)-1){
			world.chunks[updateX+1,updateY, updateZ].update=true;
		}
		
		if(y-(world.chunkSize*updateY)==0 && updateY!=0){
			world.chunks[updateX,updateY-1, updateZ].update=true;
		}
		
		if(y-(world.chunkSize*updateY)==15 && updateY!=world.chunks.GetLength(1)-1){
			world.chunks[updateX,updateY+1, updateZ].update=true;
		}
		
		if(z-(world.chunkSize*updateZ)==0 && updateZ!=0){
			world.chunks[updateX,updateY, updateZ-1].update=true;
		}
		
		if(z-(world.chunkSize*updateZ)==15 && updateZ!=world.chunks.GetLength(2)-1){
			world.chunks[updateX,updateY, updateZ+1].update=true;
		}
	  
	}
	public void LoadChunks(Vector3 playerPos, float distToLoad, float distToUnload){
		
		
		for(int x=0;x<world.chunks.GetLength(0);x++){
			for(int z=0;z<world.chunks.GetLength(2);z++){
				
				float dist=Vector2.Distance(new Vector2(x*world.chunkSize,
				                                        z*world.chunkSize),new Vector2(playerPos.x,playerPos.z));
				Debug.Log(string.Format("X:{0}. Y:{1}. Z:{2}.",x,0,z));
				if(dist<distToLoad){
					if(world.chunks[x,0,z]==null){
						world.LoadColumn(x,z);
					}
				} else if(dist>distToUnload){
					if(world.chunks[x,0,z]!=null){
						
						world.UnloadColumn(x,z);
					}
				}
				
			}
		}
		
	}
	void OnGUI()
	{
		if (showInfo && lookingAt!=null)
		{
			GUI.Label(new Rect(0,0,120,20),string.Format("P:{0},{1},{2}",lookingAt.X, lookingAt.Y, lookingAt.Z));
			GUI.Label(new Rect(0,20,120,20),string.Format("C:{0} / ({1},{2},{3}) {4}",lookingAt.Chunk.chunkId,lookingAt.Chunk.chunkX, lookingAt.Chunk.chunkY, lookingAt.Chunk.chunkZ, lookingAt.Chunk.highlight ? "H":""));
			GUI.Label(new Rect(0,40,120,20),string.Format("B:{0}",lookingAt.Block));
		}
	}
}
