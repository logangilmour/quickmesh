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
				Vertex vertex = new Vertex(Mathf.Cos(i*arcLength), Mathf.Sin(i*arcLength), 0);
				face.Vertices.Add(vertex);
			}
			Faces.Add(face);

			Debug.Log ("Normal: "+face.Normal);

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

		public Selection Attr(string key, string value){
			foreach (Face face in Selected) {
				Attr (face,key,value);
			}
			return this;
		}

		public Selection Extrude(float distance){
			return Each ((s,f) => {
				f.Visible=false;

				Vector3 extrusion = f.Normal.normalized*distance;

				Face cap = f.CloneProperties();

				Attr (cap, "cap","true");
				cap.Orientation = f.Orientation;
				int vertexCount = f.Vertices.Count;

				for(int i=0;i<vertexCount;i++){
					cap.Vertices.Add (new Vertex(f.Vertices[i].Position+extrusion));
				}

				for(int i=0;i<vertexCount;i++){
					Face side = f.CloneProperties();
					Attr (side,"side",i.ToString());

					side.Vertices.Add (f.Vertices[(i+1)%vertexCount]);
					side.Vertices.Add (cap.Vertices[(i+1)%vertexCount]);
					side.Vertices.Add (cap.Vertices[i]);
					side.Vertices.Add (f.Vertices[i]);


					side.Normal = side.CalculateNormal();
					
					side.Orientation = Vector3.Cross(side.Normal, extrusion).normalized;

					s.AddSelected (side);
				}

				s.AddSelected(cap);
			});
		}

		public Selection Subdivide(){
			var edgeVertices = new EdgeLabelSet<Vertex>();

			return Each((s,f)=>{
				f.Visible=false;
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
					Face sub = f.CloneProperties();
					sub.Orientation = f.Orientation;
					sub.Vertices.Add(facePoint);
					sub.Vertices.Add(edgeRing[(i + count - 1) % count]);
					sub.Vertices.Add(edgeRing[i]);
					sub.Vertices.Add(edgeRing[(i + 1) % count]);
					s.AddSelected(sub);
				}
			});

		}

		public Selection Smooth(int iterations, float factor){
			var edgeVertices = new EdgeFinder (this).EdgeVertices ();
			var tempVertices = new Dictionary<Vertex,Vector3> ();

			var adjacency = new AdjacencyList (this);


			foreach (Face f in Selected) {
				foreach(var v in f.Vertices){
					tempVertices[v] = v.Position;
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

				return this;
		}
	
		public Selection Keep(params String[] filters){
			return Filter((f) => {
				foreach(string filter in filters){

					if(filter.Contains("=")){
						String[] keyval = filter.Split('=');
						if(Attributes.ContainsKey(keyval[0]) 
						   && Attributes[keyval[0]].ContainsKey(f)
						   && Attributes[keyval[0]][f]==keyval[1]){
							return true;
						}
					}else if(Attributes.ContainsKey(filter) 
					         && Attributes[filter].ContainsKey(f)){
						return true;
					}
				}
				return false;

			});
		}

		public Selection Filter(params String[] filters){
			return Filter((f) => {
				foreach(string filter in filters){
					
					if(filter.Contains("=")){
						String[] keyval = filter.Split('=');
						if(Attributes.ContainsKey(keyval[0]) 
						   && Attributes[keyval[0]].ContainsKey(f)
						   && Attributes[keyval[0]][f]==keyval[1]){
							return false;
						}
					}else if(Attributes.ContainsKey(filter) 
					         && Attributes[filter].ContainsKey(f)){
						return false;
					}
				}
				return true;

			});
		}

		public delegate bool Predicate(Face f);

		public Selection Filter(Predicate p){
			return Each ((s,f) => {
								if (p (f)) {
										s.Selected.Add (f);
								}
						});
		}

		public Selection Inflate(float ammount){
			var vertexNormals = new Dictionary<Vertex,List<Vector3>> ();
			var edgeVertices = new EdgeFinder (this).EdgeVertices ();


			Each ((s,face) => {
						var normal = face.CalculateNormal();

						foreach (Vertex v in face.Vertices) {
								if (!vertexNormals.ContainsKey (v)) {
										vertexNormals [v] = new List<Vector3> ();
								}
								vertexNormals [v].Add (normal);
						}
				});
			foreach (var kp in vertexNormals) {
				Vertex v = kp.Key;
				if(edgeVertices.Contains(v)){continue;}

				var normal = Vector3.zero;
				foreach(var n in kp.Value){
					normal+=n;
				}
				v.Position+=ammount*normal.normalized;
			}
			return this;
		}

		public Selection Transform(Matrix4x4 transformation){
			
			Each ((s, face) => {
								
				var normal = face.CalculateNormal();
				var barycenter = face.Barycenter ();
				Quaternion direction = Quaternion.FromToRotation (normal, Face.DefaultNormal);
				Quaternion orientation = Quaternion.FromToRotation (direction*face.Orientation, Face.DefaultOrientation);
				
				direction = orientation * direction;
				
				foreach (Vertex v in face.Vertices) {
					Vector3 temp = direction * (v.Position - barycenter);
					temp = transformation.MultiplyPoint (temp);
					v.Position = Quaternion.Inverse (direction) * temp + barycenter;
				}
				Quaternion rotation = QuaternionFromMatrix (transformation);
				
				Vector3 newOrientation = (Quaternion.Inverse (direction) *
				                          (rotation * Face.DefaultOrientation)).normalized;
				
				Vector3 newNormal = (Quaternion.Inverse (direction) *
				                     (rotation * Face.DefaultNormal)).normalized;
				
				if (newOrientation != Vector3.zero) {
					face.Orientation = newOrientation;
				}
				
				if(newNormal !=Vector3.zero){
					face.Normal = newNormal;
				}	
			});
			return this;
		}
		
		public Selection FloodSmooth(float threshold){
			Each ((s,f)=>{
				f.SmoothingGroup=0;
			});
			var faceFinder = new AdjacentFaces(this);
			int newGroup = 0;
			Each((s,f)=>{
				if(f.SmoothingGroup==0){
					f.SmoothingGroup = newGroup;
					newGroup++;
				}
				foreach(Face adj in faceFinder.AdjacentTo(f)){
					float angle;
					Vector3 axis;
					Quaternion.FromToRotation(f.Normal,adj.CalculateNormal()).ToAngleAxis(out angle,out axis);
					if(angle<=threshold){
						adj.SmoothingGroup=f.SmoothingGroup;
					}
				}
			});
			return this;
		}
		
		public Selection Duplicate(){
			var clones = new Dictionary<Vertex,Vertex>();
			
			return Each ((s,f)=>{
				Face face = f.CloneProperties();
				foreach(Vertex v in f.Vertices){
					if(!clones.ContainsKey(v)){
						clones[v]=v.Clone();
					}
					face.Vertices.Add (clones[v]);
				}
				s.AddSelected(face);
			});
		}
		
		//returns -1 when to the left, 1 to the right, and 0 for forward/backward
		public static float Angle(Vector3 fwd, Vector3 targetDir, Vector3 up){
			var perp = Vector3.Cross(fwd, targetDir);
			var dir = Vector3.Dot(perp, up);
			
			var angle = Vector3.Angle(fwd,targetDir);
			
			if (dir < 0.0) {
				return 360f-angle;
			}
			return angle;
			
		}
		
		public Selection Join(Selection other){
			var s = Make ();
			int min = Math.Min (Selected.Count, other.Selected.Count);
			for(int i=0; i< min; i++){
				Face A = Selected[i];
				Face B = other.Selected[i];
				A.Visible=false;
				B.Visible=false;
				if(B.Vertices.Count>A.Vertices.Count){
					Face temp = A;
					A = B;
					B = temp;
				}
				var ABary = A.Barycenter();
				var BBary = B.Barycenter();
				var ANorm = A.CalculateNormal();
				var BNorm = B.CalculateNormal();
				var AQ = Quaternion.FromToRotation(ANorm,BBary-ABary);
				var BQ = Quaternion.FromToRotation(BNorm,ABary-BBary);
				
				float angleOffset;
				Vector3 dir;
				Quaternion.FromToRotation(AQ*A.Orientation,BQ*B.Orientation).ToAngleAxis(out angleOffset,out dir);
				List<float> AAngles = new List<float>();
				List<float> BAngles = new List<float>();
				
				foreach(Vertex v in A.Vertices){
					AAngles.Add (Angle (A.Orientation,v.Position - ABary,ANorm));
				}
				foreach(Vertex v in B.Vertices){
					BAngles.Add (Angle (B.Orientation,v.Position - BBary,BNorm*-1));
					
				}
				var edges = new List<Vertex>();
				var count = new Dictionary<Vertex,int>();
				for(int ai=0; ai<AAngles.Count;ai++){
					float angle = AAngles[ai];
					Debug.Log ("checking vertex with angle "+angle);
					
					float nearness = 180f;
					float next = 180f;
					Vertex first = null; 
					Vertex second = null;
					for(int bi=0; bi<BAngles.Count; bi++){
						//Debug.Log ("Comparing with "+BAngles[bi]);
						var diff = BAngles[bi]-angle+angleOffset;
						while(diff>180f){
							diff-=360;
						}
						if(diff<0){
							diff=Mathf.Abs(diff);
						}
						if(diff<nearness){
							second = first;
							first = B.Vertices[bi];
							nearness = diff;
						}else if(diff<next){
							second = B.Vertices[bi];
							next = diff;
						}
					}
					
					edges.Add (A.Vertices[ai]);
					if(!count.ContainsKey(first)){count[first]=0;}
					
					if(second!=null){
					
						if(!count.ContainsKey(second)){count[second]=0;}
						Debug.Log ("Comparing "+count[first]+" with "+A.Vertices.Count/B.Vertices.Count);
						if(count[first]<Mathf.Floor (A.Vertices.Count/B.Vertices.Count)){
							edges.Add (first);
							count[first]++;
						}else{
							edges.Add (second);
							count[second]++;
						}
					}else{
						edges.Add (first);
						count[first]++;
					}
					
					
				}
				var edgeCount = edges.Count;
				for(int e=0;e<edgeCount;e+=2){
					Face side = A.CloneProperties();
					
					Attr (side,"side",(e/2).ToString());
					
					side.Vertices.Add (edges[(e+2)%edgeCount]);
					side.Vertices.Add (edges[(e+3)%edgeCount]);
					side.Vertices.Add (edges[e+1]);
					side.Vertices.Add (edges[e]);
					
					
					side.Normal = side.CalculateNormal();
					//TODO fix this, as I'm pretty sure that just crossing the old direction with the new direction
					//preserves nothing about the orientation. Extrusion has this problem too.
					side.Orientation = Vector3.Cross(side.Normal, A.CalculateNormal()).normalized;
					
					s.AddSelected (side);
				}
			}
			return s;
		}
		
		public Selection RecalculateNormals(){
			Each ((s,f)=>{
				f.Normal = f.CalculateNormal();
			});
			return this;
		}
		
		public Selection Flip(){
			Each ((s,f)=>{
				f.Vertices.Reverse();
				f.Normal = (f.Normal*-1).normalized;
			});
			return this;
		}
		
		public Selection Rotate(float x, float y,float z){
			return Transform (Matrix4x4.TRS (
				Vector3.zero, 
				Quaternion.Euler (new Vector3 (x, y, z)), 
				Vector3.one));
		}


		public Selection Scale(float x, float y,float z){
			return Transform (Matrix4x4.TRS (
				Vector3.zero, 
				Quaternion.identity, 
				new Vector3 (x, y, z)));
		}

		public Selection Scale(float s){
			return Scale (s, s, s);
		}

		public Selection Translate(float x, float y, float z){
			return Translate (new Vector3 (x, y, z));
		}
		public Selection Translate(Vector3 translation){
			return Transform (Matrix4x4.TRS (
				translation, 
				Quaternion.identity, 
				Vector3.one));
		}

		public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }


		public void AddSelected(Face face){
			Faces.Add (face);
			Selected.Add (face);
		}

		public Selection SelectAll(){
			var m = Make ();
			foreach(Face f in Faces){
				m.Selected.Add (f);
			}
			return m;
		}

		public Selection Each(Mapper m){
			Selection s = Make ();
			foreach (Face face in Selected) {
				if(face.Visible){
					m(s,face);
				}
			}
			return s;
		}


		public Selection SmoothingGroup(int group){
			foreach(Face f in Selected){
				f.SmoothingGroup = group;
			}
			return this;
		}
		
		public Selection SubMesh(int submesh){
			foreach(Face f in Selected){
				f.SubMesh = submesh;
			}
			return this;
		}

		public Selection Colour(Color color){
			foreach(Face f in Selected){
				f.Color = color;
			}
			return this;
		}

		public Selection Hide(){
						foreach (Face f in Selected) {
								f.Visible = false;
						}
						return this;
				}
		public Selection Show(){
						foreach (Face f in Selected) {
								f.Visible = true;
						}
						return this;
				}
		public Selection MeshGroup(int group){
						foreach (Face f in Selected) {
								f.MeshGroup = group;
						}
						return this;
				}

		public List<Mesh> Finish(){



			var smoothingGroups = new Dictionary<Vertex,Dictionary<int,Vertex>> ();
			int reused = 0;
			int newvert = 0;
			int newgroups = 0;
			int unsmoothed = 0;
			int original = 0;


			foreach (Face face in Faces) {
				if(!face.Visible)continue;
				var smoothing = face.SmoothingGroup;

				for(int i=0; i<face.Vertices.Count; i++){
					Vertex v = face.Vertices[i];
					if(smoothing!=0){
						if(smoothingGroups.ContainsKey(v)){
							if(smoothingGroups[v].ContainsKey(smoothing)){
								face.Vertices[i]=smoothingGroups[v][smoothing];
							}else{
								Vertex n = v.Clone();
								face.Vertices[i]=n;
								smoothingGroups[v][smoothing]=n;

							}
						}else{
							smoothingGroups[v]=new Dictionary<int,Vertex>();
							smoothingGroups[v][smoothing]=v;
						}
					}else{
						face.Vertices[i]=v.Clone();
					}
				}
			}


			var colorLists = new Dictionary<Vertex,List<Color>> ();
			var normalLists = new Dictionary<Vertex,List<Vector3>> ();

			foreach (Face face in Faces) {
				if(!face.Visible){continue;}
				foreach (Vertex v in face.Vertices){
					if(!colorLists.ContainsKey(v)){colorLists[v]=new List<Color>();}
					if(!normalLists.ContainsKey(v)){normalLists[v]=new List<Vector3>();}
					colorLists[v].Add(face.Color);
					normalLists[v].Add (face.Normal);
				}
			}

			var colorMap = new Dictionary<Vertex,Color> ();
			var normalMap = new Dictionary<Vertex,Vector3> ();

			foreach (var kp in normalLists) {
				var v = kp.Key;

				Vector3 normal = Vector3.zero;
				Color color = Color.black;
				foreach(var n in kp.Value){
					normal += n;
				}
				foreach(var c in colorLists[v]){
					color += c;
				}
				normal.Normalize();
				color /= kp.Value.Count;

				normalMap[v]=normal;
				colorMap[v]=color;
			}
			
			
			var meshGroups = new Dictionary<Vertex,Dictionary<int,Vertex>> ();
			
			
			foreach (Face face in Faces) {
				if(!face.Visible)continue;
				var mesh = face.MeshGroup;
				
				for(int i=0; i<face.Vertices.Count; i++){
					Vertex v = face.Vertices[i];
						if(meshGroups.ContainsKey(v)){
							if(meshGroups[v].ContainsKey(mesh)){
								face.Vertices[i]=meshGroups[v][mesh];
								reused++;
							}else{
								Vertex n = v.Clone();
								normalMap[n]=normalMap[v];
								colorMap[n] = colorMap[v];
								face.Vertices[i]=n;
								meshGroups[v][mesh]=n;
								
								newvert++;
							}
						}else{
							meshGroups[v]=new Dictionary<int,Vertex>();
							meshGroups[v][mesh]=v;
							newgroups++;
						}
				}
			}
			
			
			
			
			


			var indices = new Dictionary<int,Dictionary<Vertex,int>>();
			var totalReused = 0;
			var totalCreated = 0;

			var colours = new Dictionary<int,List<Color>> ();
			var normals = new Dictionary<int,List<Vector3>> ();
			var triangles = new Dictionary<int,Dictionary<int,List<int>>> ();
			var vertices = new Dictionary<int,List<Vector3>> ();
			
			



			foreach(Face face in Faces){
				if(!face.Visible){continue;}
				var mesh = face.MeshGroup;
				var faceIndices = new List<int>();
				
				foreach(Vertex v in face.Vertices){
					if(!indices.ContainsKey(mesh)){
					indices[mesh]=new Dictionary<Vertex, int>();
					colours[mesh]=new List<Color>();
					normals[mesh] = new List<Vector3>();
					triangles[mesh] = new Dictionary<int,List<int>>();
					vertices[mesh] = new List<Vector3>();
					}
					if(indices[mesh].ContainsKey(v)){
						faceIndices.Add (indices[mesh][v]);
						totalReused++;
					}else{
						faceIndices.Add (vertices[mesh].Count);
						indices[mesh][v]=vertices[mesh].Count;
						vertices[mesh].Add (v.Position);
						colours[mesh].Add (colorMap[v]);
						normals[mesh].Add(normalMap[v]);
						totalCreated++;
					}	
				}
				if(!triangles[mesh].ContainsKey(face.SubMesh)){
					triangles[mesh][face.SubMesh] = new List<int>();
				}
				for (int i=0; i<faceIndices.Count-2; i++) {
					triangles[mesh][face.SubMesh].Add(faceIndices[0]);
					triangles[mesh][face.SubMesh].Add(faceIndices[i+1]);
					triangles[mesh][face.SubMesh].Add(faceIndices[i+2]);
				}
			}

			Debug.Log ("Reused: " + reused + ", New Verts: " + newvert + ", New Group: " + newgroups+", Unsmoothed: "+unsmoothed);
			Debug.Log ("Total Reused: " + totalReused + ", Total Created: " + totalCreated);

			List<Mesh> meshes = new List<Mesh>();

			foreach(var mesh in indices.Keys){
			
				Mesh m = new Mesh ();
				
				m.vertices = vertices[mesh].ToArray();
				int count = 0;
				foreach(var key in triangles[mesh].Keys){
					count = Math.Max(key,count);
				}
				count++;
				m.subMeshCount = count;
				foreach(var kp in triangles[mesh]){
					m.SetTriangles(kp.Value.ToArray(),kp.Key);
				}
				m.colors = colours[mesh].ToArray ();
				m.normals = normals[mesh].ToArray ();
				m.Optimize ();
				meshes.Add (m);
			}

			return meshes;
		}
	}
}

