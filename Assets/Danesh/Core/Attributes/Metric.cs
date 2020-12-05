using UnityEngine;
using System.Collections;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
public class Metric : PropertyAttribute {

	string _name;
    Type _target;

	public Metric(string Name){
		_name = Name;
	}

    public Metric(string Name, Type Target){
        _name = Name;
        _target = Target;
    }

	public string Name{
		get {return this._name;}
	}

    public Type Target{
        get {return this._target;}
    }

}
