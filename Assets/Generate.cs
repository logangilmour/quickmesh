using UnityEngine;
using System.Collections;
using QuickMesh;

public class Generate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Material mat = (Material)Resources.Load("Brown", typeof(Material));

		GameObject plane = new GameObject("GenTree");
		plane.transform.position = new Vector3(0,0,0);


		MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));

		Selection s = new Selection ().Circle (4).SmoothingGroup(1).Rotate (-90,0,0);
		float scale = 1;

		for (int i=0; i<4; i++) {
			
			s = s.Extrude(3*scale).Keep("cap").Scale (0.5f).Extrude (scale).Filter ("cap").Extrude (1*scale).Keep ("cap").Rotate (-30,0,0).Extrude(1*scale).Keep("cap").Rotate(-30,0,0);
			scale = scale*0.5f;
		}
		s = s.SelectAll ();
		s.Keep ("side=0").SmoothingGroup (1);
		s.Keep ("side=1").SmoothingGroup (2);
		s.Keep ("side=2").SmoothingGroup (3);
		s.Keep ("side=3").SmoothingGroup (4);

		for (int i=0; i<1; i++) s = s.Subdivide ().Smooth (5, 0.5f).Inflate(0.1f);
						


		meshFilter.mesh = s.Finish ();

		MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.sharedMaterial = mat;

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
