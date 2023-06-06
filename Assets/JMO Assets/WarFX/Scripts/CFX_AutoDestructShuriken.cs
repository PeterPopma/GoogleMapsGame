using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShuriken : MonoBehaviour
{	
	void Update()
    {
		if (!GetComponent<ParticleSystem>().IsAlive(true)) 
		{
			Destroy(gameObject);
		}
	}
}
