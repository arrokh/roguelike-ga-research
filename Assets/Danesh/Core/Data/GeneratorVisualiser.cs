using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class GeneratorVisualiser {

    //Just a wrapper class to carry things around in

    public string name;
    public object targetObject;
    public MethodInfo method;

    public GeneratorVisualiser(string name, object targetObject, MethodInfo method){
        this.name = name;
        this.targetObject = targetObject;
        this.method = method;
    }

}
