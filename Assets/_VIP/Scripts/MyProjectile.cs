using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyProjectile : MonoBehaviour
{
    public MyAIBaes caster;//投放者
    public MyAIBaes target;//目标

    public  float progress=0;

    public float Speed=1;

    public bool isUse = false;//这投掷物是否使用，使用才能执行它

}
