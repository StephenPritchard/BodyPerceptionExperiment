using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public static class ExtensionMethods
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void SwapPositions<T>(this IList<T> list, int indexA, int indexB)
    {
        var temp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = temp;
    }

    public static StreamReader ReadFileToStream(this FileInfo file)
    {
        StreamReader streamR = null;

        try
        {
            streamR = file.OpenText();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("There was a problem opening {0}.", file.Name);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            Application.Quit();
        }

        return streamR;
    }

    public static StreamWriter StreamToWriteFile(this FileSystemInfo file)
    {
        StreamWriter streamW = null;

        try
        {
            streamW = new StreamWriter(file.FullName, false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("There was a problem creating {0}.", file.Name);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            Application.Quit();
        }

        return streamW;
    }

    public static void WriteTextToFile(this FileInfo file, string text)
    {
        using (var streamW = file.StreamToWriteFile())
        {
            streamW.Write(text);
        }
    }
}