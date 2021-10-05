using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplates;
    public float terrainTemplateWidth;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;

    private const float debugLineHeight = 10.0f;

    private List<GameObject> spawnedTerrain;

    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    private float lastGeneratedPositionX;
    private float lastRemovedPositionX;

    // Pool List
    private Dictionary<string, List<GameObject>> pool;

    // Start is called before the first frame update
    void Start()
    {
        // Init pool
        pool = new Dictionary<string, List<GameObject>>();

        spawnedTerrain = new List<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGeneratedPositionX - terrainTemplateWidth;

        foreach (TerrainTemplateController terrain in earlyTerrainTemplates)
        {
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        while (lastGeneratedPositionX < GetHorizontalPositionEnd())
        {
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        while (lastRemovedPositionX+terrainTemplateWidth < GetHorizontalPositionStart())
        {
            lastRemovedPositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovedPositionX);
        }
    }

    // Pool Function
    private GameObject GenerateFromPool(GameObject item, Transform parent)
    {
        if (pool.ContainsKey(item.name))
        {
            // Jika item tersedia dalam pool
            if (pool[item.name].Count > 0)
            {
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);

                return newItemFromPool;
            }
        }
        else
        {
            // Jika list item tidak teridentifikasi, maka buatlah satu
            pool.Add(item.name, new List<GameObject>());
        }

        // Buat satu yang baru jika item tidak tersedia dalam pool
        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }
    
    private void ReturnToPool(GameObject item)
    {
        if (!pool.ContainsKey(item.name))
        {
            Debug.LogError(("INVALID POOL ITEM!!!"));
        }

        pool[item.name].Add(item);
        item.SetActive(false);
    }

    //private void GenerateTerrain(float posX, TerrainTemplateController forceterrain=null)
    //{
        //GameObject newTerrain = Instantiate(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
        //newTerrain.transform.position = new Vector2(posX, 0f);
        //spawnedTerrain.Add(newTerrain);
    //}

    private void GenerateTerrain(float posX, TerrainTemplateController forceTerrain=null)
    {
        GameObject newTerrain = null;
        
        if (forceTerrain == null)
        {
            newTerrain = GenerateFromPool(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
        }
        else
        {
            newTerrain = GenerateFromPool(forceTerrain.gameObject, transform);
        }

        newTerrain.transform.position = new Vector2(posX, 0f);
        spawnedTerrain.Add(newTerrain);
    }

    private void RemoveTerrain(float posX)
    {
        GameObject terrainToRemove = null;

        // Temukan terrain di posX
        foreach (GameObject item in spawnedTerrain)
        {
            if (item.transform.position.x == posX)
            {
                terrainToRemove = item;
                break;
            }
        }

        // Setelah terrain ditemukan
        if (terrainToRemove != null)
        {
            spawnedTerrain.Remove(terrainToRemove);
            //Destroy(terrainToRemove);
            ReturnToPool(terrainToRemove);
        }
    }

    private float GetHorizontalPositionStart()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd()
    {
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }

    // Debug
    private void OnDrawGizmos() {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition+Vector3.up*debugLineHeight/2, areaStartPosition+Vector3.down*debugLineHeight/2, Color.red);
        Debug.DrawLine(areaEndPosition+Vector3.up*debugLineHeight/2, areaEndPosition+Vector3.down*debugLineHeight/2, Color.red);
    }
}

