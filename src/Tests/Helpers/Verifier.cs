﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Utilities;
using NUnit.Framework;

public static class Verifier
{
    public static void Verify(string beforeAssemblyPath, string afterAssemblyPath)
    {
        var before = Validate(beforeAssemblyPath);
        var after = Validate(afterAssemblyPath);
        var message = $"Failed processing {Path.GetFileName(afterAssemblyPath)}\r\n{after}";
        Assert.AreEqual(TrimLineNumbers(before), TrimLineNumbers(after), message);
    }

    public static string Validate(string assemblyPath2)
    {
        var exePath = GetPathToPEVerify();
        using (var process = Process.Start(new ProcessStartInfo(exePath, $"\"{assemblyPath2}\"")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }))
        {
            process.WaitForExit(10000);
            return process.StandardOutput.ReadToEnd().Trim().Replace(assemblyPath2, "");
        }
    }

    static string GetPathToPEVerify()
    {
        var sdkPath = Path.GetFullPath(Path.Combine(ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.VersionLatest), "..\\.."));
        var path = Directory.GetFiles(sdkPath, "peverify.exe", SearchOption.AllDirectories).LastOrDefault();

        if (!File.Exists(path))
            Assert.Fail("PEVerify could not be found");
        return path;
    }

    static string TrimLineNumbers(string foo)
    {
        return Regex.Replace(foo, @"0x.*]", "");
    }
}