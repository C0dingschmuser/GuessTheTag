using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BenisImage : MonoBehaviour
{
    private void Update()
    {
        Color c = GetComponent<Image>().color;

        if (transform.position.y < 1915)
        {
            c.a -= 1.5f * Time.deltaTime;

            if (c.a < 0)
            {
                c.a = 0;
            }
            GetComponent<Image>().color = c;
        }

        if (transform.position.y < 1249f)
        {
            Destroy(this.gameObject);
            return;
        }
    }
}
