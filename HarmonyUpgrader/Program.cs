using Mono.Cecil;
using System;
using System.IO;
namespace HarmonyUpgrader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Concat(new string[]
      {
                      "                                                   .-'''-.                                                             ",
                      System.Environment.NewLine,
                      "                                                  '   _    \\                               .-''-.         ..-'''-.     ",
                      System.Environment.NewLine,
                      "   .                            __  __   ___    /   /` '.   \\    _..._                   .' .-.  )        \\.-'''\\ \\    ",
                      System.Environment.NewLine,
                      " .'|                           |  |/  `.'   `. .   |     \\  '  .'     '..-.          .- / .'  / /                | |   ",
                      System.Environment.NewLine,
                      "<  |                   .-,.--. |   .-.  .-.   '|   '      |  '.   .-.   .\\ \\        / /(_/   / /              __/ /    ",
                      System.Environment.NewLine,
                      " | |             __    |  .-. ||  |  |  |  |  |\\    \\     / / |  '   '  | \\ \\      / /      / /              |_  '.    ",
                      System.Environment.NewLine,
                      " | | .'''-.   .:--.'.  | |  | ||  |  |  |  |  | `.   ` ..' /  |  |   |  |  \\ \\    / /      / /                  `.  \\  ",
                      System.Environment.NewLine,
                      " | |/.'''. \\ / |   \\ | | |  | ||  |  |  |  |  |    '-...-'`   |  |   |  |   \\ \\  / /      . '                     \\ '. ",
                      System.Environment.NewLine,
                      " |  /    | | `\" __ | | | |  '- |  |  |  |  |  |               |  |   |  |    \\ `  /      / /    _.-'),.--.         , | ",
                      System.Environment.NewLine,
                      " | |     | |  .'.''| | | |     |__|  |__|  |__|               |  |   |  |     \\  /     .' '  _.'.-''//    \\        | | ",
                      System.Environment.NewLine,
                      " | |     | | / /   | |_| |                                    |  |   |  |     / /     /  /.-'_.'    \\    /       / ,' ",
                      System.Environment.NewLine,
                      " | '.    | '.\\ \\._,\\ '/|_|                                    |  |   |  | |`-' /     /    _.'        `'--'-....--'  /  ",
                      System.Environment.NewLine,
                      " '---'   '---'`--'  `\"                                        '--'   '--'  '..'     ( _.-'                `.. __..-'   ",
                      System.Environment.NewLine,
                      System.Environment.NewLine,
                      "Upgrader by bmgjet",
      }));
            if (args.Length <= 0)
            {
                Console.WriteLine("File not found, Drop .cs or .dll on this exe");
                Console.ReadKey();
                return;
            }
            string InputFile = args[0];
            Console.WriteLine("Opening " + InputFile);
            if (File.Exists(InputFile))
            {
                if (InputFile.EndsWith(".dll"))
                {
                    CheckHarmonyDlls(new FileInfo(InputFile));
                    Console.WriteLine("Dll Upgraded");
                    Console.ReadKey();
                    return;
                }
                else if (InputFile.EndsWith(".cs"))
                {
                    CheckOxidePlugins(new FileInfo(InputFile));
                    Console.WriteLine(".cs Upgraded");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    Console.WriteLine("Must Be .cs or .dll file");
                    Console.ReadKey();
                    return;
                }
            }
            Console.WriteLine("File not found, Drop .cs or .dll on this exe");
            Console.ReadKey();
        }

        #region Functions
        private static void CheckHarmonyDlls(FileInfo file)
        {
            if (file == null) { return; }
            ModuleDefinition asm = ModuleDefinition.ReadModule(file.FullName); //Read dll assembly
            try
            {
                //Check version
                AssemblyDefinition a = asm.Assembly;
                for (int i = a.MainModule.AssemblyReferences.Count - 1; i >= 0; i--)
                {
                    if (a.MainModule.AssemblyReferences[i].Name == "0Harmony" && a.MainModule.AssemblyReferences[i].Version.ToString() == "1.2.0.1")
                    {
                        //Change version number
                        a.MainModule.AssemblyReferences[i].Version = new Version(2, 3, 0, 0);
                        Console.WriteLine("Found Harmony V1.2 dll " + file.Name + " Creating Back Up As " + file.Name.Replace(".dll", ".V1_2") + " And Upgrading.");
                        //Update Referece
                        foreach (var MetaData in asm.GetMemberReferences())
                        {
                            if (MetaData.FullName.Contains("HarmonyPatch") || MetaData.FullName.Contains("HarmonyPrefix") || MetaData.FullName.Contains("HarmonyPostfix") || MetaData.FullName.Contains("HarmonyTranspiler"))
                            {
                                MetaData.FullName.Replace("Harmony.", "HarmonyLib.");
                            }
                        }

                        //Update Name Spaces
                        foreach (var moduleDefinition in a.Modules)
                        {
                            foreach (var typeDefinition in moduleDefinition.Types)
                            {
                                if (typeDefinition.Namespace == "Harmony") { typeDefinition.Namespace = "HarmonyLib"; }
                            }
                            foreach (var TypeReferences in moduleDefinition.GetTypeReferences())
                            {
                                if (TypeReferences.Namespace == "Harmony") { TypeReferences.Namespace = "HarmonyLib"; }
                            }
                        }
                        asm.Write(file.FullName.Replace(".dll", "modded.dll")); //Save as modded version
                        asm.Dispose(); //Close opened dll
                        File.Move(file.FullName, file.FullName.Replace(".dll", ".V1_2")); //Create back up of orignal dll
                        File.Move(file.FullName.Replace(".dll", "modded.dll"), file.FullName); //Rename modded dll to orignals name
                        break;
                    }
                }
            }
            catch
            {
                //Something went wrong
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed To Process: " + file.FullName);
                Console.ForegroundColor = ConsoleColor.Magenta;
            }
            if (asm != null) { asm.Dispose(); } //Close dll
        }

        private static void CheckOxidePlugins(FileInfo file)
        {
            if (file == null) { return; }
            bool Upgraded = false;
            string[] PluginSRC = File.ReadAllLines(file.FullName); //Read all plugin lines
            for (int i = PluginSRC.Length - 1; i >= 0; i--) //Check each line and upgrade any harmony init code
            {
                if (PluginSRC[i] == "using Harmony;")
                {
                    Upgraded = true;
                    PluginSRC[i] = "using HarmonyLib;";
                }
                if (PluginSRC[i].Contains("HarmonyInstance"))
                {
                    Upgraded = true;
                    PluginSRC[i] = PluginSRC[i].Replace("HarmonyInstance", "Harmony");
                }
                if (PluginSRC[i].Contains("HarmonyInstance.Create"))
                {
                    Upgraded = true;
                    PluginSRC[i] = PluginSRC[i].Replace("HarmonyInstance.Create", "new HarmonyLib.Harmony");
                    string[] getname = PluginSRC[i].Split('=');
                    if (getname.Length == 2)
                    {
                        PluginSRC[i] += System.Environment.NewLine + getname[0].TrimEnd() + ".PatchAll();";
                    }
                }
                if (PluginSRC[i].Contains("Harmony.Create"))
                {
                    Upgraded = true;
                    PluginSRC[i] = PluginSRC[i].Replace("Harmony.Create", "new HarmonyLib.Harmony");
                    string[] getname = PluginSRC[i].Split('=');
                    if (getname.Length == 2)
                    {
                        PluginSRC[i] += System.Environment.NewLine + getname[0].TrimEnd() + ".PatchAll();";
                    }
                }
                if (PluginSRC[i].Contains("PatchProcessor"))
                {
                    Upgraded = true;
                    PluginSRC[i] = "";
                }
            }
            if (Upgraded)
            {
                Console.WriteLine("Found Oxide Harmony V1.2 plugin " + file.Name + " Saving Backup And Creating Patched Version.");
                File.Copy(file.FullName, file.FullName.Replace(".cs", ".old"));
                File.WriteAllLines(file.FullName, PluginSRC);
            }
        }
        #endregion
    }
}