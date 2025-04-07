using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD.API;
using FivePD.API.Utils;
using MenuAPI;
using Newtonsoft.Json.Linq;

namespace FivePD_HostageScenarioCallout;

[Guid("BE6ED2E8-9805-4523-A5D3-2BDE0F941F79")]
[CalloutProperties("Hostage Situation","DevKilo","1.0")]
public class HostageCallout : Callout
{
    public static EventHandlerDictionary eventHandlers;
    public static JObject config;
    private List<Vector4> locations;
    private Vector4 selectedLocation;
    private Ped suspect, victim;
    private bool loading = true;
    public static int playerPoints = 0;
    private List<string> messageQueue = new List<string>();
    private bool willingToKill = true;
    public static bool EnableAI = false;
    public static bool calloutActive = false;
    
    private async Task WaitForCalloutLoad()
    {
        while (loading)
            await BaseScript.Delay(100);
    }
    private void SetLocation()
    {
        try
        {
            EnableAI = (bool)EnableAI;
            locations = Utils.GetLocationArrayFromJArray((JArray)config["Locations"], "coords");
            selectedLocation = Utils.GetRandomLocationFromArray(locations.ToArray());
            InitInfo((Vector3)selectedLocation);
        }
        catch (Exception err)
        {
            Utils.CalloutError(err, this);
        }
    }

    private void Startup()
    {
        playerPoints = 0;
        loading = true;
        SetLocation();
    }
    public HostageCallout()
    {
        EnableAI = (bool)config["EnableAI"];
        Startup();
        ShortName = "~r~Active Hostage Situation";
        CalloutDescription = "You are being called out as the situation defuser. Prove your worth and good luck!";
        ResponseCode = 3;
        StartDistance = 50f;
        eventHandlers["KiloHostageCallout:ShowDialogue"] += showDialog;
        eventHandlers["KiloHostageCallout:KillHostage"] += SuspectKillHostage;
        eventHandlers["KiloHostageCallout:ReleaseHostage"] += SuspectReleaseHostage;
        eventHandlers["KiloHostage::AIMessageToPlayer"] += ReceiveAIMessage;
        eventHandlers["KiloHostage::TaskFollowPlayer"] += TaskFollowPlayer;
        eventHandlers["KiloHostage::TaskEnterHelicopter"] += TaskEnterHelicopter;
        eventHandlers["KiloHostage::TaskEnterCar"] += TaskEnterCar;
        eventHandlers["FIVEPD::KiloAIHostageCallout:ForceStop"] += ForceStop;
    }

    private void ForceStop(bool force)
    {
        if (force)
        {
            Debug.WriteLine("^6Callout forced to end because location is in use.");
            EndCallout();
        }
        else
        {
            Debug.WriteLine("^6Callout AI forced to end by script. This may be due to another instance of this callout being active.");
            if ((bool)config["EnableComeback"])
            {
                EnableAI = false;
                Location = Game.PlayerPed.Position;
                ShowDialog("~y~This callout will continue without AI", 5000, 30f);
                script.StartNonAI();
            }
            else
            {
                EndCallout();
            }
        }
    }

    private async Task TaskFollowPlayer()
    {
        await SuspectReleaseHostage();
        willingToKill = false;
        suspect.Task.ClearAllImmediately();
        suspect.Task.FollowToOffsetFromEntity(Game.PlayerPed, new(2f, 0f, 0f), 3f, -1, 1f, true);
    }

    private async Task TaskEnterHelicopter()
    {
        // Attempt to enter nearest helicopter
        Vehicle heli = GetNearestHelicopterToPed(suspect);
        if (heli == null) return;
        suspect.Task.ClearAllImmediately();
        suspect.Task.GoTo(heli.Position);
        await Utils.WaitUntilEntityIsAtPos(suspect, heli.Position, 10f);
        BaseScript.TriggerServerEvent("KiloHostage::AcceptHelicopter");
        suspect.Task.ClearAll();
        await SuspectReleaseHostage();
        suspect.Task.EnterVehicle(heli, VehicleSeat.Driver);
        await Utils.WaitUntilPedIsInVehicle(suspect, heli, VehicleSeat.Driver);
        var pursuit = Pursuit.RegisterPursuit(suspect);
        pursuit.Init(true, 500f, 500f);
        pursuit.ActivatePursuit();
    }

