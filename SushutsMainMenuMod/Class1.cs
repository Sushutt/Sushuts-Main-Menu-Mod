using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace ReplaceMenu
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class ReplaceMenu : MonoBehaviour
    {
        bool canSpin;
        GameObject theShipItself;
        string gamePath;
        float rotationSpeed;
        // Gets all stock craft.
        public List<string> getStockCraft()
        {
            List<string> allStockCraft = new List<string>();

            DirectoryInfo VABDIR = new DirectoryInfo(gamePath + "/Ships/VAB");
            FileInfo[] VABINFO = VABDIR.GetFiles("*.craft");

            DirectoryInfo SPHDIR = new DirectoryInfo(gamePath + "/Ships/SPH");
            FileInfo[] SPHINFO = SPHDIR.GetFiles("*.craft");

            Debug.Log("Got paths to stock craft.");
            Debug.Log(VABINFO);
            Debug.Log(SPHINFO);

            for (int i = 0; i < VABINFO.Length; i++)
            {
                string currentFile = VABINFO[i].Name;
                //Debug.Log(i);
                //Debug.Log(currentFile);
                allStockCraft.Add("Ships/VAB/" + currentFile);
            }

            for (int i = 0; i < SPHINFO.Length; i++)
            {
                string currentFile = SPHINFO[i].Name;
                //Debug.Log(i);
                //Debug.Log(currentFile);
                allStockCraft.Add("Ships/SPH/" + currentFile);
            }

            Debug.Log(allStockCraft);

            return allStockCraft;
        }

        public List<string> getPlayerMadeCraft()
        {
            List<string> allPlrCraft = new List<string>();

            DirectoryInfo PLRSAVESDIR = new DirectoryInfo(gamePath + "/saves/");
            DirectoryInfo[] PLRSAVESINFO = PLRSAVESDIR.GetDirectories();

            Debug.Log("Getting Saves...");
            for (int i = 0; i < PLRSAVESINFO.Length; i++)
            {
                string currentSave = PLRSAVESINFO[i].Name;

                if (System.IO.Directory.Exists(gamePath + "/saves/" + currentSave + "/Ships"))
                {
                    DirectoryInfo VABDIR = new DirectoryInfo(gamePath + "/saves/" + currentSave + "/Ships/VAB");
                    FileInfo[] VABINFO = VABDIR.GetFiles("*.craft");

                    DirectoryInfo SPHDIR = new DirectoryInfo(gamePath + "/saves/" + currentSave + "/Ships/SPH");
                    FileInfo[] SPHINFO = SPHDIR.GetFiles("*.craft");

                    Debug.Log("Getting VAB craft in save: " + currentSave);
                    for (int a = 0; a < VABINFO.Length; a++)
                    {
                        string currentFile = VABINFO[a].Name;
                        Debug.Log(i);
                        Debug.Log(currentFile);
                        allPlrCraft.Add("saves/" + currentSave + "/Ships/VAB/" + currentFile);
                    }
                    Debug.Log("Getting SPH craft in save: " + currentSave);
                    for (int a = 0; a < SPHINFO.Length; a++)
                    {
                        string currentFile = SPHINFO[a].Name;
                        Debug.Log(i);
                        Debug.Log(currentFile);
                        allPlrCraft.Add("saves/" + currentSave + "/Ships/SPH/" + currentFile);
                    }
                }
            }

            //Debug.Log("Got paths to stock craft.");
            //Debug.Log(VABINFO);
            //Debug.Log(SPHINFO);

            //allPlrCraft.ForEach(Debug.Log);

            return allPlrCraft;
        }

        public void Start()
        {
            Debug.Log("Sushut's Main Menu (START)");
            // Get the game's path.
            gamePath = KSPUtil.ApplicationRootPath;

            Debug.Log("Reading settings file...");

            string StockOrCustom;
            string OverrideURL;
            Vector3 VAB_POS;
            Vector3 SPH_POS;

            if (System.IO.File.Exists(gamePath + "GameData/SushutsMainMenu/Settings.cfg") == true)
            {
                ConfigNode settingsConfig = ConfigNode.Load(gamePath + "GameData/SushutsMainMenu/Settings.cfg");
                ConfigNode settingsNode = settingsConfig.GetNode("SushutsMainMenu_Settings");
                Debug.Log(settingsConfig);
                StockOrCustom = settingsNode.GetValue("CraftTypes").ToLower();
                OverrideURL = settingsNode.GetValue("OverrideCraftURL");
                rotationSpeed = float.Parse(settingsNode.GetValue("RotationSpeed"));

                // Get VAB craft position. Thanks Stack Overflow.
                string VAB_POS_STR = settingsNode.GetValue("PositionVAB");
                float[] VAB_POS_FLOAT = VAB_POS_STR.Split(new string[] { ", " }, StringSplitOptions.None).Select(x => float.Parse(x)).ToArray();
                VAB_POS = new Vector3(VAB_POS_FLOAT[0], VAB_POS_FLOAT[1], VAB_POS_FLOAT[2]);

                // Get SPH craft position.
                string SPH_POS_STR = settingsNode.GetValue("PositionVAB");
                float[] SPH_POS_FLOAT = SPH_POS_STR.Split(new string[] { ", " }, StringSplitOptions.None).Select(x => float.Parse(x)).ToArray();
                SPH_POS = new Vector3(SPH_POS_FLOAT[0], SPH_POS_FLOAT[1], SPH_POS_FLOAT[2]);
            }
            else
            {
                StockOrCustom = "both";
                OverrideURL = "";
                rotationSpeed = -0.25f;
                VAB_POS = new Vector3(4, -1, 7);
                SPH_POS = new Vector3(3, -1, 7);
            }

            Debug.Log(StockOrCustom);
            Debug.Log(OverrideURL);

            Debug.Log("Loading random ship...");

            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

            // Deletes the mun scene because it sucks and I hate it. (Thanks Kopernicus.)
            MainMenu main = FindObjectOfType<MainMenu>();
            MainMenuEnvLogic logic = main.envLogic;
            logic.areas[0].SetActive(false);
            logic.areas[1].SetActive(true);

            GameObject.Find("Kerbals").SetActive(false); // This one does remove those kerbals though.

            Debug.Log(gamePath);

            // Call functions to get all craft saved in the game. (Including stock craft.)
            List<string> allStockCraft = getStockCraft();
            List<string> allPlrCraft = getPlayerMadeCraft();

            // Add both together.
            List<string> allCraft = new List<string>();
            allCraft.AddRange(allStockCraft);
            allCraft.AddRange(allPlrCraft);

            // Merge player craft with stock craft. Unless if one doesn't want to.
            // allPlrCraft.AddRange(allStockCraft);
            // allPlrCraft.ForEach(Debug.Log);

            // Pick random craft.
            String RandomCraftName = "";
            // Check if it's been overriden or not.
            // Also check if the overriden URL exists.
            if (System.IO.File.Exists(gamePath + OverrideURL) == false)
            {
                // Switch statements are so yummy...
                switch (StockOrCustom)
                {
                    case "both":
                        RandomCraftName = allCraft[UnityEngine.Random.Range(0, allCraft.Count)];
                        break;
                    case "stock":
                        RandomCraftName = allStockCraft[UnityEngine.Random.Range(0, allStockCraft.Count)];
                        break;
                    case "custom":
                        RandomCraftName = allPlrCraft[UnityEngine.Random.Range(0, allPlrCraft.Count)];
                        break;
                    default:
                        RandomCraftName = allCraft[UnityEngine.Random.Range(0, allCraft.Count)];
                        break;
                }
            }
            else
            {
                RandomCraftName = OverrideURL; // Not so random now huh?
            }

            Debug.Log(RandomCraftName);

            ShipConstruct shipThing = ShipConstruction.LoadShip(RandomCraftName); // The big boy..
            String root = shipThing.parts[0].name; // Get the root name.
            theShipItself = GameObject.Find(root); // Get the ship's GameObject using the root name because I'm not quite sure how else to do it.
            // Move and rotate the thing to a more "viewable" position.
            // Also check if it's in VAB or SPH because those have different orientations for some reason.
            EditorFacility facility = shipThing.shipFacility;
            if (facility == EditorFacility.SPH)
            {
                theShipItself.transform.position = SPH_POS;
                theShipItself.transform.Rotate(Vector3.forward * 190);
            }
            else if (facility == EditorFacility.VAB)
            {
                theShipItself.transform.position = VAB_POS;
                theShipItself.transform.Rotate(Vector3.forward * 210);
                theShipItself.transform.Rotate(Vector3.left * 90);
            }

            Debug.Log(root);
            Debug.Log(shipThing);
            Debug.Log("Random ship succesfully loaded!");

            canSpin = true;
            Debug.Log("Sushut's Main Menu (END)");
        }
        // Spin it.
        public void Update()
        {
            if (canSpin == true)
            {
                float rotation = Time.deltaTime * rotationSpeed;
                theShipItself.transform.Rotate(Vector3.up * rotation);
            }
        }
    }
}