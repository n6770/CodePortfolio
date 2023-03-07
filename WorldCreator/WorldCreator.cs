using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Sirenix.OdinInspector;

public class WorldCreator : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float waterLevel;
    [SerializeField] private float grassLevel;
    [SerializeField] private float mountainLevel;
    [SerializeField] private float snowLevel;
    [SerializeField] private int mapSize;
    private Vector2 mapSizeVector;
    [SerializeField] private AnimationCurve heightCurve;
    
    [Header("Meshes")]
    [SerializeField] private Transform waterMesh;
    [Range(1, 8), SerializeField] private int landMeshSubdivisions;
    [SerializeField] private float heightOffset;
    [SerializeField] private Material landMaterial;
    private float[,] heightMap;
    private Mesh groundMesh;
    private MeshRenderer meshRenderer;

    [Header("Island")]
    [SerializeField] private bool useIsland;
    [SerializeField] private float edgeOffset;
    [SerializeField] private float islandClamp;
    [SerializeField] private Texture2D gradientTexture;

    [Header("Perlin noise")]
    [SerializeField] private float startScale;
    [Range(1, 10), SerializeField] private int octaves; 
    [SerializeField] private float amplitudeMultiplier;
    [SerializeField] private float frequencyMultiplier;
    [SerializeField] private float weightMultiplier;

    [Header("Texture")]
    [Range(1, 20), SerializeField] private int textureScaling;
    [SerializeField] private Color sandColor;
    [SerializeField] private Color grassColor;
    [SerializeField] private Color mountainColor;
    [SerializeField] private Color snowColor;
    [SerializeField] private Texture2D noiseTexture;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            useIsland = false;
            CreateWorld();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            useIsland = true;
            CreateWorld();
        }

    }

    [Button]
    public void CreateWorld()
    {
        mapSizeVector = new Vector2(mapSize, mapSize);
        CreateMap();
        CreateLandMesh();
        SetWaterLevel();
        CreateTexture();
        FindObjectOfType<WaterMeshGen>().CreateWaterMesh(mapSizeVector);
    }

    public void CreateMap()
    {
        heightMap = new float[(int)mapSizeVector.x * landMeshSubdivisions + 1, (int)mapSizeVector.y * landMeshSubdivisions + 1];

        PerlinNoiseMap();
        if (useIsland) IslandGradient();
        ApplyCurve();
        FinalMap();
    }
    private void PerlinNoiseMap()
    {
        //EasyNoiseGenerator
        Noise noise = new Noise(Random.Range(0, 100000));
        Channel channel1 = new Channel("Perlin", Algorithm.Perlin3d, startScale, NoiseStyle.Linear, 0, 1f, Edge.Smooth);
        channel1.setFractal(octaves, frequencyMultiplier, amplitudeMultiplier);
        noise.addChannel(channel1);

        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                float height = noise.getNoise(new Vector3(x, y, 0f), "Perlin");
                heightMap[x, y] = height;
            }
        }
    }
    private void IslandGradient()
    {
        float[,] gradient = new float[(int)mapSizeVector.x * landMeshSubdivisions + 1, (int)mapSizeVector.y * landMeshSubdivisions + 1];


        Vector2 midPoint = new Vector2(gradient.GetLength(0) / 2, gradient.GetLength(1) / 2);
        float multiplier = Vector2.Distance(Vector2.zero, midPoint);

        for (int x = 0; x < gradient.GetLength(0); x++)
        {
            for (int y = 0; y < gradient.GetLength(1); y++)
            {
                float result = Vector2.Distance(new Vector2(x, y), midPoint) / multiplier;
                gradient[x, y] = 1f - result; //Invert
                gradient[x, y] = Mathf.Clamp(gradient[x, y] - edgeOffset, 0f, islandClamp);
            }
        }

        //visuaalista debuggia varten
        gradientTexture = new Texture2D(gradient.GetLength(0), gradient.GetLength(1), TextureFormat.RGBA32, false);
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                float colorValue = gradient[x, y];
                Color pixelColor = new Color(colorValue, colorValue, colorValue);
                gradientTexture.SetPixel(x, y, pixelColor);
            }
        }
        gradientTexture.Apply();

        float max = 0f;

        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                heightMap[x, y] = gradient[x, y] * heightMap[x, y];

                if (heightMap[x, y] > max) max = heightMap[x, y];
            }
        }

        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                heightMap[x, y] = heightMap[x, y] / max;
            }
        }
    }
    private void ApplyCurve()
    {
        //samplataan noise curveen
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                float height = heightCurve.Evaluate(heightMap[x, y]);
                heightMap[x, y] = height;
            }
        }
    }
    private void FinalMap()
    {
        //luodaan lopullinen noise grayscale tekstuuri
        noiseTexture = new Texture2D((int)heightMap.GetLength(0), (int)heightMap.GetLength(1), TextureFormat.RGBA32, false);

        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                float colorValue = heightMap[x, y];
                Color pixelColor = new Color(colorValue, colorValue, colorValue);
                noiseTexture.SetPixel(x, y, pixelColor);
            }
        }
        noiseTexture.Apply();
    }
    private void CreateLandMesh()
    {
        //uusi tyhjä mesh
        groundMesh = new Mesh { name = "Land Mesh" };
        groundMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshRenderer = GetComponent<MeshRenderer>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        //Luodaan vertexit listaan.
        //mapsize + 1 jotta saadaan quadit aikaan.
        for (int z = 0; z < mapSizeVector.y * landMeshSubdivisions + 1; z++)
        {
            for (int x = 0; x < mapSizeVector.x * landMeshSubdivisions + 1; x++)
            {
                vertices.Add(new Vector3(x, heightMap[x, z] * heightOffset, z) / landMeshSubdivisions); //Tässä final korkeus
                uvs.Add(new Vector2(x, z) / landMeshSubdivisions);
            }
        }

        //lasketaan kolmiot, kaksi kolmiota per quad.
        //myötäpäivään jotta facet asettuu oikein päin.
        for (int z = 0; z < mapSizeVector.y * landMeshSubdivisions; z++)
        {
            for (int x = 0; x < mapSizeVector.x * landMeshSubdivisions; x++)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i == 0)
                    {
                        triangles.Add((z * (int)mapSizeVector.y * landMeshSubdivisions) + x + z);
                        triangles.Add(((z + 1) * (int)mapSizeVector.y * landMeshSubdivisions) + x + 1 + z);
                        triangles.Add((z * (int)mapSizeVector.y * landMeshSubdivisions) + (x + 1) + z);
                    }
                    else
                    {
                        triangles.Add(z * ((int)mapSizeVector.y * landMeshSubdivisions) + x + 1 + z);
                        triangles.Add((z + 1) * ((int)mapSizeVector.y * landMeshSubdivisions) + x + 1 + z);
                        triangles.Add((z + 1) * ((int)mapSizeVector.y * landMeshSubdivisions) + x + 2 + z);
                    }
                }
            }
        }

        //asetetaan uudet luodut listat meshiin
        groundMesh.vertices = vertices.ToArray();
        groundMesh.triangles = triangles.ToArray();
        groundMesh.uv = uvs.ToArray();

        GetComponent<MeshFilter>().mesh = groundMesh;
        meshRenderer.material = landMaterial;
    }
    private void SetWaterLevel()
    {
        var waterPos = waterMesh.position;
        waterPos.y = waterLevel * heightOffset;
        waterMesh.position = waterPos;
    }
    private void CreateTexture()
    {
        //TODO: kokonaan uusiksi

        Texture2D texture = new Texture2D((int)mapSizeVector.x * landMeshSubdivisions * textureScaling + 1, (int)mapSizeVector.y * landMeshSubdivisions * textureScaling +1, TextureFormat.RGBA32, false);
        
        for (int i = 0; i < heightMap.GetLength(0) -1; i++)
        {
            for (int j = 0; j < heightMap.GetLength(1) -1; j++)
            {
                for (int k = 0; k < textureScaling; k++)
                {
                    for (int l = 0; l < textureScaling; l++)
                    {


                        float xOffset = 1f / textureScaling * k;
                        float yOffset = 1f / textureScaling * l;
                        float up = heightMap[i, j + 1] - heightMap[i, j];
                        float right = heightMap[i + 1, j] - heightMap[i, j];

                        float evaluate = heightMap[i, j] + (((up * yOffset) + (right * xOffset)) / 2);

                        Color pixelColor;
                        if (evaluate > snowLevel) pixelColor = snowColor;
                        else if (evaluate > mountainLevel) pixelColor = mountainColor;
                        else if (evaluate > grassLevel) pixelColor = grassColor;
                        else pixelColor = sandColor;
                        texture.SetPixel(i * textureScaling + k, j * textureScaling + l, pixelColor);
                    }
                }
            }
        }
        texture.Apply();
        meshRenderer.sharedMaterial.SetTexture("_baseMap", texture);
        meshRenderer.sharedMaterial.SetVector("_tiling", new Vector2(1 / mapSizeVector.x, 1 / mapSizeVector.y));
    }
}