    private async Task TaskEnterCar()
    {
        // Attempt to enter nearest car
        Vehicle car = GetNearestVehicleToPed(suspect);
        if (car == null)
        {
            Debug.WriteLine("Car is null");
            return;
        }

        await SuspectReleaseHostage();
        suspect.Task.ClearAllImmediately();
        suspect.Task.EnterVehicle(car, VehicleSeat.Driver);
        await Utils.WaitUntilPedIsInVehicle(suspect, car, VehicleSeat.Driver);
        suspect.Task.CruiseWithVehicle(car, 20f, 0);
        var pursuit = Pursuit.RegisterPursuit(suspect);
        pursuit.Init(true, 200f, 100f, true);
        pursuit.ActivatePursuit();
    }
    private Vehicle GetNearestHelicopterToPed(Ped ped)
    {
        Vehicle[] allVehicles = World.GetAllVehicles();
        Vehicle closestVeh = null;
        foreach (var allVehicle in allVehicles)
        {
            if (allVehicle.Position.DistanceTo(ped.Position) < 30f && allVehicle.Model.IsHelicopter)
            {
                if (closestVeh == null)
                    closestVeh = allVehicle;
                if (allVehicle.Position.DistanceTo(ped.Position) < closestVeh.Position.DistanceTo(ped.Position))
                    closestVeh = allVehicle;
            } 
        }

        return closestVeh;
    }

    private Vehicle GetNearestVehicleToPed(Ped ped)
    {
        Vehicle[] allVehicles = World.GetAllVehicles();
        Vehicle closestVeh = null;
        foreach (var allVehicle in allVehicles)
        {
            if (allVehicle.Position.DistanceTo(ped.Position) < 20f && allVehicle.Model.IsCar)
            {
                if (closestVeh == null)
                    closestVeh = allVehicle;
                if (allVehicle.Position.DistanceTo(ped.Position) < closestVeh.Position.DistanceTo(ped.Position))
                    closestVeh = allVehicle;
            }
        }

        return closestVeh;
    }
    
    private void ReceiveAIMessage(string message)
    {
        if (message.Contains("Success Points: "))
        {
            try
            {
                string justThePointsBit = message.Substring(message.IndexOf("Success Points:"));
                var pointsAdded = Int32.Parse(justThePointsBit.Replace("Success Points: ", "").Trim());
                playerPoints += pointsAdded;
                string restMessage = message.Replace(justThePointsBit, "");
                messageQueue.Add(restMessage);
            }
            catch (Exception err)
            {
                Debug.WriteLine("Failed to parse integer from success points.");
                Utils.CalloutError(err,null);
            }
        }
       else if (message.Contains("*enter_car*"))
        {
            TaskEnterCar();
            string justTheBit = message.Substring(message.IndexOf("*enter_car*"));
            string restMessage = message.Replace(justTheBit, "").Trim();
            messageQueue.Add(restMessage);
        } else if (message.Contains("*enter_helicopter*"))
        {
            TaskEnterHelicopter();
            string justTheBit = message.Substring(message.IndexOf("*enter_helicopter*"));
            string restMessage = message.Replace(justTheBit, "").Trim();
            messageQueue.Add(restMessage);
        } else if (message.Contains("*follow_player*"))
        {
            TaskFollowPlayer();
            string justTheBit = message.Substring(message.IndexOf("*follow_player*"));
            string restMessage = message.Replace(justTheBit, "").Trim();
            messageQueue.Add(restMessage);
        } else if (message.Contains("*release_hostage*"))
        {
            SuspectReleaseHostage();
            string justTheBit = message.Substring(message.IndexOf("*release_hostage*"));
            string restMessage = message.Replace(justTheBit, "").Trim();
            messageQueue.Add(restMessage);
        } else if (message.Contains("*kill_hostage*"))
        {
            SuspectKillHostage();
            string justTheBit = message.Substring(message.IndexOf("*kill_hostage*"));
            string restMessage = message.Replace(justTheBit, "").Trim();
            messageQueue.Add(restMessage);
        }
        else
        {
            messageQueue.Add("~r~Suspect~s~: "+message);    
        }
    }

