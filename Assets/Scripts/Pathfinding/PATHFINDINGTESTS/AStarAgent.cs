using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using HordeAStar;

public class AStarAgent : MonoBehaviour 
{
	private Vector3[] nodes;
	[ReadOnly] public int index = 0;
	[ReadOnly] public bool isMoving = false;	
	
	
	// Use this for initialization
	void Start () 
	{
		//arr = Program.Main(tex,prefab,lineRenderer);
		
		// fly through points
		//StartCoroutine(fly());
		transform.position = nodes[0];
		flystart();
		
	}
	
	private void flystart()
	{
		StartCoroutine(fly());
	}
	
	private IEnumerator fly()
    {
		Vector3 startPosition = transform.position;
		Vector3 endPosition = nodes[index];
		isMoving = true;
		float time = 0;
		
		//transform.LookAt(arr[p+1]);
		SmoothLookAt smooth = GetComponent<SmoothLookAt>();
//		smooth.target = arr[p+1];

		
		while (time < 1.0f)
		{
			time += Time.deltaTime*0.5f;
			
			//transform.LookAt(Vector3.Lerp(arr[p], arr[p+1], t));
			//transform.LookAt(Vector3.Lerp(transform.position, arr[p+1], t));
			
			transform.position = Vector3.Lerp(startPosition, endPosition, time);
			
			//var rotation = Quaternion.LookRotation(arr[p+1] - arr[p]);
			//var rotation = Quaternion.LookRotation(Vector3.Lerp(arr[p], arr[p+1], t) - transform.position,  Vector3.up);
			//transform.rotation = Quaternion.Slerp(transform.rotation, rotation, t);
			
			smooth.target = Vector3.Lerp(nodes[index+1], nodes[index+2], time);
			
			yield return null;
		}
		
		// TODO: last point is over the array..?
		
		// Assets/FastAStar/AStarRunner.cs(59,21): error CS0019: Operator `<' cannot be applied to operands of type `int' and `method group'
		//if (p<arr.length)
		if (index<nodes.Length-1)
		{
			index++;
			Invoke("flystart", 0);
		}
		
		isMoving = false;
		yield return 0;
	}

}
