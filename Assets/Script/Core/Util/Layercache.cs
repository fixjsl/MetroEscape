using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class Layercache
    {
    public static readonly int Player = LayerMask.NameToLayer("Player");
    public static readonly int PlayerHitbox = LayerMask.NameToLayer("PlayerHitbox");
    public static readonly int Monster = LayerMask.NameToLayer("Monster");
    public static readonly int MonsterHitbox = LayerMask.NameToLayer("MonsterHitbox");
    public static readonly int Dodge = LayerMask.NameToLayer("Dodge");
    public static readonly int Die = LayerMask.NameToLayer("Die");
    public static readonly int Stun = LayerMask.NameToLayer("Stun");
    }

