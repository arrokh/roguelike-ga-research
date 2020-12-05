using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class GeneratorMetric {

    //Used in ERAs
    public Color color;

	public MethodInfo method;
    public object target;

    public string name;
    public float currentValue = 0f;
    public bool targeted = true;

    public Type base_type;

    public GeneratorMetric(MethodInfo method, string name){
        this.method = method;
        this.name = name;
        this.target = null;
        base_type = null;
        color = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1f);
    }

    public GeneratorMetric(MethodInfo method, string name, Type type){
        this.method = method;
        this.name = name;
        this.target = null;
        base_type = type;
        color = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1f);
    }

    public GeneratorMetric(MethodInfo method, object target, string name){
        this.name = name;
        this.method = method;
        this.target = target;
        color = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1f);
    }
}
