// using System.Collections.Generic;
// using UnityEngine;

// public sealed class PopupGameplayVisibilityController : MonoBehaviour
// {
//     [Header("Gameplay References")]
//     [SerializeField] private CannonController cannonController;
//     [SerializeField] private LevelRuntimeController levelRuntimeController;


//     private readonly List<GameObject> hiddenTowerObjects =
//         new List<GameObject>();


//     private bool gameplayHidden;


//     private void Awake()
//     {
//         ResolveReferences();
//     }


//     private void ResolveReferences()
//     {
//         if (cannonController == null)
//         {
//             cannonController =
//                 FindFirstObjectByType<CannonController>(
//                     FindObjectsInactive.Include
//                 );
//         }

//         if (levelRuntimeController == null)
//         {
//             levelRuntimeController =
//                 FindFirstObjectByType<LevelRuntimeController>(
//                     FindObjectsInactive.Include
//                 );
//         }
//     }


//     public void HideGameplay()
//     {
//         if (gameplayHidden)
//         {
//             return;
//         }

//         ResolveReferences();

//         gameplayHidden = true;

//         HideCannon();
//         HideTables();
//         HideTowerObjects();
//     }


//     public void ShowGameplay()
//     {
//         if (!gameplayHidden)
//         {
//             return;
//         }

//         ResolveReferences();

//         gameplayHidden = false;

//         ShowCannon();
//         ShowTables();
//         ShowTowerObjects();
//     }


//     private void HideCannon()
//     {
//         if (cannonController == null)
//         {
//             return;
//         }

//         cannonController.SetGameplayActive(false);
//     }


//     private void ShowCannon()
//     {
//         if (cannonController == null)
//         {
//             return;
//         }

//         cannonController.SetGameplayActive(true);
//     }


//     private void HideTables()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         IReadOnlyList<LevelTable> tables =
//             levelRuntimeController.CurrentTables;

//         if (tables == null)
//         {
//             return;
//         }

//         for (int i = 0;
//              i < tables.Count;
//              i++)
//         {
//             LevelTable table =
//                 tables[i];

//             if (table != null &&
//                 table.gameObject.activeSelf)
//             {
//                 table.gameObject.SetActive(false);
//             }
//         }
//     }


//     private void ShowTables()
//     {
//         if (levelRuntimeController == null)
//         {
//             return;
//         }

//         IReadOnlyList<LevelTable> tables =
//             levelRuntimeController.CurrentTables;

//         if (tables == null)
//         {
//             return;
//         }

//         for (int i = 0;
//              i < tables.Count;
//              i++)
//         {
//             LevelTable table =
//                 tables[i];

//             if (table != null)
//             {
//                 table.gameObject.SetActive(true);
//             }
//         }
//     }


//     private void HideTowerObjects()
//     {
//         hiddenTowerObjects.Clear();

//         PhysicsTowerObject[] towerObjects =
//             FindObjectsByType<PhysicsTowerObject>(
//                 FindObjectsInactive.Include,
//                 FindObjectsSortMode.None
//             );

//         for (int i = 0;
//              i < towerObjects.Length;
//              i++)
//         {
//             PhysicsTowerObject towerObject =
//                 towerObjects[i];

//             if (towerObject == null)
//             {
//                 continue;
//             }

//             GameObject towerGameObject =
//                 towerObject.gameObject;

//             if (!towerGameObject.activeInHierarchy)
//             {
//                 continue;
//             }

//             hiddenTowerObjects.Add(
//                 towerGameObject
//             );

//             towerGameObject.SetActive(false);
//         }
//     }


//     private void ShowTowerObjects()
//     {
//         for (int i = 0;
//              i < hiddenTowerObjects.Count;
//              i++)
//         {
//             GameObject towerObject =
//                 hiddenTowerObjects[i];

//             if (towerObject != null)
//             {
//                 towerObject.SetActive(true);
//             }
//         }

//         hiddenTowerObjects.Clear();
//     }
// }






using System.Collections.Generic;
using UnityEngine;

public sealed class PopupGameplayVisibilityController : MonoBehaviour
{
    [Header("Gameplay References")]
    [SerializeField] private CannonController cannonController;
    [SerializeField] private LevelRuntimeController levelRuntimeController;


