//using System;
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public sealed class PhysicsTowerObject : MonoBehaviour
//{
//    public event Action<PhysicsTowerObject> Cleared;

//    [Header("Level")]
//    [SerializeField]
//    private bool countsAsTarget = true;

//    [Header("Physics")]
//    [SerializeField]
//    private Rigidbody body;

//    [SerializeField, Min(1)]
//    private int solverIterations = 8;

//    [SerializeField, Min(1)]
//    private int solverVelocityIterations = 2;

//    private bool isCleared;

//    public bool CountsAsTarget => countsAsTarget;
//    public bool IsCleared => isCleared;

//    private void Awake()
//    {
//        if (body == null)
//        {
//            body = GetComponent<Rigidbody>();
//        }
//    }

//    public void PrepareForSpawn()
//    {
//        isCleared = false;

//        if (body == null)
//            return;

//        body.isKinematic = false;
//        body.useGravity = true;
//        body.detectCollisions = true;

//        body.solverIterations =
//            solverIterations;

//        body.solverVelocityIterations =
//            solverVelocityIterations;

//        SetVelocity(Vector3.zero);

//        body.angularVelocity =
//            Vector3.zero;

//        body.WakeUp();
//    }

//    public void PrepareForPool()
//    {
//        isCleared = true;

//        if (body == null)
//            return;

//        SetVelocity(Vector3.zero);

//        body.angularVelocity =
//            Vector3.zero;

//        body.useGravity = false;
//        body.isKinematic = true;

//        body.Sleep();
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (isCleared || other == null)
//            return;

//        LevelClearZone clearZone =
//            other.GetComponentInParent<LevelClearZone>();

//        if (clearZone == null)
//            return;

//        isCleared = true;

//        Cleared?.Invoke(this);
//    }

//    private void SetVelocity(Vector3 velocity)
//    {
//#if UNITY_6000_0_OR_NEWER
//        body.linearVelocity = velocity;
//#else
//        body.velocity = velocity;
//#endif
//    }
//}





