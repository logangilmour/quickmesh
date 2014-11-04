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
	public class EdgeLabelSet<T>
	{
		private Dictionary<Vertex,List<KeyValuePair<Vertex,T>>> Labels = new Dictionary<Vertex,List<KeyValuePair<Vertex,T>>>();
				public EdgeLabelSet ()
				{
				
				}

		public void Add(Vertex a, Vertex b, T label){
			if (!Labels.ContainsKey (a)) {
				Labels [a] = new List<KeyValuePair<Vertex,T>> ();
			}
			Labels [a].Add(new KeyValuePair<Vertex, T>(b,label));

			if (!Labels.ContainsKey (b)) {
				Labels [b] = new List<KeyValuePair<Vertex,T>> ();
			}
			Labels [b].Add(new KeyValuePair<Vertex, T>(a,label));
		}

		public T Label(Vertex a, Vertex b){
			if (Labels.ContainsKey (a)) {
				foreach(var kp in Labels[a]){
					if(kp.Key==b){
						return kp.Value;
					}
				}
			}
			return default(T);
		}	
	}

}

