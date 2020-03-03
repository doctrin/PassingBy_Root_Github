using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBounds : MonoBehaviour
{

    public float minVisibleX;
    public float maxVisibleX;
    private float minValue;
    private float maxValue;
    public float cameraHalfWidth;

    public Camera activeCamera;
    public Transform cameraRoot;

    public Transform leftBounds;
    public Transform rightBounds;

    public Transform introWalkStart;
    public Transform introWalkEnd;

    void Start()
    {

        activeCamera = Camera.main;

        cameraHalfWidth = Mathf.Abs(activeCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - activeCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x) * 0.5f;
        minValue = minVisibleX + cameraHalfWidth;
        maxValue = maxVisibleX - cameraHalfWidth;

        Vector3 position;
        position = leftBounds.transform.localPosition;
        position.x = transform.localPosition.x - cameraHalfWidth;
        leftBounds.transform.localPosition = position;

        position = rightBounds.transform.localPosition;
        position.x = transform.localPosition.x + cameraHalfWidth;
        rightBounds.transform.localPosition = position;

        //Walk in Start marker setting
        position = introWalkStart.transform.localPosition;
        position.x = transform.localPosition.x - cameraHalfWidth - 2.0f;
        introWalkStart.transform.localPosition = position;

        //Walk in End marker setting
        position = introWalkEnd.transform.localPosition;
        position.x = transform.localPosition.x - cameraHalfWidth + 5.0f;
        introWalkEnd.transform.localPosition = position;
    }

    //cameraRoot의 x 포지션 변경
    public void SetXPosition(float x)
    {
        Vector3 trans = cameraRoot.position;
        trans.x = Mathf.Clamp(x, minValue, maxValue);
        cameraRoot.position = trans;

    }

    public void EnableBounds(bool isEnabled)
    {
        rightBounds.GetComponent<Collider>().enabled = isEnabled;
        leftBounds.GetComponent<Collider>().enabled = isEnabled;
    }

}