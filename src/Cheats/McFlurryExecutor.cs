using UnityEngine;
using System;
using System.IO;
using System.Reflection;

namespace MalumMenu;

public class McFlurryExecutor : MonoBehaviour
{
    // Path to the file you will write your "live" code in
    private string execPath = @"C:\McFlurry\exec.txt";

    public void Update()
    {
        if (MalumMenu.isPanicked) return;

        // Press F5 to execute whatever is in the text file
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ExecuteFile();
        }
    }

    private void ExecuteFile()
    {
        if (!File.Exists(execPath))
        {
            Debug.LogWarning($"[Executor] No file found at {execPath}");
            return;
        }

        try
        {
            string code = File.ReadAllText(execPath);
            Debug.Log("[Executor] Running live code...");
            
            // This is a "Poor Man's Executor"
            // It searches for a method in your mod and runs it.
            // For a REAL C# executor, you'd need the Mono.CSharp library to compile strings.
            
            Eval(code);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Executor] Error: {e.Message}");
        }
    }

    private void Eval(string command)
    {
        // Example: If you write "KickAll" in the text file, it runs your KickAll logic
        if (command.Trim() == "KickAll")
        {
            // You can call your existing methods here
            // This acts as a bridge for your custom commands
        }
    }
}
