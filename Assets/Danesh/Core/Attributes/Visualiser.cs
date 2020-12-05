using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
public class Visualiser : PropertyAttribute {

    string _type;

    public Visualiser(string type="texture"){
        _type = type;
    }

    public string Type{
        get {return this._type;}
    }

}