using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class PhysicsTowerObject : MonoBehaviour
{
    public event Action<PhysicsTowerObject> Cleared;


    [Header("Level")]

    [SerializeField]
    private bool countsAsTarget = true;


    [Header("Physics")]

    [SerializeField]
    private Rigidbody body;

    [SerializeField, Min(1)]
    private int solverIterations = 8;

    [SerializeField, Min(1)]
    private int solverVelocityIterations = 2;


    [Header("Visual Color")]

    [Tooltip(
        "Empty ho to script automatically child Renderers find karegi. " +
        "Agar sirf specific mesh ko color karna ho to Renderer manually assign karein."
    )]
    [SerializeField]
    private Renderer[] colorRenderers;


    private Collider[] physicsColliders;

    private MaterialPropertyBlock
        colorPropertyBlock;

    private bool isCleared;


    private static readonly int
        BaseColorProperty =
            Shader.PropertyToID(
                "_BaseColor"
            );

    private static readonly int
        ColorProperty =
            Shader.PropertyToID(
                "_Color"
            );


    public bool CountsAsTarget =>
        countsAsTarget;

    public bool IsCleared =>
        isCleared;

    public Rigidbody Body =>
        body;


    private void Awake()
    {
        ResolveReferences();
    }


    private void ResolveReferences()
    {
        if (body == null)
        {
            body =
                GetComponent<Rigidbody>();
        }

        physicsColliders =
            GetComponentsInChildren<Collider>(
                true
            );

        if (colorRenderers == null ||
            colorRenderers.Length == 0)
        {
            colorRenderers =
                GetComponentsInChildren<Renderer>(
                    true
                );
        }

        if (colorPropertyBlock == null)
        {
            colorPropertyBlock =
                new MaterialPropertyBlock();
        }
    }


    public bool TryGetPhysicsBounds(
        out Bounds combinedBounds)
    {
        combinedBounds =
            default;

        if (physicsColliders == null ||
            physicsColliders.Length == 0)
        {
            ResolveReferences();
        }

        bool hasBounds =
            false;

        foreach (Collider targetCollider
                 in physicsColliders)
        {
            if (targetCollider == null ||
                !targetCollider.enabled ||
                targetCollider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds =
                    targetCollider.bounds;

                hasBounds =
                    true;
            }
            else
            {
                combinedBounds.Encapsulate(
                    targetCollider.bounds
                );
            }
        }

        return hasBounds;
    }


    /// <summary>
    /// Optimized color assignment.
    ///
    /// Renderer.material use nahi hota,
    /// isliye material instances create nahi hongi.
    /// </summary>
    public void SetVisualColor(
        Color color)
    {
        if (colorRenderers == null ||
            colorRenderers.Length == 0 ||
            colorPropertyBlock == null)
        {
            ResolveReferences();
        }

        if (colorRenderers == null)
        {
            return;
        }

        foreach (Renderer targetRenderer
                 in colorRenderers)
        {
            if (targetRenderer == null)
            {
                continue;
            }

            Material sharedMaterial =
                targetRenderer.sharedMaterial;

            if (sharedMaterial == null)
            {
                continue;
            }

            colorPropertyBlock.Clear();

            targetRenderer.GetPropertyBlock(
                colorPropertyBlock
            );

            /*
             * Unity URP Lit / Simple Lit
             */
            if (sharedMaterial.HasProperty(
                    BaseColorProperty))
            {
                colorPropertyBlock.SetColor(
                    BaseColorProperty,
                    color
                );
            }

            /*
             * Built-in Standard ya kuch
             * custom shaders.
             */
            if (sharedMaterial.HasProperty(
                    ColorProperty))
            {
                colorPropertyBlock.SetColor(
                    ColorProperty,
                    color
                );
            }

            targetRenderer.SetPropertyBlock(
                colorPropertyBlock
            );
        }
    }


    public void PrepareForSpawn()
    {
        isCleared =
            false;

        if (body == null)
        {
            ResolveReferences();
        }

        if (body == null)
        {
            return;
        }

        SetVelocity(
            Vector3.zero
        );

        body.angularVelocity =
            Vector3.zero;

        body.detectCollisions =
            true;

        body.isKinematic =
            false;

        body.useGravity =
            true;

        body.solverIterations =
            solverIterations;

        body.solverVelocityIterations =
            solverVelocityIterations;

        body.WakeUp();
    }


    public void PrepareForPool()
    {
        isCleared =
            true;

        /*
         * Old level ke event references
         * pooled object ke sath nahi rahenge.
         */
        Cleared =
            null;

        if (body == null)
        {
            ResolveReferences();
        }

        if (body == null)
        {
            return;
        }

        SetVelocity(
            Vector3.zero
        );

        body.angularVelocity =
            Vector3.zero;

        body.useGravity =
            false;

        body.isKinematic =
            true;

        body.Sleep();
    }


    private void OnTriggerEnter(
        Collider other)
    {
        if (isCleared ||
            other == null)
        {
            return;
        }

        LevelClearZone clearZone =
            other.GetComponentInParent<
                LevelClearZone>();

        if (clearZone == null)
        {
            return;
        }

        MarkCleared();
    }


    private void MarkCleared()
    {
        if (isCleared)
        {
            return;
        }

        isCleared =
            true;

        Cleared?.Invoke(
            this
        );
    }


    private void SetVelocity(
        Vector3 velocity)
    {
        if (body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity =
            velocity;
#else
        body.velocity =
            velocity;
#endif
    }


#if UNITY_EDITOR
    private void Reset()
    {
        body =
            GetComponent<Rigidbody>();

        physicsColliders =
            GetComponentsInChildren<Collider>(
                true
            );

        colorRenderers =
            GetComponentsInChildren<Renderer>(
                true
            );
    }
#endif
}







//using System;
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public sealed class PhysicsTowerObject : MonoBehaviour
//{
//    public event Action<PhysicsTowerObject> Cleared;

//    [Header("Level")]

//    [SerializeField]
//    private bool countsAsTarget = true;

//    [Header("Physics")]

//    [SerializeField]
//    private Rigidbody body;

//    [SerializeField, Min(1)]
//    private int solverIterations = 8;

//    [SerializeField, Min(1)]
//    private int solverVelocityIterations = 2;

//    private Collider[] physicsColliders;

//    private bool isCleared;

//    public bool CountsAsTarget =>
//        countsAsTarget;

//    public bool IsCleared =>
//        isCleared;

//    public Rigidbody Body =>
//        body;

//    private void Awake()
//    {
//        ResolveReferences();
//    }

//    private void ResolveReferences()
//    {
//        if (body == null)
//        {
//            body =
//                GetComponent<Rigidbody>();
//        }

//        physicsColliders =
//            GetComponentsInChildren<Collider>(
//                true
//            );
//    }

//    /// <summary>
//    /// Actual enabled non-trigger physics colliders
//    /// ki combined world-space bounds return karta hai.
//    ///
//    /// Generator isi se actual block width/height
//    /// automatically calculate karega.
//    /// </summary>
//    public bool TryGetPhysicsBounds(
//        out Bounds combinedBounds)
//    {
//        combinedBounds =
//            default;

//        if (physicsColliders == null ||
//            physicsColliders.Length == 0)
//        {
//            ResolveReferences();
//        }

//        bool hasBounds =
//            false;

//        foreach (Collider targetCollider
//                 in physicsColliders)
//        {
//            if (targetCollider == null ||
//                !targetCollider.enabled ||
//                targetCollider.isTrigger)
//            {
//                continue;
//            }

//            if (!hasBounds)
//            {
//                combinedBounds =
//                    targetCollider.bounds;

//                hasBounds =
//                    true;
//            }
//            else
//            {
//                combinedBounds.Encapsulate(
//                    targetCollider.bounds
//                );
//            }
//        }

//        return hasBounds;
//    }

//    /// <summary>
//    /// Pool se gameplay mein aate waqt
//    /// real physics enable hoti hai.
//    /// </summary>
//    public void PrepareForSpawn()
//    {
//        isCleared =
//            false;

//        if (body == null)
//        {
//            ResolveReferences();
//        }

//        if (body == null)
//        {
//            return;
//        }

//        SetVelocity(
//            Vector3.zero
//        );

//        body.angularVelocity =
//            Vector3.zero;

//        body.detectCollisions =
//            true;

//        body.isKinematic =
//            false;

//        body.useGravity =
//            true;

//        body.solverIterations =
//            solverIterations;

//        body.solverVelocityIterations =
//            solverVelocityIterations;

//        body.WakeUp();
//    }

//    /// <summary>
//    /// Object destroy nahi hota.
//    /// Pool mein return hota hai.
//    /// </summary>
//    public void PrepareForPool()
//    {
//        isCleared =
//            true;

//        Cleared =
//            null;

//        if (body == null)
//        {
//            ResolveReferences();
//        }

//        if (body == null)
//        {
//            return;
//        }

//        SetVelocity(
//            Vector3.zero
//        );

//        body.angularVelocity =
//            Vector3.zero;

//        body.useGravity =
//            false;

//        body.isKinematic =
//            true;

//        body.Sleep();
//    }

//    private void OnTriggerEnter(
//        Collider other)
//    {
//        if (isCleared ||
//            other == null)
//        {
//            return;
//        }

//        LevelClearZone clearZone =
//            other.GetComponentInParent<
//                LevelClearZone>();

//        if (clearZone == null)
//        {
//            return;
//        }

//        MarkCleared();
//    }

//    private void MarkCleared()
//    {
//        if (isCleared)
//        {
//            return;
//        }

//        isCleared =
//            true;

//        Cleared?.Invoke(
//            this
//        );
//    }

//    private void SetVelocity(
//        Vector3 velocity)
//    {
//        if (body == null)
//        {
//            return;
//        }

//#if UNITY_6000_0_OR_NEWER
//        body.linearVelocity =
//            velocity;
//#else
//        body.velocity =
//            velocity;
//#endif
//    }

//#if UNITY_EDITOR
//    private void Reset()
//    {
//        body =
//            GetComponent<Rigidbody>();

//        physicsColliders =
//            GetComponentsInChildren<Collider>(
//                true
//            );
//    }
//#endif
//}



