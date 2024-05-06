using RedLoader;
using SonsSdk;
using UnityEngine;
using System.Collections;
using Sons.Gameplay.Traps;
using SUI;
using TheForest.Utils;

namespace TrapReset
{
    public class TrapReset : SonsMod
    {
        public static bool _firstStart = true;
        public static bool _returnedToTitle = false;
        private static float floatValue;
        private static bool timerShouldRun = false;
        private static bool animalTraps = false;
        private static bool defenseTraps = false;
        private static bool springTraps = false;

        private RedLoader.Coroutines.CoroutineToken resetCoroutine;
        private List<GameObject> existingTraps = new List<GameObject>();

        public TrapReset()
        {
            // Uncomment any of these if you need a method to run on a specific update loop.
            //OnUpdateCallback = MyUpdateMethod;
            //OnLateUpdateCallback = MyLateUpdateMethod;
            //OnFixedUpdateCallback = MyFixedUpdateMethod;
            //OnGUICallback = MyGUIMethod;

            // Uncomment this to automatically apply harmony patches in your assembly.
            //HarmonyPatchAll = true;
        }

        protected override void OnInitializeMod()
        {
            // Do your early mod initialization which doesn't involve game or sdk references here
            Config.Init();
        }

        protected override void OnSdkInitialized()
        {
            // Do your mod initialization which involves game or sdk references here
            // This is for stuff like UI creation, event registration etc.
            SettingsRegistry.CreateSettings(this, null, typeof(Config), callback: OnSettingsUiClosed);
            TrapResetUi.Create();
        }

        protected override void OnGameStart()
        {
            _firstStart = false;
            _returnedToTitle = false;
            // This is called once the player spawns in the world and gains control.
            FirstCheckAndResetTraps();
        }

        protected override void OnSonsSceneInitialized(ESonsScene sonsScene)
        {
            if (sonsScene == ESonsScene.Title)
            {
                if (!_firstStart)
                {
                    _returnedToTitle = true;
                    //RLog.Msg("In Title Screen, not first start, set _returnedToTitle " + _returnedToTitle);
                    //RLog.Msg("Returned to title, stopping coroutine");
                    timerShouldRun = false;
                    return;
                }
                else
                {
                    return;
                }
            }
        }


        public void OnSettingsUiClosed()
        {
            if (!LocalPlayer._instance && _returnedToTitle)
            {
                SonsTools.ShowMessageBox("Oops", "Cant change settings for TrapReset while not In-Game");
                //SonsTools.ShowMessage("Cant change settings for FrankyModMenu while not In-Game", 3f);
                RLog.Error("Cant change settings for TrapReset while not In-Game");
                return;
            }
            else
            {
                CheckAndResetTraps();
            }
        }

        public void FirstCheckAndResetTraps()
        {
            // Get existing traps for each type
            GetTraps();

            // Update boolean values based on config values
            UpdateTrapConfigStates();

            // Reset the timer based on the current configuration
            ResetTimer();

            // Start or stop the reset coroutine based on timerShouldRun
            if (timerShouldRun && resetCoroutine == null)
            {
                resetCoroutine = ResetTraps().RunCoro();
            }
            else if (!timerShouldRun && resetCoroutine != null)
            {
                Coroutines.Stop(resetCoroutine);
                resetCoroutine = null;
            }
        }
        public void CheckAndResetTraps()
        {
            if (Config.CheckForTrapChange.Value) 
            {
                Config.CheckForTrapChange.Value = Config.CheckForTrapChange.DefaultValue;
                GetTraps();
            }

            // Update boolean values based on config values
            UpdateTrapConfigStates();

            // Reset the timer based on the current configuration
            ResetTimer();

            // Start or stop the reset coroutine based on timerShouldRun
            if (timerShouldRun)
            {
                // Only start a new coroutine if resetCoroutine is null
                if (resetCoroutine == null)
                {
                    resetCoroutine = ResetTraps().RunCoro();
                }
            }
            else
            {
                // Stop the coroutine if timerShouldRun is false
                resetCoroutine?.Stop(); // Stop the coroutine if it's currently running
            }
        }

        private void GetTraps()
        {
            // Clear the existing list of traps
            existingTraps.Clear();

            // Find all GameObjects in the scene
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            // Add game objects with specific names to the list
            foreach (GameObject obj in allObjects)
            {
                switch (obj.name)
                {
                    case "MaceTrapStructure(Clone)":
                    case "GrindTrapStructure(Clone)":
                    case "TrapFlySwatterStructure(Clone)":
                    case "TrapBoneMakerStructure(Clone)":
                    case "SpinTrapStructure(Clone)":
                    case "UberTrapStructure(Clone)":
                    case "SpringTrapStructure(Clone)":
                    case "HangGliderLauncherStructure(Clone)":
                    case "TrapSmallAnimalCatcherStructure(Clone)":
                        existingTraps.Add(obj);
                        break;
                    default:
                        // Exclude other objects
                        break;

                }
            }
            
            // Update boolean flags based on configuration values
            UpdateTrapConfigStates();
        }

