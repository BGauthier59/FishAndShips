using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireObject : MonoBehaviour
{
    public bool burning;
    public List<ParticleSystem> firePart;

    public void Start()
    {
        burning = true;
        //ParticulesSystem
        firePart[0].gameObject.SetActive(true);
        firePart[0].Play();
    }

    public void ResetFire()
    {
        burning = true;
        firePart[0].gameObject.SetActive(true);
    }

    public void ExtinguishFlame()
    {
        burning = false;
        firePart[0].Stop();
        firePart[0].gameObject.SetActive(false);
    }
}