using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    private PlayerController controller;

    private void Awake() => controller = GetComponent<PlayerController>();

    private void OnEnable()
    {
        controller.OnSlide += Player_OnSlide;
        controller.OnJump += CancelSlide;
        controller.OnMoveLeft += CancelSlide;
        controller.OnMoveRight += CancelSlide;
    }
    
    private void OnDisable()
    {
        controller.OnSlide -= Player_OnSlide;
        controller.OnJump -= CancelSlide;
        controller.OnMoveLeft -= CancelSlide;
        controller.OnMoveRight -= CancelSlide;
    }

    private void Player_OnSlide()
    {
        StartCoroutine("VisualSlideRoutine");
    }

    private void CancelSlide()
    {
        StopCoroutine("VisualSlideRoutine");
        visual.transform.localRotation = Quaternion.identity;
    }

    private IEnumerator VisualSlideRoutine()
    {
        visual.transform.Rotate(-15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(-15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(-15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(-15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(-15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(-15, 0, 0);
        yield return new WaitForFixedUpdate();


        yield return new WaitForSeconds(controller.SlidingTime);

        visual.transform.Rotate(15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(15, 0, 0);
        yield return new WaitForFixedUpdate();
        visual.transform.Rotate(15, 0, 0);
        yield return new WaitForFixedUpdate();
    }
}
