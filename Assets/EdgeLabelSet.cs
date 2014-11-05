
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace QuickMesh
{
	public struct EdgeLabel<TI>{
		public Vertex A;
		public Vertex B;
		public TI Label;
	}
	public class EdgeLabelSet<T> : IEnumerable<EdgeLabel<T>>
	{


		protected Dictionary<Vertex,Dictionary<Vertex,T>> Labels = new Dictionary<Vertex,Dictionary<Vertex,T>>();
				public EdgeLabelSet ()
				{
				
				}

		public void Add(Vertex a, Vertex b, T label){
			if (!Labels.ContainsKey (a)) {
				var temp = a;
				a = b;
				b = temp;
			}

			if (!Labels.ContainsKey (a)) {
				Labels [a] = new Dictionary<Vertex,T> ();
			}
			Labels [a][b]=label;
		}

		public T Label(Vertex a, Vertex b){
			if (Labels.ContainsKey (a)) {
				foreach(var kp in Labels[a]){
					if(kp.Key==b){
						return kp.Value;
					}
				}
			}
			if (Labels.ContainsKey (b)) {
				foreach(var kp in Labels[b]){
					if(kp.Key==a){
						return kp.Value;
					}
				}
			}
			return default(T);
		}

		IEnumerator<EdgeLabel<T>> IEnumerable<EdgeLabel<T>>.GetEnumerator()
		{
			foreach (var neighbours in Labels){
				foreach (var kp in neighbours.Value) {
					EdgeLabel<T> label = new EdgeLabel<T> ();
					label.A = neighbours.Key;	
					label.B = kp.Key;
					label.Label = kp.Value;
					yield return label;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() { 
			foreach (var neighbours in Labels){
				foreach (var kp in neighbours.Value) {
					EdgeLabel<T> label = new EdgeLabel<T> ();
					label.A = neighbours.Key;	
					label.B = kp.Key;
					label.Label = kp.Value;
					yield return label;
				}
			}
		}

	}

}

