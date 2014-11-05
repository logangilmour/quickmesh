using System;
using System.Collections.Generic;
using UnityEngine;
namespace QuickMesh
{
	public class Selection
	{
		private List<Face> Faces = new List<Face>();
		public List<Face> Selected = new List<Face>();
		public Dictionary<String, Dictionary<Face,String>> Attributes = new Dictionary<String, Dictionary<Face,String>>();

		public Selection ()
		{
		}

		public Selection Make(){
			Selection s = new Selection ();
			s.Faces = this.Faces;
			s.Attributes = this.Attributes;
			return s;
		}

		public Selection Circle(int vertexCount){
			Selection s = Make ();
			float arcLength = Mathf.PI * 2 / vertexCount;
			Face face = new Face();
			for (int i = 0; i < vertexCount; i++){
				Vertex vertex = new Vertex(Mathf.Cos(-i*arcLength), Mathf.Sin(-i*arcLength), 0);
				face.Vertices.Add(vertex);
			}
			Faces.Add(face);
			s.Selected.Add (face);
			return s;
		}

		public delegate void Mapper(Selection selection, Face face);

		public String Attr(Face face, String key){
			if (!Attributes.ContainsKey (key)) {
				return null;
			}
			var dict = Attributes [key];
			if (!dict.ContainsKey (face)) {
				return null;
			}
			return dict [face];
		}

		public void Attr(Face face, String key, String val){

			if (!Attributes.ContainsKey(key))
			{
				Attributes[key]= new Dictionary<Face, string> ();
			}
			Attributes[key] [face] = val;
		}

		public Selection Extrude(float distance){
			return Each ((s,f) => {
				Attr (f,"hidden","true");
				Vector3 extrusion = f.Normal()*distance;

				Face cap = new Face();
				Attr (cap, "cap","true");
				cap.Orientation = f.Orientation;
				int vertexCount = f.Vertices.Count;

				for(int i=0;i<vertexCount;i++){
					cap.Vertices.Add (new Vertex(f.Vertices[i].Position+extrusion));
				}

				for(int i=0;i<vertexCount;i++){
					Face side = new Face();
					Attr (side,"side",i.ToString());

					side.Vertices.Add (f.Vertices[(i+1)%vertexCount]);
					side.Vertices.Add (cap.Vertices[(i+1)%vertexCount]);
					side.Vertices.Add (cap.Vertices[i]);
					side.Vertices.Add (f.Vertices[i]);


					side.Orientation = Vector3.Cross(side.Normal(), extrusion).normalized;

					s.AddSelected (side);
				}

				s.AddSelected(cap);
			});
		}

		public Selection Subdivide(){
			var t = Time.time;
			var edgeVertices = new EdgeLabelSet<Vertex>();

			return Each((s,f)=>{
				Attr(f,"hidden","true");
				int vertexCount = f.Vertices.Count;
				List<Vertex> edgeRing = new List<Vertex>();

				for(int i=0; i<vertexCount;i++)
				{
					Vertex current = f.Vertices[i];
					Vertex next = f.Vertices[(i+1)%vertexCount];
					var edgePoint = edgeVertices.Label(current,next);
					if(edgePoint==null){
						edgePoint = new Vertex((current.Position+next.Position)/2);
						edgeVertices.Add(current,next,edgePoint);
					}
					edgeRing.Add (current);
					edgeRing.Add (edgePoint);
				}

				var facePoint = new Vertex(f.Barycenter());

				int count = edgeRing.Count;
				for (int i = 0; i < count; i += 2){
					Face sub = new Face();
					sub.Orientation = f.Orientation;
					sub.Vertices.Add(facePoint);
					sub.Vertices.Add(edgeRing[(i + count - 1) % count]);
					sub.Vertices.Add(edgeRing[i]);
					sub.Vertices.Add(edgeRing[(i + 1) % count]);
					s.AddSelected(sub);
				}
			});

			Debug.Log ("Subdivided in " + (Time.time - t));
		}

		public Selection Smooth(int iterations, float factor){
			var t = Time.time;
			var edges = new EdgeLabelSet<int>();
			var tempVertices = new Dictionary<Vertex,Vector3> ();

			var adjacency = new Adjacency ();

			foreach (Face f in Selected) {
				int vertexCount = f.Vertices.Count;

				for(int i = 0; i< vertexCount; i++){
					Vertex current = f.Vertices[i];
					Vertex next = f.Vertices[(i + 1) % vertexCount];

					adjacency.Add (current,next);

					edges.Add (current,next,edges.Label(current,next)+1);
					tempVertices[current] = current.Position;
				}
			}

			var edgeVertices = new HashSet<Vertex> ();
			int tot = 0;
			int edge = 0;
			foreach (var label in edges) {
				tot++;
				if(label.Label<2){
					edge++;
					edgeVertices.Add (label.A);	
					edgeVertices.Add (label.B);
				}
			}

			for(int i=0; i<iterations; i++){
				foreach(var kp in adjacency.Adjacent){

					var a = kp.Key;
					if(edgeVertices.Contains(a)){continue;}
					Vector3 bary = Vector3.zero;
					float total = 0;
					int ncount=0;
					foreach(var b in kp.Value){
						ncount++;
						Vector3 delta = b.Position - tempVertices[a];
						float weight = 1f / Vector3.Distance(tempVertices[a], b.Position);

						bary+= (delta * weight);

						total += weight;
					}
					
					a.Position = tempVertices[a] + (bary/total) * factor;
				}
				foreach(var v in adjacency.Adjacent.Keys){
					tempVertices[v]=v.Position;
				}
			}
			Debug.Log ("Smoothed in " + (Time.time - t));

				return this;
		}
	
		public Selection Filter(params String[] filters){
			return Each ((s,f)=>{
				foreach(String filter in filters){

					if(filter.Contains("=")){
						String[] keyval = filter.Split('=');
						if(Attributes.ContainsKey(keyval[0]) 
						   && Attributes[keyval[0]].ContainsKey(f)
						   && Attributes[keyval[0]][f]==keyval[1]){
							s.Selected.Add (f);
						}
					}else if(Attributes.ContainsKey(filter) 
					         && Attributes[filter].ContainsKey(f)){
						s.Selected.Add(f);
					}
				}
			});
		}

		public void AddSelected(Face face){
			Faces.Add (face);
			Selected.Add (face);
		}

		public Selection Each(Mapper m){
			Selection s = Make ();
			foreach (Face face in Selected) {
				m(s,face);
			}
			return s;
		}

		public Mesh Finish(){
			Mesh m = new Mesh ();
			List<int> triangles = new List<int> ();
			List<Vector3> vertices = new List<Vector3> ();

			foreach (Face face in Faces) {
				if(Attr (face,"hidden")!="true"){
				int first = vertices.Count;
				for (int i=0; i<face.Vertices.Count;i++){
					vertices.Add(face.Vertices[i].Position);
				}
				for (int i=0; i<face.Vertices.Count-2; i++) {
					
					triangles.Add(first);
					triangles.Add(first+i+1);
					triangles.Add(first+i+2);
				}
				}
			}
			m.vertices = vertices.ToArray();
			m.triangles = triangles.ToArray ();	
			m.RecalculateNormals();
			return m;
		}
	}
}

