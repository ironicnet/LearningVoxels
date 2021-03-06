﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolygonGenerator : MonoBehaviour {
	// This first list contains every vertex of the mesh that we are going to render
	public List<Vector3> newVertices = new List<Vector3>();
	
	// The triangles tell Unity how to build each section of the mesh joining
	// the vertices
	public List<int> newTriangles = new List<int>();
	
	// The UV list is unimportant right now but it tells Unity how the texture is
	// aligned on each polygon
	public List<Vector2> newUV = new List<Vector2>();
	public byte[,] blocks;
	public List<Vector3> colVertices = new List<Vector3>();
	public List<int> colTriangles = new List<int>();
	public bool update;

	private float tUnit = 0.25f;
	private Vector2 tStone = new Vector2 (0, 0);
	private Vector2 tGrass = new Vector2 (0, 1);
	private int squareCount;
	private int colCount;
	
	
	// A mesh is made up of the vertices, triangles and UVs we are going to define,
	// after we make them up we'll save them as this mesh
	private Mesh mesh;
	private MeshCollider col;
	// Use this for initialization
	void Start () {
		//Obtengo el mesh
		mesh = GetComponent<MeshFilter> ().mesh;
		col = GetComponent<MeshCollider> ();
		
		float x = transform.position.x;
		float y = transform.position.y;
		float z = transform.position.z;
		
		
	
		
		GenTerrain();
		BuildMesh();
		UpdateMesh ();
	}

	void GenSquare (float x, float y, float z, Vector2 texture)
	{
		newVertices.Add (new Vector3 (x, y, z));
		newVertices.Add (new Vector3 (x + 1, y, z));
		newVertices.Add (new Vector3 (x + 1, y - 1, z));
		newVertices.Add (new Vector3 (x, y - 1, z));

		newTriangles.Add(squareCount*4);
		newTriangles.Add((squareCount*4)+1);
		newTriangles.Add((squareCount*4)+3);
		newTriangles.Add((squareCount*4)+1);
		newTriangles.Add((squareCount*4)+2);
		newTriangles.Add((squareCount*4)+3);
		newUV.Add (new Vector2 (tUnit * texture.x, tUnit * texture.y + tUnit));
		newUV.Add (new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y + tUnit));
		newUV.Add (new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
		newUV.Add (new Vector2 (tUnit * texture.x, tUnit * texture.y));
		squareCount++;
	}
	void GenTerrain(){
		blocks=new byte[96,128];
		int baseVal = 75;
		for(int px=0;px<blocks.GetLength(0);px++){
			int stone=0;
			stone += Noise(px,0, 80,15,1);
			stone+= Noise(px,0, 50,30,1);
			stone+= Noise(px,0, 10,10,1);
			stone+=baseVal;
			
			int dirt =0;
			dirt += Noise(px,0, 100,35,1);
			dirt+= Noise(px,0, 50,30,1);
			dirt+=baseVal;
			for(int py=0;py<blocks.GetLength(1);py++){
				if(py<stone){
					blocks[px, py]=1;
					
					//The next three lines make dirt spots in random places
					if(Noise(px,py,12,16,1)>10){
						blocks[px,py]=2;
						
					}
					
					//The next three lines remove dirt and rock to make caves in certain places
					if(Noise(px,py*2,16,14,1)>10){ //Caves
						blocks[px,py]=0;
						
					}
					
				} else if(py<dirt) {
					blocks[px,py]=2;
				}
				
				
			}
		}
	}
	int Noise (int x, int y, float scale, float mag, float exp){
		
		return (int) (Mathf.Pow ((Mathf.PerlinNoise(x/scale,y/scale)*mag),(exp) )); 
		
	}
	void GenCollider(int x, int y, int z){
		
		//Top
		if(Block(x,y+1)==0){
		colVertices.Add( new Vector3 (x  , y  , z + 1));
		colVertices.Add( new Vector3 (x + 1 , y  , z + 1));
		colVertices.Add( new Vector3 (x + 1 , y  , z ));
		colVertices.Add( new Vector3 (x  , y  , z ));
		
		ColliderTriangles();
		
		colCount++;
		}
		
		//bot
		if(Block(x,y-1)==0){
		colVertices.Add( new Vector3 (x  , y -1 , z));
		colVertices.Add( new Vector3 (x + 1 , y -1 , z));
		colVertices.Add( new Vector3 (x + 1 , y -1 , z + 1 ));
		colVertices.Add( new Vector3 (x  , y -1 , z + 1 ));
		
		ColliderTriangles();
		colCount++;
		}
		
		//left
		if(Block(x-1,y)==0){
		colVertices.Add( new Vector3 (x  , y -1 , z + 1));
		colVertices.Add( new Vector3 (x  , y  , z + 1));
		colVertices.Add( new Vector3 (x  , y  , z ));
		colVertices.Add( new Vector3 (x  , y -1 , z ));
		
		ColliderTriangles();
		
		colCount++;
		}
		
		//right
		if(Block(x+1,y)==0){
		colVertices.Add( new Vector3 (x +1 , y  , z + 1));
		colVertices.Add( new Vector3 (x +1 , y -1 , z + 1));
		colVertices.Add( new Vector3 (x +1 , y -1 , z ));
		colVertices.Add( new Vector3 (x +1 , y  , z ));
		
		ColliderTriangles();
		
		colCount++;
		}
	}
	byte Block (int x, int y){
		
		if(x==-1 || x==blocks.GetLength(0) ||   y==-1 || y==blocks.GetLength(1)){
			return (byte)1;
		}
		
		return blocks[x,y];
	}

	void ColliderTriangles ()
	{
		colTriangles.Add (colCount * 4);
		colTriangles.Add ((colCount * 4) + 1);
		colTriangles.Add ((colCount * 4) + 3);
		colTriangles.Add ((colCount * 4) + 1);
		colTriangles.Add ((colCount * 4) + 2);
		colTriangles.Add ((colCount * 4) + 3);
	}

	void BuildMesh(){
		for(int px=0;px<blocks.GetLength(0);px++){
			for(int py=0;py<blocks.GetLength(1);py++){
				
				//If the block is not air
				if(blocks[px,py]!=0){
					// GenCollider here, this will apply it
					// to every block other than air
					GenCollider(px,py,0);
					
					if(blocks[px,py]==1){
						GenSquare(px,py,0,tStone);
					} else if(blocks[px,py]==2){
						GenSquare(px,py,0,tGrass);
					}
				}
				
			}
		}
	}
	void UpdateMesh ()
	{
		Mesh newMesh = new Mesh();
		newMesh.vertices = colVertices.ToArray();
		newMesh.triangles = colTriangles.ToArray();
		col.sharedMesh= newMesh;
		
		colVertices.Clear();
		colTriangles.Clear();
		colCount=0;

		mesh.Clear ();
		mesh.vertices = newVertices.ToArray ();
		mesh.triangles = newTriangles.ToArray ();
		mesh.uv = newUV.ToArray ();
		mesh.Optimize ();
		mesh.RecalculateNormals ();
		squareCount=0;
		newVertices.Clear();
		newTriangles.Clear();
		newUV.Clear();
	}
	
	// Update is called once per frame
	void Update(){
		if(update){
			BuildMesh();
			UpdateMesh();
            update=false;
        }
	}
}
