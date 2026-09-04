using UnityEngine;
using UnityEngine.UI;

public class BallController : MonoBehaviour
{
    public Rigidbody rb;


    [Header("Drag Movement")]
    public float dragSpeed = 0.03f;
    public float maxXPosition = 5f;
    public float smoothMove = 12f;



    [Header("Release Roll")]
    public float rollSpeed = 5f;


    public Slider powerSlider;



    [Header("Slope Direction")]
    public Transform releaseDirection;



    [Header("Input Zone")]
    public RectTransform inputZone;



    private bool dragging;

    private Vector2 lastInputPosition;

    private Vector3 targetPosition;




    void Start()
    {
        targetPosition = transform.position;
    }





    void Update()
    {
        HandleInput();


        if(dragging)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothMove * Time.deltaTime
            );
        }
    }






    void HandleInput()
    {


        // Mouse Down

        if(Input.GetMouseButtonDown(0))
        {
            if(IsInsideZone(Input.mousePosition))
            {
                StartDrag();

                lastInputPosition = Input.mousePosition;
            }
        }





        // Mouse Drag

        if(Input.GetMouseButton(0) && dragging)
        {

            Vector2 currentPosition = Input.mousePosition;


            Vector2 delta = currentPosition - lastInputPosition;


            MoveBall(delta.x);


            lastInputPosition = currentPosition;

        }





        // Mouse Release

        if(Input.GetMouseButtonUp(0))
        {
            if(dragging)
            {
                ReleaseBall();
            }
        }






        // Touch

        if(Input.touchCount > 0)
        {

            Touch touch = Input.GetTouch(0);



            if(touch.phase == TouchPhase.Began)
            {
                if(IsInsideZone(touch.position))
                {
                    StartDrag();

                    lastInputPosition = touch.position;
                }
            }





            if(touch.phase == TouchPhase.Moved && dragging)
            {

                Vector2 delta = touch.position - lastInputPosition;


                MoveBall(delta.x);


                lastInputPosition = touch.position;

            }






            if(touch.phase == TouchPhase.Ended)
            {

                if(dragging)
                {
                    ReleaseBall();
                }

            }

        }

    }








    void StartDrag()
    {

        dragging = true;


        // Disable physics while controlling
        rb.isKinematic = true;


        targetPosition = transform.position;

    }







    void MoveBall(float input)
    {

        targetPosition.x += input * dragSpeed;



        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            -maxXPosition,
            maxXPosition
        );

    }









    void ReleaseBall()
    {

        dragging = false;



        // Enable physics
        rb.isKinematic = false;




        // Remove any jump velocity

        Vector3 currentVelocity = rb.linearVelocity;


        currentVelocity.y = 0;


        rb.linearVelocity = currentVelocity;





        float power = 1f;


        if(powerSlider != null)
        {
            power = powerSlider.value;
        }






        if(releaseDirection != null)
        {

            Vector3 direction = releaseDirection.forward;



            // Remove vertical direction
            direction.y = 0;



            direction.Normalize();





            float speed = power * rollSpeed;




            Vector3 velocity = rb.linearVelocity;



            velocity.x = direction.x * speed;

            velocity.z = direction.z * speed;

            velocity.y = 0;




            rb.linearVelocity = velocity;

        }

    }









    void FixedUpdate()
    {

        // Safety: prevent flying

        Vector3 velocity = rb.linearVelocity;


        if(velocity.y > 0)
        {
            velocity.y = 0;

            rb.linearVelocity = velocity;
        }

    }









    bool IsInsideZone(Vector2 pos)
    {

        return RectTransformUtility.RectangleContainsScreenPoint(
            inputZone,
            pos
        );

    }

}