using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBS : MonoBehaviour {

    public enum AmmoType { M4A1, AK47, Handgun, FragGrenade, None };
    public AmmoType ammoType = AmmoType.None;
	public enum Type { SingleRound, Clip, Bandolier };
    public Type type = Type.Clip;
    public int rounds = 0;
    public int maxRounds = 30;
    public ItemBS item;

    // Use this for initialization
	void Start () {
        item = GetComponent<ItemBS>();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool isCompatible(WeaponBS weapon)
    {
        if(ammoType == weapon.ammoType)
        {
            return true;
        }
        return false;
    }
}
