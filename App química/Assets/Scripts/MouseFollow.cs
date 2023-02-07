using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    // Prefabs + camera
    public Camera mainCamera;
    [SerializeField] GameObject carbonPrefab;
    GameObject i_prefab;
    GameObject e_prefab;

    // Analysis
    [HideInInspector] public GameObject firstPos;

    // Ligants
    GameObject currentP;
    GameObject lastP;

    [SerializeField] GameObject linkSprite;
    [SerializeField] GameObject linkSprite_parent;
    [SerializeField] float width;
    [SerializeField] float s_between_bonds;
    [SerializeField] float radius;

    // Grid
    public int gridSize;
    public int influenceRadius;
    float selectRadius;
    E_prefab i_prefab_info;

    void Start()
    {
        UpdatePrefab(carbonPrefab);

		SelectRadius();
    }

    public void UpdatePrefab(GameObject prefab)
	{
        if(i_prefab != null)
            Destroy(i_prefab);

        e_prefab = prefab;
        i_prefab = Instantiate(prefab);
        i_prefab.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.5f);
        i_prefab_info = i_prefab.GetComponent<E_prefab>();
    }

    void Update()
    {
        // Mouse Pos + Instatiation of components
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        i_prefab.transform.position = Grid(mousePos);

        // Correct distance allowed
        bool inRadius = false;

        if (currentP == null)
            inRadius = true;
        else if (Vector2.Distance(i_prefab.transform.position, currentP.transform.position) <= selectRadius)
            inRadius = true;

        // How many bonds will it make
        int bondStrenght = 0;
        bool input = false;

		if (Input.GetKeyDown("1"))
		{
            bondStrenght = 1;
            input = true;
		}
        if (Input.GetKeyDown("2"))
        {
            bondStrenght = 2;
            input = true;
        }
        if (Input.GetKeyDown("3"))
        {
            bondStrenght = 3;
            input = true;
        }

        // Inputs
        if (input && i_prefab_info.colision == false && inRadius)
		{
            PlaceE(bondStrenght);
		}

        if(Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) == false)
		{
            Select();
		}

        if(Input.GetKey(KeyCode.LeftShift) && input && i_prefab_info.colision == true)
		{
            Connect(i_prefab_info.colidedObj, currentP, bondStrenght);
		}

		if (Input.GetKeyDown(KeyCode.Backspace) && currentP != null)
		{
            DestroyP(currentP);
		}
    }

    void PlaceE(int bondStrength)
	{
        // Check for the max ligands
        if (currentP != null)
        {
            E_prefab currentP_check = currentP.GetComponent<E_prefab>();

            int p_n_ligands = n_ligands(currentP_check);

            if (p_n_ligands == currentP_check.max_n_ligands)
            {
                Debug.LogWarning("Can't make any more bonds!");
                return;
            }
            if(p_n_ligands + bondStrength > currentP_check.max_n_ligands)
			{
                Debug.LogWarning("That's too many bonds!");
                return;
            }
        }

        // Instantiate
        Vector3 pos = i_prefab.transform.position;
        GameObject component = Instantiate(e_prefab, pos, Quaternion.identity);

        // First component to be analyzed
        if (firstPos == null)
            firstPos = component;
        else if (pos.x < firstPos.transform.position.x)
            firstPos = component;

        // First prefab placed
        if (currentP == null)
        {
            currentP = component;
            return;
        }

        // Ligands
        lastP = currentP;
        currentP = component;

        CreatLink(currentP, lastP, bondStrength);
    }

    void Select()
    {
        if (i_prefab_info.colidedObj != null)
            currentP = i_prefab_info.colidedObj;
	}

    void Connect(GameObject current, GameObject last, int bondStrength)
	{
        if (current == last)
            return;

        if (Vector2.Distance(last.transform.position, current.transform.position) <= selectRadius)
        {
            int permission = 0;
            
            // Check whether current already has made all the bonds it can / if it can make more bonds
            E_prefab current_check = current.GetComponent<E_prefab>();

            int p_n_ligands = n_ligands(current_check);

            if (p_n_ligands == current_check.max_n_ligands)
            {
                Debug.LogWarning("Can't make any more bonds!");
                return;
			}
            else if (p_n_ligands + bondStrength > current_check.max_n_ligands)
            {
                Debug.LogWarning("That's too many bonds!");
                return;
            }
            else
			{
                permission++;
			}

            // Check whether last already has made all the bonds it can
            E_prefab last_check = last.GetComponent<E_prefab>();

            p_n_ligands = n_ligands(last_check);

            if (p_n_ligands == last_check.max_n_ligands)
            {
                Debug.LogWarning("Can't make any more bonds!");
                return;
            }
            else if (p_n_ligands + bondStrength > last_check.max_n_ligands)
            {
                Debug.LogWarning("That's too many bonds!");
                return;
            }
            else
            {
                permission++;
            }

            // Creates link
            if(permission == 2)
			{
                CreatLink(current, last, bondStrength);
			}
        }
        else
            return;
    }

    void CreatLink(GameObject current, GameObject last, int bondStrenght)
	{
        Vector2 linkPos;
        float linkRotation;
        GameObject link_parent;

        linkRotation = Mathf.Atan((last.transform.position.y - current.transform.position.y) / (last.transform.position.x - current.transform.position.x)) * Mathf.Rad2Deg;
        linkPos = (current.transform.position + last.transform.position) / 2;

        link_parent = Instantiate(linkSprite_parent);

        link_parent.transform.position = linkPos;
        link_parent.transform.rotation = Quaternion.AngleAxis(linkRotation, new Vector3(0, 0, 1));

        List<GameObject> links = new List<GameObject>();

		for (int i = 1; i <= bondStrenght; i++)
		{
            links.Add(Instantiate(linkSprite));
            links[i - 1].transform.parent = link_parent.transform;
            links[i - 1].transform.localPosition = Vector3.zero;
            links[i - 1].transform.localRotation = Quaternion.identity;
            links[i - 1].transform.localScale = new Vector3(Vector2.Distance(current.transform.position, last.transform.position) - (2 * radius), width);
        }

        if (bondStrenght == 2)
        {
            links[0].transform.localPosition = new Vector2(0, -s_between_bonds / 2);
            links[1].transform.localPosition = new Vector2(0, s_between_bonds / 2);
        }
        if (bondStrenght == 3)
        {
            links[1].transform.localPosition = new Vector2(0, -s_between_bonds);
            links[2].transform.localPosition = new Vector2(0, s_between_bonds);
        }

        E_prefab lastP_info = last.GetComponent<E_prefab>();
        E_prefab currentP_info = current.GetComponent<E_prefab>();

        currentP_info.ligands.Add(new Bond (last, bondStrenght));
        lastP_info.ligands.Add(new Bond(current, bondStrenght));

        currentP_info.links.Add(link_parent);
        lastP_info.links.Add(link_parent);
    }

    void DestroyP(GameObject destroyed_p)
	{
        E_prefab destroyedP_info = destroyed_p.GetComponent<E_prefab>();

		foreach (GameObject link in destroyedP_info.links)
		{
            Destroy(link);
		}

		for (int i = 0; i < destroyedP_info.ligands.Count; i++)
		{
            E_prefab prefab_info = destroyedP_info.ligands[i].o_ligand.GetComponent<E_prefab>();

			for (int f = 0; f < prefab_info.ligands.Count; f++)
			{
                if (prefab_info.ligands[f].o_ligand == destroyed_p)
                {
                    prefab_info.ligands.Remove(prefab_info.ligands[f]);
                    break;
                }
            }
        }

        Destroy(destroyed_p);
	}

    Vector3 Grid(Vector3 Pos)
	{
        Vector3 relativePos;
        relativePos.z = 0;

        // y value + formato

        float remainder_y = Pos.y % (gridSize * Mathf.Sin(Mathf.PI / 3));

        relativePos.y = Pos.y - remainder_y;

        int a = 1;
        if (Pos.y < 0)
            a = -1;

        if (remainder_y * a > (gridSize * Mathf.Sin(Mathf.PI / 3)) / 2)
            remainder_y = gridSize * Mathf.Sin(Mathf.PI / 3) * a;
        else
            remainder_y = 0;

        relativePos.y = relativePos.y + remainder_y;


        // x value

        float remainder_x = Pos.x % (gridSize * 2);

        relativePos.x = Pos.x - remainder_x;

        bool center;
        if (relativePos.y % (gridSize * Mathf.Sin(Mathf.PI / 3) * 2) == 0)
            center = false;
        else
            center = true;

        int b = 1;
        if (Pos.x < 0)
            b = -1;

        if (center)
		{
            if (remainder_x * b > gridSize)
                remainder_x = gridSize * 2 * b;
            else
                remainder_x = 0;
        }
		else
		{
            if (remainder_x * b > gridSize)
                remainder_x = gridSize * 1.5f * b;
            else
                remainder_x = gridSize / 2f * b;
		}
        
        relativePos.x = relativePos.x + remainder_x;

        
        return relativePos;
	}

    void SelectRadius()
	{
        int a;
        int b;

        if (influenceRadius % 2 == 1)
		{
            a = (influenceRadius - 1) / 2;
            b = a + 1;
		}
		else
		{
            a = influenceRadius / 2;
            b = a;
		}

        selectRadius = Mathf.Sqrt(Mathf.Pow(a * gridSize, 2) + Mathf.Pow(b * gridSize, 2) - 2 * a * b * gridSize * gridSize * Mathf.Cos(2 * Mathf.PI / 3)) + (gridSize / 4f);
	}

    int n_ligands(E_prefab prefab)
	{
        int n = 0;
        foreach (Bond bond in prefab.ligands)
        {
            n += bond.strenght;
        }

        return n;
    }
}
