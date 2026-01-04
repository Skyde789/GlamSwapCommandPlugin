using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
namespace GlamSwapCommand;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; set; } = null!;

    private const string CommandName = "/cgs";

    public Plugin()
    {
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Change the glamour for your currently selected gear set! usage: /cgs <glamNumber>"
        });
        Log.Information($"===Loaded{PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        // Unregister all actions to not leak anything during disposal of plugin
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        int glamour = 1;
        int gearset = 1;

        if (args != null && args?.Length > 0)
        {
            Log.Information($"==={PluginInterface.Manifest.Name}=== Trying to parse the glamour number...");

            if (int.TryParse(args, out var value))
            {
                Log.Information($"==={PluginInterface.Manifest.Name}=== Glamour number found: " + value);
                glamour = value;

                if(value <= 0 || value > 20)
                {
                    ChatGui.Print("Please input only numbers between 1-20: '/cgs 4'");
                    return;
                }
            }
            else
            {
                ChatGui.Print("Glamour number parsing failed");
                ChatGui.Print("Please input only numbers: '/cgs 4'");
                return;
            }
        }

        unsafe
        {
            Log.Information($"==={PluginInterface.Manifest.Name}=== Trying to parse the gearset number...");

            var test = RaptureGearsetModule.Instance();

            if (test != null)
            {
                gearset = test->CurrentGearsetIndex;
                Log.Information($"==={PluginInterface.Manifest.Name}=== Currently selected gearset found: {gearset}");
            }
            else
            {
                ChatGui.Print("Gear set could not be found");
                return;
            }
            Log.Information($"==={PluginInterface.Manifest.Name}=== Trying to input command: '/gs change {gearset} {glamour}'");
            try
            {
                test->EquipGearset(gearset, (byte)glamour);
            }
            catch
            {
                ChatGui.Print($"Command failed");
            }

        }
    }
}
