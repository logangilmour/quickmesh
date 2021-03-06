// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
namespace QuickMesh
{
	public class AdjacentFaces
	{
		private EdgeLabelSet<Face> finder = new EdgeLabelSet<Face>();
		private Dictionary<Face,HashSet<Face>> adjacent = new Dictionary<Face,HashSet<Face>>();
		public AdjacentFaces (Selection s)
		{
			s.Each ((sel,f)=>{
				if(!adjacent.ContainsKey(f)){adjacent[f]=new HashSet<Face>();}
				var vertexCount = f.Vertices.Count;
				for (int i = 0; i< vertexCount; i++) {
					Vertex current = f.Vertices [i];
					Vertex next = f.Vertices [(i + 1) % vertexCount];
					var adj = finder.Label (current,next);
					if(adj!=null){
						if(!adjacent.ContainsKey(adj)){adjacent[adj]=new HashSet<Face>();}
						adjacent[adj].Add(f);
						adjacent[f].Add (adj);
					}else{
						finder.Add (current, next, f);
					}
				}
			});
		}
		
		public HashSet<Face> AdjacentTo(Face f){
			return adjacent[f];
		}
		
	}
}

