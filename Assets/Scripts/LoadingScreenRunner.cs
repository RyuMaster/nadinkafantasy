using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenRunner : MonoBehaviour {

    public GameObject img1;
    public GameObject img2;
    public GameObject img3;

    float tick = 0;
    int stage = 0;

     void Start()
    {
        img1.SetActive(false);
        img2.SetActive(false);
        img3.SetActive(false);

    }

    // Update is called once per frame
    void Update ()
    {


        tick += Time.deltaTime;

        if (tick >= 0.15f)
        {
            stage++;
            tick = 0;
            if (stage == 4)
            {
                stage = 0;
            }

            if (stage == 1)
            {
                img1.SetActive(true);
            }
            if (stage == 2)
            {
                img2.SetActive(true);
            }
            if (stage == 3)
            {
                img3.SetActive(true);
            }
            if (stage == 0)
            {
                img1.SetActive(false);
                img2.SetActive(false);
                img3.SetActive(false);
            }
        }

	}
}
