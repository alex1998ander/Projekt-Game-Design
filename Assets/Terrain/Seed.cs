using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed {

    private string seed;
    private int seedHashcode;

    public int Fst { get; }
    public int Snd { get; }
    public int Trd { get; }

    public Seed() : this("Default") { }

    public Seed(string seed) {
        this.seed = seed;
        seedHashcode = seed.GetHashCode();

        Random.InitState(seedHashcode);

        Fst = Random.Range(0, 10000);
        Snd = Random.Range(0, 10000);
        Trd = Random.Range(0, 10000);
    }
}