    /*
     * Sirf woh tower objects store honge jo popup open hote waqt
     * actually active thay.
     *
     * Is se pooled / already inactive objects ko ShowGameplay()
     * galti se active nahi karega.
     */
    private readonly List<GameObject> hiddenTowerObjects =
        new List<GameObject>();


    private bool gameplayHidden;


    private void Awake()
    {
        ResolveReferences();
    }


    private void ResolveReferences()
    {
        if (cannonController == null)
        {
            cannonController =
                FindFirstObjectByType<CannonController>(
                    FindObjectsInactive.Include
                );
        }

        if (levelRuntimeController == null)
        {
            levelRuntimeController =
                FindFirstObjectByType<LevelRuntimeController>(
                    FindObjectsInactive.Include
                );
        }
    }


    public void HideGameplay()
    {
        if (gameplayHidden)
        {
            return;
        }

        ResolveReferences();

        gameplayHidden = true;

        /*
         * Popup / doosri gameplay screen open hote hi jo cannon balls
         * pehle fire ho chuki hain unko foran remove kar do.
         *
         * SetActive(false) pehle kar rahe hain taake Destroy end-of-frame
         * hone se pehle bhi ball screen par nazar na aaye.
         */
        DestroyActiveShotBalls();
        DestroyActiveChainReactionVfx();

        HideCannon();
        HideTables();
        HideTraps();
        HideTowerObjects();
    }


    public void ShowGameplay()
    {
        if (!gameplayHidden)
        {
            return;
        }

        ResolveReferences();

        gameplayHidden = false;

        ShowCannon();
        ShowTables();
        ShowTraps();
        ShowTowerObjects();
    }


