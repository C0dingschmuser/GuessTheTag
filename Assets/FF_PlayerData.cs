using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FF_PlayerData : MonoBehaviour
{
    public GameObject ffHandler, msgBox;
    public bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void PlayerGo(bool full = false)
    {
        dead = false;
        msgBox.SetActive(false);

        if(full)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
            GetComponent<Rigidbody2D>().gravityScale = 150;
        } else
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            GetComponent<Animator>().SetTrigger("Play");
        }
    }

    public void PlayerFly()
    {
        if(dead)
        {
            return;
        }

        Vector2 velocity = new Vector3(0, 750f);
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if((collision.gameObject.tag.Equals("FF_World") ||
            collision.gameObject.tag.Equals("FF_Pipe") ||
            collision.gameObject.tag.Equals("FF_PipeTop")) && !dead)
        { //Death
            if(!ffHandler.GetComponent<FlatterFogelHandler>().gameActive)
            {
                return;
            }

            dead = true;

            msgBox.SetActive(true);

            GetComponent<Animator>().SetTrigger("Stop");
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            ffHandler.GetComponent<FlatterFogelHandler>().PlayerDeath();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(dead)
        {
            msgBox.transform.position = new Vector3(transform.position.x + 117f,
                transform.position.y + 28.5f);
        }
    }
}
