using System.Collections.Generic;
using UnityEngine;

public sealed class PhysicsObjectPool : MonoBehaviour
{
    private sealed class PoolBucket
    {
        public PhysicsObjectDefinition Definition;

        public readonly Stack<PhysicsTowerObject>
            Inactive =
                new Stack<PhysicsTowerObject>();

        public readonly HashSet<PhysicsTowerObject>
            InactiveLookup =
                new HashSet<PhysicsTowerObject>();

        public int TotalCreated;
    }


    [Header("Pool")]

    [SerializeField]
    private Transform poolRoot;


    private readonly Dictionary<
        PhysicsObjectDefinition,
        PoolBucket>
        pools =
            new Dictionary<
                PhysicsObjectDefinition,
                PoolBucket>();


    private readonly Dictionary<
        PhysicsTowerObject,
        PoolBucket>
        objectOwners =
            new Dictionary<
                PhysicsTowerObject,
                PoolBucket>();


    private void Awake()
    {
        if (poolRoot == null)
        {
            poolRoot =
                transform;
        }
    }


    public void PrepareForLevel(
        GridLevelData levelData)
    {
        if (levelData == null)
        {
            return;
        }


        IReadOnlyList<PhysicsObjectDefinition> palette =
            levelData.BlockPalette;


        if (palette == null ||
            palette.Count == 0)
        {
            Debug.LogError(
                "GridLevelData mein Block Palette empty hai.",
                levelData
            );

            return;
        }


        /*
         * Pehle har palette entry ko poori OccupiedCellCount tak prewarm
         * kiya jata tha - worst case, ye maan kar ke koi bhi shape har
         * cell par aa sakti hai. Chhote palette (2-3 shapes) par ye theek
         * tha, magar shape variety ke liye palette barhate hi ye hazaaron
         * objects bana deta hai (10 shapes x 100 cells = 1000).
         *
         * Ab grid scan kar ke har definition ka ASAL usage count nikalte
         * hain. Jo shape is level mein kahin use hi nahi hoti, uska ek
         * bhi object nahi banta - aur agar kabhi zarurat par gayi to
         * Get() waise bhi demand par naya bana leta hai.
         */
        Dictionary<int, int> usageByDefinitionIndex =
            CountDefinitionUsage(levelData);

        for (int index = 0;
             index < palette.Count;
             index++)
        {
            PhysicsObjectDefinition definition =
                palette[index];

            if (definition == null ||
                definition.Prefab == null)
            {
                continue;
            }

            int usage;

            if (!usageByDefinitionIndex.TryGetValue(
                    index,
                    out usage) ||
                usage <= 0)
            {
                continue;
            }

            int requiredCount =
                Mathf.Max(
                    usage,
                    definition.MinimumPrewarmCount
                );

            EnsureCapacity(
                definition,
                requiredCount
            );
        }
    }


    /// <summary>
    /// Har palette index ke liye ginti karta hai ke is level mein wo
    /// shape kitni dafa actually spawn hogi. Covered cells count nahi
    /// hotin - unka anchor cell hi spawn karta hai.
    /// </summary>
    private static Dictionary<int, int> CountDefinitionUsage(
        GridLevelData levelData)
    {
        Dictionary<int, int> usage =
            new Dictionary<int, int>();

        int tableCount =
            Mathf.Max(1, levelData.TableCount);

        for (int tableIndex = 0;
             tableIndex < tableCount;
             tableIndex++)
        {
            for (int y = 0; y < levelData.GridHeight; y++)
            {
                for (int z = 0; z < levelData.GridDepth; z++)
                {
                    for (int x = 0; x < levelData.GridWidth; x++)
                    {
                        GridCellData cell =
                            levelData.GetCell(x, y, z, tableIndex);

                        if (cell == null ||
                            !cell.Occupied ||
                            cell.IsCovered)
                        {
                            continue;
                        }

                        int current;

                        usage.TryGetValue(
                            cell.DefinitionIndex,
                            out current
                        );

                        usage[cell.DefinitionIndex] =
                            current + 1;
                    }
                }
            }
        }

        return usage;
    }


    public PhysicsTowerObject Get(
        PhysicsObjectDefinition definition,
        Vector3 position,
        Quaternion rotation,
        Transform parent)
    {
        if (definition == null ||
            definition.Prefab == null)
        {
            return null;
        }


        PoolBucket bucket =
            GetOrCreateBucket(
                definition
            );


        if (bucket.Inactive.Count == 0)
        {
            CreatePooledObject(
                bucket
            );
        }


        PhysicsTowerObject instance =
            bucket.Inactive.Pop();


        bucket.InactiveLookup.Remove(
            instance
        );


        Transform objectTransform =
            instance.transform;


        objectTransform.SetParent(
            parent,
            false
        );


        objectTransform.SetPositionAndRotation(
            position,
            rotation
        );


        objectTransform.localScale =
            definition.Prefab
                .transform
                .localScale;


        instance.PrepareForSpawn();


        instance.gameObject.SetActive(
            true
        );


        return instance;
    }


    public void Release(
        PhysicsTowerObject instance)
    {
        if (instance == null)
        {
            return;
        }


        if (!objectOwners.TryGetValue(
                instance,
                out PoolBucket bucket))
        {
            return;
        }


        if (bucket.InactiveLookup.Contains(
                instance))
        {
            return;
        }


        instance.PrepareForPool();


        instance.gameObject.SetActive(
            false
        );


        instance.transform.SetParent(
            poolRoot,
            false
        );


        bucket.Inactive.Push(
            instance
        );


        bucket.InactiveLookup.Add(
            instance
        );
    }


    /// <summary>
    /// Addressable level switch se pehle purane level definitions aur
    /// prefab instances ki tamam references release karta hai.
    /// </summary>
    public void ClearAll()
    {
        List<PhysicsTowerObject> instances =
            new List<PhysicsTowerObject>(objectOwners.Keys);

        foreach (PhysicsTowerObject instance in instances)
        {
            if (instance == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(instance.gameObject);
            }
            else
            {
                DestroyImmediate(instance.gameObject);
            }
        }

        pools.Clear();
        objectOwners.Clear();
    }


    private void EnsureCapacity(
        PhysicsObjectDefinition definition,
        int requiredCount)
    {
        PoolBucket bucket =
            GetOrCreateBucket(
                definition
            );


        while (bucket.TotalCreated <
               requiredCount)
        {
            CreatePooledObject(
                bucket
            );
        }
    }


    private PoolBucket GetOrCreateBucket(
        PhysicsObjectDefinition definition)
    {
        if (pools.TryGetValue(
                definition,
                out PoolBucket existing))
        {
            return existing;
        }


        PoolBucket bucket =
            new PoolBucket
            {
                Definition =
                    definition
            };


        pools.Add(
            definition,
            bucket
        );


        return bucket;
    }


    private void CreatePooledObject(
        PoolBucket bucket)
    {
        PhysicsTowerObject instance =
            Instantiate(
                bucket.Definition.Prefab,
                poolRoot
            );


        instance.name =
            bucket.Definition.Prefab.name +
            "_Pooled_" +
            bucket.TotalCreated;


        bucket.TotalCreated++;


        objectOwners.Add(
            instance,
            bucket
        );


        instance.PrepareForPool();


        instance.gameObject.SetActive(
            false
        );


        bucket.Inactive.Push(
            instance
        );


        bucket.InactiveLookup.Add(
            instance
        );
    }
}
