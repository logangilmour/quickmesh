
using System;
using System.Collections.Generic;
namespace QuickMesh
{
	public class EdgeFinder : EdgeLabelSet<int>
	{
		public EdgeFinder ()
		{
		}
		public EdgeFinder(List<Face> faces){
			AddFaces(faces);
		}
		public void AddFaces(List<Face> faces){
			foreach (Face face in faces) 
			{
				var vertexCount = face.Vertices.Count;
				for (int i = 0; i< vertexCount; i++) {
					Vertex current = face.Vertices [i];
					Vertex next = face.Vertices [(i + 1) % vertexCount];
					Add (current, next, Label (current, next) + 1);
				}
			}
		}

		public HashSet<Vertex> EdgeVertices(){
			var edgeVertices = new HashSet<Vertex> ();
			foreach (var entry in this) {
				if(entry.Label<2){
					edgeVertices.Add (entry.A);	
					edgeVertices.Add (entry.B);
				}
			}
			return edgeVertices;
		}
	}
}

