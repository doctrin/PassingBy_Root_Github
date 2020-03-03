using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SD_PositionControl : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Vector3 sdPosition     = this.transform.position;
        Vector3 parentPosition = this.transform.parent.transform.position;
        Hero parentHeroScript = this.transform.parent.gameObject.GetComponent<Hero>();
        int numberFacingLeft = parentHeroScript.isFacingLeft == true ? -1 : 1;

        //카메라 Bounds를 못 벗어나도록 제어 한다.  //카메라 Bounds 하고 너무 일치 되어도 곤란 하니까, 약간의 간격을 주도록 하자.
        if (sdPosition.x <= GameObject.Find("LeftCamBounds").transform.position.x)
        {
            this.transform.position = new Vector3(GameObject.Find("LeftCamBounds").transform.position.x + 0.3f, sdPosition.y, sdPosition.z);
        }
        else if (sdPosition.x >= GameObject.Find("RightCamBounds").transform.position.x)
        {
            this.transform.position = new Vector3(GameObject.Find("RightCamBounds").transform.position.x - 0.3f, sdPosition.y, sdPosition.z);
        }
        else
        {
            this.transform.position = new Vector3( (parentPosition.x + (5 * numberFacingLeft)), parentPosition.y, parentPosition.z);
        }
    }
}
