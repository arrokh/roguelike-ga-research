using UnityEngine;
using System.Collections;
using System.Reflection;

public class GeneratorParameter {

    public string name;
    public bool activated = true;

    public object minValue;
    public object maxValue;
    public object currentValue;
    public object originalValue;

    public object owner;
    public FieldInfo field;
    public string type;

    public bool locked = false;

	public GeneratorParameter(string name, object currentValue, object minValue, object maxValue, FieldInfo field, object owner){
        this.name = name;

        this.currentValue = currentValue;
        this.originalValue = currentValue;
        this.minValue = minValue;
        this.maxValue = maxValue;

        this.field = field;
        this.owner = owner;
        if(currentValue is bool)
            type = "bool";
        if(currentValue is int)
            type = "int";
        if(currentValue is float)
            type = "float";
    }

    public void ParseAndSetValue(string s){
        if(locked)
            return;

        // if(currentValue is bool)
            //???


        if(currentValue is int){
            int o;
            if(int.TryParse(s, out o))
                SetValue(o);
        }
        if(currentValue is float){
            float o;
            if(float.TryParse(s, out o))
                SetValue(o);
        }
    }

    public void ParseSetMinValue(string s){
        if(currentValue is int){
            int o;
            if(int.TryParse(s, out o)){
                minValue = (object)o;
            }
        }
        if(currentValue is float){
            float o;
            if(float.TryParse(s, out o))
                minValue = (object)o;
        }
    }

    public void ParseSetMaxValue(string s){
        if(currentValue is int){
            int o;
            if(int.TryParse(s, out o)){
                maxValue = (object)o;
            }
        }
        if(currentValue is float){
            float o;
            if(float.TryParse(s, out o))
                maxValue = (object)o;
        }
    }

    public void SetValueFractional(float f){
        if(locked)
            return;

        if(currentValue is float){
            float delta = ((float)maxValue - (float)minValue) * f;
            field.SetValue(owner, (float)minValue + delta);
            currentValue = (float)minValue + delta;
        }
        if(currentValue is int){
            int delta = (int) Mathf.Round(((int)maxValue - (int)minValue) * f);
            field.SetValue(owner, (int)minValue + delta);
            currentValue = (int)minValue + delta;
        }
    }

    public void SetValue(object o){
        if(locked)
            return;

        field.SetValue(owner, o);
        currentValue = o;
    }

    public object GetValue(){
        return field.GetValue(owner);
    }

    public float GetSparklineValue(){
        object temp = field.GetValue(owner);
        if(temp is int){
            return ((float)(int)field.GetValue(owner)-(float)(int)minValue)/((float)(int)maxValue-(float)(int)minValue);
        }
        else if(temp is float){
            return ((float)field.GetValue(owner)-(float)minValue)/((float)maxValue-(float)minValue);
        }
        else if(temp is bool){
            if((bool)temp){
                return 1f;
            }
            return 0f;
        }
        return 0f;
    }

    public object GetRandomValue(){
        object temp = field.GetValue(owner);
        if(temp is int){
            return Random.Range((int)minValue, ((int)maxValue)+1);
        }
        else if(temp is float){
            return Random.Range((float)minValue, (float)maxValue);
        }
        else if(temp is bool){
            return Random.Range(0, 2) == 0;
        }

        return null;
    }

    public void RandomiseValue(){
        if(locked)
            return;

        object temp = field.GetValue(owner);
        if(temp is int){
            field.SetValue(owner, Random.Range((int)minValue, ((int)maxValue)+1));
        }
        else if(temp is float){
            field.SetValue(owner, Random.Range((float)minValue, (float)maxValue));
        }
        else if(temp is bool){
            field.SetValue(owner, Random.Range(0, 2) == 0);
        }
    }

}
