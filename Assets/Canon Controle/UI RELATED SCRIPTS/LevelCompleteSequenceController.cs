using System.Collections;
using System;
using UnityEngine;


public sealed class LevelCompleteSequenceController : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField]
    private PopupGameplayVisibilityController gameplayVisibilityController;



    [Header("Particle Parent")]
    [SerializeField]
    private GameObject particleParent;



    [Header("First Particle Group")]
    [SerializeField]
    private ParticleSystem particleOne;

    [SerializeField]
    private ParticleSystem particleTwo;



    [Header("Second Particle Group")]
    [SerializeField]
    private ParticleSystem particleThree;

    [SerializeField]
    private ParticleSystem particleFour;



    [Header("Timing")]
    [SerializeField]
    private float firstParticleDuration = 1.5f;


    [SerializeField]
    private float secondParticleDuration = 1.5f;


    [SerializeField]
    private float popupDelay = 0.2f;



    [Header("Level Complete Popup")]
    [SerializeField]
    private GameObject levelCompletePopup;



    private Coroutine sequenceRoutine;
    private Action sequenceCompleted;



    private void Awake()
    {
        if(levelCompletePopup != null)
        {
            levelCompletePopup.SetActive(false);
        }


        if(particleParent != null)
        {
            particleParent.SetActive(false);
        }
    }



    public void PlayLevelCompleteSequence(
        Action onSequenceCompleted = null)
    {
        StopSequence();

        sequenceCompleted = onSequenceCompleted;


        sequenceRoutine =
            StartCoroutine(
                LevelCompleteRoutine()
            );
    }




    private IEnumerator LevelCompleteRoutine()
    {
        if(levelCompletePopup != null)
        {
            UITransition.HideImmediate(
                levelCompletePopup
            );
        }

        // Hide gameplay
        if(gameplayVisibilityController != null)
        {
            gameplayVisibilityController.HideGameplay();
        }



        // Enable particle screen
        if(particleParent != null)
        {
            particleParent.SetActive(true);
        }



        // First 2 particles
        PlayParticle(
            particleOne
        );

        PlayParticle(
            particleTwo
        );


        yield return WaitForUnscaledSeconds(
            firstParticleDuration
        );



        // Second 2 particles
        PlayParticle(
            particleThree
        );

        PlayParticle(
            particleFour
        );


        yield return WaitForUnscaledSeconds(
            secondParticleDuration
        );


        StopParticle(particleOne);
        StopParticle(particleTwo);
        StopParticle(particleThree);
        StopParticle(particleFour);



        // Disable particle screen
        if(particleParent != null)
        {
            particleParent.SetActive(false);
        }



        yield return WaitForUnscaledSeconds(
            popupDelay
        );


        Action completedCallback =
            sequenceCompleted;

        sequenceCompleted = null;
        sequenceRoutine = null;


        // LevelCompleteUIController ka proper popup setup/pause use karo.
        if(completedCallback != null)
        {
            completedCallback.Invoke();
        }
        else if(levelCompletePopup != null)
        {
            UITransition.Show(
                levelCompletePopup
            );
        }
    }





    private void PlayParticle(
        ParticleSystem particle)
    {
        if(particle == null)
        {
            return;
        }


        particle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );


        particle.Play();
    }


    private static void StopParticle(
        ParticleSystem particle)
    {
        if(particle == null)
        {
            return;
        }

        particle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }


    private static IEnumerator WaitForUnscaledSeconds(
        float duration)
    {
        float elapsed = 0f;

        while(elapsed < Mathf.Max(0f, duration))
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }


    public void StopSequence()
    {
        if(sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        sequenceCompleted = null;

        StopParticle(particleOne);
        StopParticle(particleTwo);
        StopParticle(particleThree);
        StopParticle(particleFour);

        if(particleParent != null)
        {
            particleParent.SetActive(false);
        }
    }


    private void OnDisable()
    {
        StopSequence();
    }
}
