using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenRunner : MonoBehaviour {

    public GameObject img1;
    public GameObject img2;
    public GameObject img3;

//    float tick = 0;
//    int stage = 0;

     void Start()
    {
        Debug.Log("\t\tLSR Start");
        img1.SetActive(false);
        img2.SetActive(false);
        img3.SetActive(false);

    }

    // Update is called once per frame
    void Update ()
    {


        //tick += Time.deltaTime;

        //if (tick >= 0.55f)
        //{
        //    stage++;
        //    tick = 0;
        //    switch (stage) {
        //        case 4: stage = 0;
        //            img1.SetActive(false);
        //            img2.SetActive(false);
        //            img3.SetActive(false);
        //            break;
        //        case 1: img1.SetActive(true);
        //            break;
        //        case 2: img2.SetActive(true);
        //            break;
        //        case 3: img3.SetActive(true);
        //            break;
        //    }
        //}

	}
}