    private async Task SuspectKillHostage()
    {
        await SuspectReleaseHostage();
        suspect.Task.ShootAt(victim, -1);
        while (victim.IsAlive)
        {
            if (suspect.IsDead || suspect.IsCuffed || victim.IsInjured)
                break;
            await BaseScript.Delay(100);
        }
        if (suspect.IsAlive && !suspect.IsCuffed)
            suspect.Task.FightAgainst(Game.PlayerPed);
    }

    private async Task SuspectReleaseHostage()
    {
        victim.Detach();
        await Utils.RequestAnimDict("reaction@shove");
        suspect.Task.PlayAnimation("reaction@shove", "shoved_back", 8f, -8f, -1, AnimationFlags.None, 1f);
        await BaseScript.Delay(250);
        suspect.Task.ClearSecondary();
        suspect.Task.HandsUp(-1);
        victim.Task.ClearSecondary();
        victim.Task.ReactAndFlee(suspect);
    }

    public void showDialog(string text, int duration, float showRadius)
    {
        ShowDialog(text, duration, showRadius);
    }

    public override Task OnAccept()
    {
        ShortName = "Active Hostage Situation";
        AcceptHandler();
        return base.OnAccept();
    }
    bool playerstepdb = false;
    int playerstepped = 0;
    public static bool aitalking = false;
    private async void LoopCheck()
    {
        bool playershooting = false;
        bool handlingAISpeech = false;
        Tick += async () =>
        {
            if (!playershooting && Game.PlayerPed.IsShooting)
            {
                playershooting = true;
                if (suspect.IsAlive && !suspect.IsCuffed)
                {
                    await SuspectKillHostage();
                    suspect.Task.FightAgainst(Game.PlayerPed);
                }
            }

            PlayerStepHandle();

            if (!handlingAISpeech && messageQueue.Count > 0)
            {
                handlingAISpeech = true;
                Array messages = messageQueue.ToArray();
                foreach (string message in messages)
                {
                    aitalking = true;
                    int time = message.Length * 100;
                    showDialog(message, time, 50f);
                    messageQueue.Remove(message);
                    await BaseScript.Delay(time);
                }
                aitalking = false;
                handlingAISpeech = false;
            }
        };
    }

    private async void PlayerStepHandle()
    {
        if (!willingToKill) return;
        if (suspect.IsDead || suspect.IsCuffed) return;
        float ogDistance = 10f;
            if (!playerstepdb)
            {
                playerstepdb = true;
                ogDistance = Game.PlayerPed.Position.DistanceTo(suspect.Position);
                await BaseScript.Delay(3000);
                if (Game.PlayerPed.Position.DistanceTo(suspect.Position) < ogDistance && Game.PlayerPed.Position.DistanceTo(suspect.Position) < 5f)
                    if (Math.Abs(ogDistance - Game.PlayerPed.Position.DistanceTo(suspect.Position)) >= 0.3f)
                    {
                        playerstepped++;
                        if ((bool)EnableAI)
                        {
                            BaseScript.TriggerServerEvent("KiloHostage::SendAITrigger","police_stepped_closer");
                        }
                    }
                if (playerstepped >= 10)
                {
                    SuspectKillHostage();
                } else
                    playerstepdb = false;
            }    
    }
    
