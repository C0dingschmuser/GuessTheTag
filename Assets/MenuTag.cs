using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class MenuTag : MonoBehaviour
{
    public int internalPos = 0; //0-2, 0 is top

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void ResetLock()
    {
        transform.parent.GetComponent<MenuData>().locked = false;
    }

    private void Lock()
    {
        transform.parent.GetComponent<MenuData>().locked = false;

        Invoke("ResetLock", 0.51f);
    }

    private bool IsLocked()
    {
        return transform.parent.GetComponent<MenuData>().locked;
    }

    public void VoteUp(bool force = false)
    {
        if(!IsLocked())
        {
            Lock();
        } else
        {
            return;
        }

        if (internalPos > 0)
        {
            int old = internalPos;
            transform.DOMoveY(transform.position.y + 150, 0.5f);
            internalPos--;

            if(force)
            {
                transform.SetSiblingIndex(internalPos);
                return;
            }

            switch(old)
            {
                case 1: //mitte
                    Transform obj = transform.parent.GetChild(0);
                    obj.GetComponent<MenuTag>().VoteDown(true);
                    break;
                case 2: //unten
                    obj = transform.parent.GetChild(1);
                    obj.GetComponent<MenuTag>().VoteDown(true);
                    break;
            }

            transform.SetSiblingIndex(internalPos);
        }
    }

    public void VoteDown(bool force = false)
    {
        if (!IsLocked())
        {
            Lock();
        }
        else
        {
            return;
        }

        if (internalPos < 2)
        {
            int old = internalPos;
            transform.DOMoveY(transform.position.y - 150, 0.5f);
            internalPos++;

            if(force)
            {
                return;
            }

            switch(old)
            {
                case 0: //oben
                    Transform obj = transform.parent.GetChild(1);
                    obj.GetComponent<MenuTag>().VoteUp(true);
                    break;
                case 1: //mitte
                    obj = transform.parent.GetChild(2);
                    obj.GetComponent<MenuTag>().VoteUp(true);
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
