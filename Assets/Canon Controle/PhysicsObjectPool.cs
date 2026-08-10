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


        PhysicsObjectDefinition definition =
            levelData.ObjectDefinition;


        if (definition == null ||
            definition.Prefab == null)
        {
            Debug.LogError(
                "GridLevelData mein Object Definition missing hai.",
                levelData
            );

            return;
        }


        int requiredCount =
            Mathf.Max(
                levelData.BakedOccupiedCellCount,
                definition.MinimumPrewarmCount
            );


        EnsureCapacity(
            definition,
            requiredCount
        );
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