    private async Task AcceptHandler()
    {
        BaseScript.TriggerServerEvent("FIVEPD::KiloAIHostageCallout:CalloutBegin",selectedLocation.X,selectedLocation.Y,selectedLocation.Z);
        InitBlip();
        calloutActive = true;
        try {
            suspect = await Utils.SpawnPedOneSync(Utils.GetRandomPed(), Location, true, selectedLocation.W);
            victim =
                await Utils.SpawnPedOneSync(Utils.GetRandomPed(), Location.Around(2f), true, selectedLocation.W);
            suspect.Weapons.Give(WeaponHash.CombatPistol, 255, true, true);
            loading = false;
            Utils.TaskPedTakeTargetPedHostage(suspect, victim);
        }
        catch (Exception err)
        {
            Utils.CalloutError(err,this);
        }
    }

    public override async void OnStart(Ped closest)
    {
        try
        {
            await WaitForCalloutLoad();
            suspect.AttachBlip();
            if (suspect == null)
                Debug.WriteLine("Suspect is null!");
            if (victim == null)
                Debug.WriteLine("Victim is null!");
            if (!(bool)EnableAI)
                NoAI_WarningDialog();
            else
                AI_WarningDialog();
            
            LoopCheck();
        }
        catch (Exception err)
        {
            Utils.CalloutError(err, this);
        }
        
    }

    private async void AI_WarningDialog()
    {
        // TO-DO
        BaseScript.TriggerServerEvent("KiloHostage::InitiateConversation");
        await BaseScript.Delay(1000);
        BaseScript.TriggerServerEvent("KiloHostage::SendAITrigger", "hostage_situation_start");
    }

    private async void NoAI_WarningDialog()
    {
        // Default pronouns
        string pronouns = "them";
        // Set pronouns
        try
        {
            switch (victim.Gender)
            {
                case Gender.Female:
                {
                    pronouns = "her";
                    break;
                }
                case Gender.Male:
                {
                    pronouns = "him";
                    break;
                }
            }
        }
        catch (Exception err)
        {
            // Do nothing
        }
        //
        ShowDialog("~r~Suspect~s~: Do ~r~NOT~s~ step ~r~ANY~s~ closer. I will ~r~kill " + pronouns+"~s~!", 5000, 50f);
        await BaseScript.Delay(5000);
        // Show notification to primary officer
        Location = Game.PlayerPed.Position;
        ShowNetworkedNotification("Press ~y~Y~s~ to open negotiations menu!", "CHAR_911CALL", "CHAR_911CALL",
            "Callout",
            "Hint", 1f);
        Location = (Vector3)selectedLocation;
        //    
    }

    public override void OnCancelBefore()
    {
        calloutActive = false;
        BaseScript.TriggerServerEvent("FIVEPD::KiloAIHostageCallout:CalloutEnd",selectedLocation.X,selectedLocation.Y,selectedLocation.Z);
        if (suspect != null)
            suspect.Detach();
        if (victim != null)
            victim.Detach();
        if (suspect.AttachedBlip != null)
            suspect.AttachedBlip.Delete();
        if (suspect != null)
        {
            if (!suspect.IsCuffed)
            {
                Utils.ReleaseEntity(suspect);    
            }
            else
            {
                suspect.IsPersistent = false;
            } 
        }
            if (victim != null)
            {
                Utils.ReleaseEntity(victim);    
            }
            playerstepdb = false;
            willingToKill = true;
            eventHandlers = null;
            BaseScript.TriggerEvent("KiloHostage::ResetEventHandlers");
    }
}

public class script : BaseScript
{
    public static Menu _menu = new Menu("Negotiations", "Select an option");
    public static List<MenuItem> speechOptions = new List<MenuItem>();

