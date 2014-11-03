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

		Selection s = new Selection ();
		meshFilter.mesh = s.Circle (5).Extrude(2).Extrude (1).Finish ();

		MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		renderer.sharedMaterial = mat;
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