    private void DestroyActiveShotBalls()
    {
        /*
         * CannonController har fired cannon ball par
         * LowerGroundDisappearEffect component ensure karta hai.
         *
         * Isliye CannonController ke private active-ball list ko touch
         * kiye baghair independent tarike se runtime shot balls find
         * aur destroy kar sakte hain.
         */
        LowerGroundDisappearEffect[] shotEffects =
            FindObjectsByType<LowerGroundDisappearEffect>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < shotEffects.Length;
             i++)
        {
            LowerGroundDisappearEffect shotEffect =
                shotEffects[i];

            if (shotEffect == null)
            {
                continue;
            }

            GameObject shotObject =
                shotEffect.gameObject;

            if (shotObject == null)
            {
                continue;
            }

            /*
             * Cannon ball prefab ke liye Rigidbody required hai.
             * Ye extra guard kisi unrelated object ko accidentally
             * destroy hone se bachata hai.
             */
            Rigidbody shotBody =
                shotObject.GetComponent<Rigidbody>();

            if (shotBody == null)
            {
                continue;
            }

            /*
             * Tower pieces ko kabhi projectile samajh kar destroy na karo.
             */
            if (shotObject.GetComponent<PhysicsTowerObject>() != null ||
                shotObject.GetComponentInParent<PhysicsTowerObject>() != null)
            {
                continue;
            }

            /*
             * Cannon ke apne hierarchy object ko touch na karo.
             */
            if (cannonController != null &&
                shotObject.transform.IsChildOf(
                    cannonController.transform))
            {
                continue;
            }

            shotObject.SetActive(false);
            Destroy(shotObject);
        }
    }


    /// <summary>
    /// Chain-reaction blast ka VFX popup ke saath screen par nahi rehna
    /// chahiye.
    ///
    /// Fired balls aur break fragments par Rigidbody +
    /// LowerGroundDisappearEffect hota hai, is liye DestroyActiveShotBalls()
    /// unhein pehle se pakad leta hai. Blast VFX sirf ek ParticleSystem
    /// hai (na Rigidbody, na wo effect), is liye usay
    /// ChainReactionVfxMarker se alag se dhoondte hain.
    ///
    /// Balls ki tarah isay bhi destroy karte hain, chhupa kar wapas nahi
    /// laate - warna resume par ek adhoora, jama hua dhamaka phir se
    /// nazar aata.
    /// </summary>
    private void DestroyActiveChainReactionVfx()
    {
        ChainReactionVfxMarker[] activeVfx =
            FindObjectsByType<ChainReactionVfxMarker>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < activeVfx.Length;
             i++)
        {
            ChainReactionVfxMarker vfx =
                activeVfx[i];

            if (vfx == null)
            {
                continue;
            }

            GameObject vfxObject =
                vfx.gameObject;

            if (vfxObject == null)
            {
                continue;
            }

            /*
             * SetActive(false) pehle, taake end-of-frame Destroy se
             * pehle bhi ye frame par nazar na aaye - wahi tarteeb jo
             * DestroyActiveShotBalls() use karta hai.
             */
            vfxObject.SetActive(false);
            Destroy(vfxObject);
        }
    }


    private void HideCannon()
    {
        if (cannonController == null)
        {
            return;
        }

        /*
         * CannonController ka existing visibility method use kar rahe hain.
         * Iske shooting logic ko touch nahi kar rahe.
         */
        cannonController.SetGameplayActive(false);
    }


    private void ShowCannon()
    {
        if (cannonController == null)
        {
            return;
        }

        cannonController.SetGameplayActive(true);
    }


    private void HideTables()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        IReadOnlyList<LevelTable> tables =
            levelRuntimeController.CurrentTables;

        if (tables == null)
        {
            return;
        }

        for (int i = 0;
             i < tables.Count;
             i++)
        {
            LevelTable table =
                tables[i];

            if (table != null &&
                table.gameObject.activeSelf)
            {
                table.gameObject.SetActive(false);
            }
        }
    }


    private void ShowTables()
    {
        if (levelRuntimeController == null)
        {
            return;
        }

        IReadOnlyList<LevelTable> tables =
            levelRuntimeController.CurrentTables;

        if (tables == null)
        {
            return;
        }

        for (int i = 0;
             i < tables.Count;
             i++)
        {
            LevelTable table =
                tables[i];

            if (table != null)
            {
                table.gameObject.SetActive(true);
            }
        }
    }


    private void HideTraps()
    {
        if (levelRuntimeController != null)
        {
            levelRuntimeController.SetTrapsVisible(false);
        }
    }


    private void ShowTraps()
    {
        if (levelRuntimeController != null)
        {
            levelRuntimeController.SetTrapsVisible(true);
        }
    }


    private void HideTowerObjects()
    {
        hiddenTowerObjects.Clear();

        /*
         * Important:
         * runtimeObjectsRoot par depend nahi kar rahe.
         *
         * Actual active PhysicsTowerObject instances find karke
         * unhi ko hide karte hain. Isliye chahe pooling system
         * unko kisi aur parent ke andar rakhe, tower phir bhi hide hoga.
         */
        PhysicsTowerObject[] towerObjects =
            FindObjectsByType<PhysicsTowerObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < towerObjects.Length;
             i++)
        {
            PhysicsTowerObject towerObject =
                towerObjects[i];

            if (towerObject == null)
            {
                continue;
            }

            GameObject towerGameObject =
                towerObject.gameObject;

            /*
             * Sirf currently visible/active level pieces save karo.
             * Pooled inactive objects ko bilkul touch nahi karna.
             */
            if (!towerGameObject.activeInHierarchy)
            {
                continue;
            }

            hiddenTowerObjects.Add(
                towerGameObject
            );

            towerGameObject.SetActive(false);
        }
    }


    private void ShowTowerObjects()
    {
        /*
         * Sirf wahi objects restore honge jo HideGameplay()
         * ke waqt active thay.
         */
        for (int i = 0;
             i < hiddenTowerObjects.Count;
             i++)
        {
            GameObject towerObject =
                hiddenTowerObjects[i];

            if (towerObject == null)
            {
                continue;
            }

            towerObject.SetActive(true);

            /*
             * HideTowerObjects() ke SetActive(false) ne is block ki
             * shrink coroutine maar di thi, aur SetActive(true) usay
             * khud restart nahi karta. Is liye adhoora sequence yahan
             * manually resume karte hain, warna ground par gira hua
             * block hamesha ke liye khara reh jata hai aur kabhi
             * cleared count na hone ki wajah se level complete hi
             * nahi ho pata.
             */
            LowerGroundDisappearEffect disappearEffect =
                towerObject.GetComponent<LowerGroundDisappearEffect>();

            if (disappearEffect != null)
            {
                disappearEffect.ResumeIfInterrupted();
            }
        }

        hiddenTowerObjects.Clear();
    }
}







