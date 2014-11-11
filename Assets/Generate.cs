using UnityEngine;
using System.Collections;
using QuickMesh;

public class Generate : MonoBehaviour {

	// Use this for initialization
	void Start () {


		Selection s = new Selection ().Circle (4).Rotate(-90,0,0).Colour(Color.green).Extrude(1);
		var s2 = s.Keep("side=1").Colour (Color.black);
		s = s.Keep ("side=3").Extrude (1).Keep ("cap");
		for(int i=0; i<6;i++){
			s=s.Extrude(1).Keep ("cap").Rotate (0,30,0);
		}
		s=s.Extrude(5).Keep("cap");
		for(int i=0; i<4;i++){
			s=s.Extrude(1).Keep ("cap").Rotate (0,30,20);
		}
		s.Colour(Color.red);//.Join();
		
		s.Join(s2);
		
		
		
		
		//s=Tree(s);
		/*
		s=s=s.Scale(0.1f).Extrude(3).Filter ("cap");
		for(int i=0; i<8; i++){
			s=s.Extrude(3).Keep("cap").Extrude(0.1f);
			s=i%2==0?s.Keep("side=2"):s.Keep("side=2");
		}
		s = s.SelectAll();
		for (int i=0; i<2; i++) s = s.Subdivide ()
			.Smooth (5, 0.5f).Inflate(0.1f);
			
			
		for (int i=0; i<10; i++) s = s.Smooth (1, 0.5f).Inflate(0.05f);
		
		
		s=s.FloodSmooth(90);
		
		
		s.RecalculateNormals();
		s.SmoothingGroup(1);
		
		s.Flip();
		*/
		
		
		
		//s.Duplicate().Inflate(0.05f).Flip().SubMesh(1).Colour(Color.black);
		
		float offset = 0;
		foreach(Mesh mesh in s.Finish ()){
		
			Material mat2 = (Material)Resources.Load("Outline", typeof(Material));
			Material mat = (Material)Resources.Load("Brown", typeof(Material));
			
			
			GameObject plane = new GameObject("GenTree");
			plane.transform.position = new Vector3(0,0,0);
			
			
			MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
			meshFilter.mesh=mesh;
			MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			Material[] mats = new Material[2];
			mats[0]=mat;
			mats[1]=mat2;
			renderer.materials= mats;
				
			//plane.transform.localScale = new Vector3(100,100,100);
		}



	}
	
	private Selection Tree(Selection s){
		s=s.Extrude (1).Keep("cap");
		
		float scale = 1;
		
		int group = 0;
		for (int i=0; i<4; i++) {
			s = s.Colour(new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f)));
			
			s = s.Extrude(3*scale).Keep("cap").Scale (0.5f).Extrude (scale).Filter("cap","side=1").Extrude (1*scale).Keep ("cap").Rotate (-30,0,0).Extrude(1*scale).Keep("cap").Rotate(-30,0,0);
			
			
			foreach(Face f in s.Selected){
				f.MeshGroup = group/50;
				group++;
				
			}
			
			scale = scale*0.5f;
		}
		
		s = s.SelectAll ();
		s=s.FloodSmooth(30f);
		
		for (int i=0; i<1; i++) s = s.Subdivide ()
			.Smooth (5, 0.5f).Inflate(0.03f);
		
		
		
		return s;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
