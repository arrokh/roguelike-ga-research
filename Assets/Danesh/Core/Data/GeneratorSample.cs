using UnityEngine;
using System.Collections;

public class GeneratorSample {

    public object content;
    public object[] parameterValues;

    public GeneratorSample(object content, object[] values){
        this.content = content;
        this.parameterValues = values;
    }

}
