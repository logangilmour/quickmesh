using System;
using System.Collections.Generic;
namespace QuickMesh
{
		public class AdjacencyList
		{
		public Dictionary<Vertex,HashSet<Vertex>> Adjacent = new Dictionary<Vertex,HashSet<Vertex>>();
			public AdjacencyList ()
			{
			}

			public AdjacencyList(Selection selection)
			{
				AddAll (selection);
			}

		public void Add(Vertex a, Vertex b){
			if (!Adjacent.ContainsKey (a)) {
				Adjacent[a]=new HashSet<Vertex>();
						}
			Adjacent [a].Add (b);
			if (!Adjacent.ContainsKey (b)) {
				Adjacent[b]=new HashSet<Vertex>();
			}
			Adjacent [b].Add (a);
		}

		public void Add(Face f){
			int vertexCount = f.Vertices.Count;
			
			for(int i = 0; i< vertexCount; i++){
				Vertex current = f.Vertices[i];
				Vertex next = f.Vertices[(i + 1) % vertexCount];
				Add (current,next);
			}
		}

		public void AddAll(Selection selection){
			selection.Each ((s,f) => {
								Add (f);
						});
		}
			
	}
}

