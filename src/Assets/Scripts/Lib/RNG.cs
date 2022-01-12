using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RNG {
    private int[] state;

    public RNG(int seed) {
        state = new int[] {0, seed};
    }

    private int xorshift(int num) {
        num ^= num << 13;
        num ^= num >> 17;
        num ^= num << 5;

        return num;
    }

    private int next() {
        int nextNum = 0;
        for (int i = 0; i < state.Length; i++) {
            nextNum += xorshift(state[i]);
                
            if (i < state.Length - 1) {
                state[i] = state[i + 1];
            }
        }

        return state[state.Length - 1] = (int) (nextNum ^ 0x96696996);
    }

    public int nextInt() {
        return next();
    }

    public long nextLong() {
        return (long) next() + ((long) next() << 32);
    }

    public float nextFloat() {
        return (next() / (float) (Mathf.Pow(2, 32) - 1)) + 0.5f;
    }

    public double nextDouble() {
        return (nextLong() / (Mathf.Pow(2, 64) - 1)) + 0.5d;
    }

    public double nextGaussian() {
        return (double) Mathf.Sqrt(-2f * Mathf.Log(nextFloat())) * Mathf.Cos(2f * Mathf.PI * nextFloat());
    }
}