        private void UpdateTrapConfigStates()
        {
            // Update boolean values based on config values
            defenseTraps = Config.DefenseTraps.Value;
            springTraps = Config.SpringTraps.Value;
            animalTraps = Config.AnimalTrap.Value;

            // Set timerShouldRun to true if any of the traps are enabled, else set to false
            timerShouldRun = defenseTraps || springTraps || animalTraps;
        }

        public void ResetTimer()
        {
            resetCoroutine?.Stop(); // Stop the coroutine if it's currently running

            if (timerShouldRun)
            {
                if (float.TryParse(Config.ResetTimer.Value, out floatValue))
                {
                    // Restart the reset coroutine with the new timer value
                    resetCoroutine = ResetTraps().RunCoro();
                }
                else
                {
                    RLog.Error("Value for Config.ResetTimer did not parse as a valid float - Contact Franky");
                }
            }
        }


        IEnumerator ResetTraps()
        {
            Sons.Gameplay.Traps.MaceTrap maceTrap;
            Sons.Gameplay.Traps.GrindTrap grindTrap;
            Sons.Gameplay.Traps.FlySwatterTrap flySwatterTrap;
            Sons.Gameplay.Traps.BoneMakerTrap boneMakerTrap;
            Sons.Gameplay.Traps.SpinTrap spinTrap;
            Sons.Gameplay.Traps.UberTrap uberTrap;
            Sons.Gameplay.Traps.SmallAnimalCatcherTrap smallAnimalCatcherTrap;
            Sons.Gameplay.Traps.SpringTrap springTrap;

            // Loop infinitely until timerShouldRun becomes false
            while (timerShouldRun)
            {
                //RLog.Msg("Resetting Traps in " + floatValue + " seconds");
                yield return new WaitForSeconds(floatValue);

                // Reset traps if necessary
                foreach (GameObject trap in existingTraps)
                {
                    // Check if the trap GameObject is null
                    if (trap == null)
                    {
                        continue; // Skip to the next trap if it's null
                    }
                    // Check if the trap has any of the individual trap controllers
                    switch (trap.name)
                    {
                        case "MaceTrapStructure(Clone)":
                            maceTrap = trap.GetComponent<Sons.Gameplay.Traps.MaceTrap>();
                            if (maceTrap != null && defenseTraps && maceTrap.IsTriggered)
                            {
                                maceTrap.ResetTrap();
                            }
                            break;

                        case "GrindTrapStructure(Clone)":
                            grindTrap = trap.GetComponent<Sons.Gameplay.Traps.GrindTrap>();
                            if (grindTrap != null && defenseTraps && grindTrap.IsTriggered)
                            {
                                grindTrap.ResetTrap();
                            }
                            break;

                        case "TrapFlySwatterStructure(Clone)":
                            flySwatterTrap = trap.GetComponent<Sons.Gameplay.Traps.FlySwatterTrap>();
                            if (flySwatterTrap != null && defenseTraps && flySwatterTrap.IsTriggered)
                            {
                                flySwatterTrap.ResetTrap();
                            }
                            break;
                        case "TrapBoneMakerStructure(Clone)":
                            boneMakerTrap = trap.GetComponent<Sons.Gameplay.Traps.BoneMakerTrap>();
                            if (boneMakerTrap != null && defenseTraps && boneMakerTrap.IsTriggered)
                            {
                                boneMakerTrap.ResetTrap();
                            }
                            break;

                        case "SpinTrapStructure(Clone)":
                            spinTrap = trap.GetComponent<Sons.Gameplay.Traps.SpinTrap>();
                            if (spinTrap != null && defenseTraps && spinTrap.IsTriggered)
                            {
                                spinTrap.ResetTrap();
                            }
                            break;

                        case "UberTrapStructure(Clone)":
                            uberTrap = trap.GetComponent<Sons.Gameplay.Traps.UberTrap>();
                            if (uberTrap != null && defenseTraps && uberTrap.IsTriggered)
                            {
                                uberTrap.ResetTrap();
                            }
                            break;

                        case "TrapSmallAnimalCatcherStructure(Clone)":
                            smallAnimalCatcherTrap = trap.GetComponent<Sons.Gameplay.Traps.SmallAnimalCatcherTrap>();
                            if (smallAnimalCatcherTrap != null && animalTraps && smallAnimalCatcherTrap.IsTriggered)
                            {
                                smallAnimalCatcherTrap.ResetTrap();
                            }
                            break;

                        case "SpringTrapStructure(Clone)":
                            springTrap = trap.GetComponent<Sons.Gameplay.Traps.SpringTrap>();
                            if (springTrap != null && springTraps && springTrap.IsTriggered)
                            {
                                springTrap.ResetTrap();
                            }
                            break;
                        
                        case "HangGliderLauncherStructure(Clone)":
                            springTrap = trap.GetComponent<Sons.Gameplay.Traps.SpringTrap>();
                            if (springTrap != null && springTraps && springTrap.IsTriggered)
                            {
                                springTrap.ResetTrap();
                            }
                            break;

                        
                        default:
                            // Handle default case (if needed)
                            break;
                    }

                    // Update timerShouldRun based on trap configurations
                    UpdateTrapConfigStates();

                    // If timerShouldRun is now false, exit the coroutine
                    if (!timerShouldRun)
                    {
                        break;
                    }
                }
            }
        }
    }
}
