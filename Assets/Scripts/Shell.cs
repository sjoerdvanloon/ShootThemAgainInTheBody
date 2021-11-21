using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{

    public Rigidbody _rigidbody;
    public float forceMin;
    public float forceMax;


    float _lifeTime = 4;
    float _fadeTime = 2;
    // Start is called before the first frame update
    void Start()
    {
        float force = Random.Range(forceMin, forceMax);
        _rigidbody.AddForce(transform.right * force);
        _rigidbody.AddTorque(Random.insideUnitSphere * force);

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(_lifeTime);

        float percent = 0;
        float fadeSpeed = 1 / _fadeTime;

        var material = GetComponent<Renderer>().material;
        var initialColor = material.color;

        while(percent <1)
        {
            percent += Time.deltaTime * fadeSpeed;
            material.color = Color.Lerp(initialColor, Color.clear, percent);

            yield return null; // wait a frame per loop iteration
        }


        Destroy(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
