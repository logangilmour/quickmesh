using UnityEngine;
using System.Collections;
using QuickMesh;

public class Generate : MonoBehaviour {

	// Use this for initialization
	void Start () {


		Selection s = new Selection ().Circle (4).Colour(Color.red).Rotate (-90, 0, 0).Extrude (1).Keep("cap");

		float scale = 1;

		int group = 0;
		for (int i=0; i<6; i++) {
			s = s.Colour(new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f)));
			
			s = s.Extrude(3*scale).Keep("cap").Scale (0.5f).Extrude (scale).Filter("cap").Extrude (1*scale).Keep ("cap").Rotate (-30,0,0).Extrude(1*scale).Keep("cap").Rotate(-30,0,0);
			
			
			foreach(Face f in s.Selected){
				f.MeshGroup = group/50;
				group++;
				
			}
			scale = scale*0.5f;
		}

		s = s.SelectAll ();
		s.Keep ("side=0").SmoothingGroup (1);
		s.Keep ("side=1").SmoothingGroup (2);
		s.Keep ("side=2").SmoothingGroup (3);
		s.Keep ("side=3").SmoothingGroup (4);
		s.Keep ("cap").SmoothingGroup (5);

		for (int i=0; i<2; i++) s = s.Subdivide ()
			.Smooth (5, 0.5f).Inflate(0.02f);
			

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