    public static void StartNonAI()
    {
        MenuController.AddMenu(_menu);
            MenuController.MainMenu = _menu;
            API.RegisterCommand("-openKiloHostageCalloutNegotiationsMenu", new Action<int, List<object>, string>(
                (source, args, rawCommand) =>
                {
                    if (HostageCallout.EnableAI) return;
                    if (!HostageCallout.calloutActive) return;
                    _menu.Visible = !_menu.Visible;
                }), false);
            API.RegisterKeyMapping("-openKiloHostageCalloutNegotiationsMenu", "Opens Kilo Hostage Callout Menu", "keyboard",
                "y");
            var negotiationOptions = (JArray)HostageCallout.config["NegotiationOptions"];
            foreach (JObject negotiationOption in negotiationOptions)
            {
                MenuItem newItem = new MenuItem((string)negotiationOption["ButtonText"]);
                _menu.AddMenuItem(newItem);
                speechOptions.Add(newItem);
            }

            _menu.OnItemSelect += async (menu, item, index) =>
            {
                JObject obj = null;
                foreach (JObject negotiationOption in negotiationOptions)
                {
                    if ((string)negotiationOption["ButtonText"] == item.Text)
                    {
                        obj = negotiationOption;
                    }
                }
                JArray dialogue = (JArray)obj["Dialogue"];
                foreach (var menuItem in _menu.GetMenuItems())
                {
                    menuItem.Enabled = false;
                }
                foreach (string speech in dialogue)
                {
                    int time = speech.Length * 100;
                    BaseScript.TriggerEvent("KiloHostageCallout:ShowDialogue", speech, time, 50f);
                    await BaseScript.Delay(time);
                }
                HostageCallout.playerPoints += (int)obj["SuccessPoints"];
                if (HostageCallout.playerPoints >= (int)HostageCallout.config["PointsToSucceed"])
                {
                    TriggerEvent("KiloHostageCallout:ReleaseHostage");
                } else if (HostageCallout.playerPoints <= (-1 * (int)HostageCallout.config["PointsToSucceed"]))
                {
                    TriggerEvent("KiloHostageCallout:KillHostage");
                }
                foreach (var menuItem in _menu.GetMenuItems())
                {
                    if (menuItem == item)
                    {
                        if ((bool)obj["Repeatable"])
                            menuItem.Enabled = true;
                    }
                    else
                        menuItem.Enabled = true;
                }
            };
    }
    public script()
    {
        HostageCallout.eventHandlers = EventHandlers;
        EventHandlers["KiloHostage::ResetEventHandlers"] += new Action(() =>
        {
            HostageCallout.eventHandlers = EventHandlers;
        });
        HostageCallout.config = Utils.GetConfig();
        HostageCallout.EnableAI = (bool)HostageCallout.config["EnableAI"];
        if (!(bool)HostageCallout.EnableAI)
        {
            StartNonAI();
        }
        else
        {
            if ((bool)HostageCallout.config["EnablePlayerReporting"])
            {
                API.RegisterCommand("kiloreport", new Action<int, List<object>, string>((source, EventArgs, rawCommand) =>
                {
                    string message = "";
                    foreach (var eventArg in EventArgs)
                        message = message + " " + eventArg;
                    BaseScript.TriggerServerEvent("KiloHostage::PlayerReportToWebhook", message);
                }), false);    
            }
            API.RegisterCommand("speak", new Action<int, List<object>, string>((source, EventArgs, rawCommand) =>
            {
                Debug.WriteLine("command");
                string message = "";
                foreach (var eventArg in EventArgs)
                {
                    message = message + " " + eventArg.ToString();
                }

                HandleCommandSpeak(message);
            }), false);
        }
    }

    private void HandleCommandSpeak(string message)
    {
        if (!HostageCallout.EnableAI) return;
        if (HostageCallout.aitalking)
        {
             HintHelpDisplay(
                "You feel like you shouldn't interrupt the suspect, so you hold yourself back from speaking",
                5000);
            
        } else
            BaseScript.TriggerServerEvent("KiloHostage::SendMessageToAI",message);
    }

    private async Task HintHelpDisplay(string text, int duration)
    {
        API.BeginTextCommandDisplayHelp("STRING");
        API.AddTextComponentString(text);
        API.EndTextCommandDisplayHelp(duration, false, true, 0);
    }
    
}