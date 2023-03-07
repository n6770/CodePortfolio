using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterMeshGen : MonoBehaviour
{
    [SerializeField] private Material waterMaterial;

    public void CreateWaterMesh(Vector2 mapSize)
    {
        //uusi tyhjä mesh
        var mesh = new Mesh { name = "Water Mesh" };

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        //Luodaan vertexit listaan.
        //mapsize + 1 jotta saadaan quadit aikaan.
        for (int z = 0; z < mapSize.y + 1; z++)
        {
            for (int x = 0; x < mapSize.x + 1; x++)
            {
                vertices.Add(new Vector3(x, 0, z));
                uvs.Add(new Vector2(x, z));
            }
        }

        //lasketaan kolmiot, kaksi kolmiota per quad.
        //myötäpäivään jotta facet asettuu oikein päin.

        //FOR LOOP HELL
        for (int z = 0; z < mapSize.y; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        triangles.Add((z * (int)mapSize.y) + x + z);
                        triangles.Add(((z + 1) * (int)mapSize.y) + x + 1 + z);
                        triangles.Add((z * (int)mapSize.y) + (x + 1) + z);
                    }
                    else
                    {
                        triangles.Add(z * (int)mapSize.y + x + 1 + z);
                        triangles.Add((z + 1) * (int)mapSize.y + x + 1 + z);
                        triangles.Add((z + 1) * (int)mapSize.y + x + 2 + z);
                    }
                }
            }
        }

        //asetetaan uudet luodut listat meshiin
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        GetComponent<MeshFilter>().mesh = mesh; 
        GetComponent<MeshRenderer>().material = waterMaterial;
    }

}
