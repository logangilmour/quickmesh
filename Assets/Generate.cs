using UnityEngine;
using System.Collections;
using QuickMesh;

public class Generate : MonoBehaviour {

	// Use this for initialization
	void Start () {


		Selection s = new Selection ().Circle (4).Colour(Color.green).Rotate (-90, 0, 0).Extrude (1).Keep("cap");

		float scale = 1;

		int group = 0;
		for (int i=0; i<4; i++) {
			//s = s.Colour(new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f)));
			
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
			

		float offset = 0;
		foreach(Mesh mesh in s.Finish ()){
			Material mat = (Material)Resources.Load("Brown", typeof(Material));
			
			GameObject plane = new GameObject("GenTree");
			plane.transform.position = new Vector3(0,0,0);
			
			
			MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
			meshFilter.mesh=mesh;
			MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
			renderer.sharedMaterial = mat;
			plane.transform.position+=Vector3.up*offset;
		}



	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
