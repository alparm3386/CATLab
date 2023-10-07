package com.tm;

import java.util.Collections;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.Map;

import org.junit.Test;

import gnu.trove.map.hash.THashMap;

public class PerfTests {

    @Test
    public void testMaps() {
        testMap("HashMap", new HashMap<>());
        testMap("Hashtable", new Hashtable<>());
        testMap("THashMap", new THashMap<>());
        //testMap("Synced-HashMap", Collections.synchronizedMap(new HashMap<>()));
    }

    void testMap(String name, Map<String, String> h) {
        for(int i=1; i<=tries; ++i) {
            long t1 = timeit(() -> testMapPut(h));
            long t2 = timeit(() -> testMapGet(h));
            System.out.println(String.format("%d %s put/get -->  %7.2f  /  %7.2f  ms",
                    i, name, t1/1000/1000.0, t2/1000/1000.0));
        }
    }

    long timeit(Runnable r) {
        System.gc();
        long t = System.nanoTime();
        r.run();
        return System.nanoTime() - t;
    }

    static final int tries = 5;
    static final int count = 100000000;

    static final String VALUE = "-";

    static final int putSpace = 100;
    static final int getSpace = putSpace*2;
    static final Integer[] numbers = new Integer[getSpace+1];

    static {
        for(int i=getSpace; i>=0; --i)
            numbers[i] = i;
    }

    void testMapPut(Map<String, String> m) {
        for(int i=count; i>0; --i)
            m.put(numbers[i%putSpace].toString(), VALUE);
    }

    void testMapGet(Map<String, String> m) {
        for(int i=count; i>0; --i)
            m.get(numbers[i%getSpace]);
    }
}