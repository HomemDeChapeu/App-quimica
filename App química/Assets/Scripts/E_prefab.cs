using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bond
{
	public GameObject o_ligand;
	public int strenght;

	public Bond(GameObject gameObject, int bondStrenght)
	{
		o_ligand = gameObject;
		strenght = bondStrenght;
	}
}

public class E_prefab : MonoBehaviour
{
	// Colision
	[Header("Colision")]
	public bool colision = false;
	public GameObject colidedObj;
	[SerializeField] Sprite red;
	[SerializeField] Sprite black;

	SpriteRenderer s_renderer;

	// Element's information
	[Header("Element's information")]
	public int max_n_ligands;
	[HideInInspector] public List<Bond> ligands = new List<Bond>();
	[HideInInspector] public List<GameObject> links = new List<GameObject>();

	private void Start()
	{
		s_renderer = GetComponent<SpriteRenderer>();
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		colision = true;
		s_renderer.sprite = red;
		colidedObj = collision.gameObject;
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		colision = false;
		s_renderer.sprite = black;
		colidedObj = null;
	}
}
