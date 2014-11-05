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
			
	}
}

