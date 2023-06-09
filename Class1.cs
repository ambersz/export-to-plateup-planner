﻿using Kitchen;
using Kitchen.Layouts;
using KitchenData;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LZString;

namespace LinkToPlanner
{
    public class CustomSystem : StartOfDaySystem, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();

        }

        protected override void OnUpdate()
        {
            SerializeLayoutWalls();
            this.Enabled = false;
            return;
        }
        static string wallPacking = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";
        static void LogVector(Vector3 vector)
        {
            // x is x, y floor to ceiling, z is "vertical" in the restaurant
            Debug.Log($"({vector.x},{vector.y},{vector.z})");
        }

        // map Planner's appliance code to GDO ID
        public static Dictionary<int, string> applianceMap = new (string, int)[]{
            ("60", 505496455),
            ("eY", -1357906425),
            ("AY", -1440053805),
            ("Z9", 1329097317),
            ("oH", -1013770159),
            ("2V", 2127051779),
            ("Qs", -1632826946),
            ("70", -1855909480),
            ("n2", 481495292),
            ("0R", 1551609169),
            ("Ad", 1351951642),
            ("3D", 1765889988),
            ("6O", -1495393751),
            ("VX", 1776760557),
            ("HD", -1993346570),
            ("1Z", -751458770),
            ("1Z", -1723340146),
            ("9V", -2091039911),
            ("9V", -2147057861),
            ("fU", 1973114260),
            ("w5", -1906799936),
            ("sC", -1238047163),
            ("BM", -1029710921),
            ("Dg", -1462602185),
            ("zg", 459840623),
            ("ze", -1248669347),
            ("E2", -1573577293),
            ("qV", 756364626),
            ("H5", 532998682),
            ("e6", 1921027834),
            ("5V", -770041014),
            ("ud", -1448690107),
            ("96", 1266458729),
            ("O9", 1154757341),
            ("UQ", 862493270),
            ("5d", -1813414500),
            ("F5", -571205127),
            ("5T", -729493805),
            ("4K", -272437832),
            ("4K", 1586911545),
            ("CR", 1446975727),
            ("8B", 1139247360),
            ("pj", 238041352),
            ("UJ", -1817838704),
            ("Gt", -246383526),
            ("hM", -1610332021),
            ("2Q", -1311702572),
            ("JD", -1068749602),
            ("yi", -905438738),
            ("FG", 1807525572),
            ("uW", 269523389),
            ("dG", -1573812073),
            ("Dc", 759552160),
            ("Ar", -452101383),
            ("zQ", -117339838),
            ("NV", 961148621),
            ("Ls", -1735137431),
            ("Ls", -609358791),
            ("AG", 925796718),
            ("vu", -1533430406),
            ("SS", 1193867305),
            ("uW", -484165118),
            ("5B", -1097889139),
            ("Ja", 1834063794),
            ("WU", -1963699221),
            ("2o", -1434800013),
            ("Qi", -1201769154),
            ("ET", -1506824829),
            ("0s", -1353971407),
            ("ze", -996680732),
            ("2M", 1653145961),
            ("2M", 434150763),
            ("ao", 380220741),
            ("m2", 1313469794),
            ("NH", -957949759),
            ("2A", 235423916),
            ("94", 314862254),
            ("wn", -1857890774),
            ("ot", -759808000),
            ("31", 1656358740),
            ("r6", 639111696),
            ("Yn", 1358522063),
            ("J1", 221442949),
            ("mD", 1528688658),
            ("zZ", 2080633647),
            ("CZ", 446555792),
            ("qB", 938247786),
            ("qC", 1648733244),
            ("qB", -1979922052),
            ("tV", -3721951),
            ("T2", -34659638),
            ("cJ", -203679687),
            ("GM", -2019409936),
            ("lq", 209074140),
            ("WS", 1738351766),
            ("1P", 624465484),
            ("kv", 2023704259),
            ("ZE", 723626409),
            ("kF", 1796077718),
            ("py", 230848637),
            ("3G", 1129858275),
            ("1g", -214126192),
            ("W1", 1083874952),
            ("v2", 1467371088),
            ("Nt", 1860904347),
            ("bZ", -266993023),
            ("0R", 1159228054),
            ("IX", 303858729),
            ("zd", -2133205155),
            ("fU", -667884240),
            ("96", -349733673),
            ("96", 1836107598),
            ("96", 369884364),
            ("jC", 976574457),
  }.ToDictionary(a => a.Item2, a => a.Item1);
        public string SerializeLayoutWalls()
        {
            var bounds = base.Bounds;
            LogVector(bounds.min);
            LogVector(bounds.max);
            LogVector(base.GetFrontDoor());
            int height = (int)(bounds.max.z - bounds.min.z+1);
            int width = (int)(bounds.max.x - bounds.min.x+1);
            string layoutString = $"v2 {height}x{width} ";
            string applianceString = "";
            IEnumerable<int> wallCodes = new List<int>();
            for (float i = bounds.max.z; i >=bounds.min.z; i--)
            {
                List<int> verticalWallString = new List<int>();
                List<int> horizontalWallString = new List<int>();
                for (float j = bounds.min.x; j<=bounds.max.x; j++)
                {
                    var position = new Vector3 { x=j, z=i };
                    // add appliance or empty square to appliance string
                    var applianceEntity = base.GetPrimaryOccupant(position);
                    CAppliance appliance;
                    string applianceCode;
                    if (EntityManager.RequireComponent<CAppliance>(applianceEntity, out appliance)&& applianceMap.ContainsKey(appliance.ID))
                    {
                        // TODO get appliance rotation
                        applianceCode = applianceMap[appliance.ID] +"u";

                    }
                    else
                    {
                        applianceCode = "00u";
                    }
                    applianceString += applianceCode;

                    // check horizontal adjacencies for wall presence
                    if (j<bounds.max.x)
                    {
                        var right = position + (Vector3)LayoutHelpers.Directions[3];
                        if (GetRoom(position) == GetRoom(right))
                        {
                            // same room, must be no walls!
                            verticalWallString.Add(0b11);

                        }
                        else
                        if (CanReach(position, right))
                        {
                            // can target into the next room, must be a half wall
                            // or maybe a door............? Don't know how to tell
                            verticalWallString.Add(0b10);
                        }
                        else
                        {
                            // different rooms and can't target, must be an actual wall
                            verticalWallString.Add(0b01);
                        }
                    }
                    // check vertical adjacencies for wall presence
                    if (i>bounds.min.z)
                    {
                        var down = position + (Vector3)LayoutHelpers.Directions[1];
                        if (GetRoom(position) == GetRoom(down))
                        {
                            // same room, must be no walls!
                            horizontalWallString.Add(0b11);

                        }
                        else
if (CanReach(position, down))
                        {
                            // can target into the next room, must be a half wall
                            // or maybe a door............? Don't know how to tell
                            horizontalWallString.Add(0b10);
                        }
                        else
                        {
                            // different rooms and can't target, must be an actual wall
                            horizontalWallString.Add(0b01);
                        }
                    }
                }

                // append wall strings in correct order
                wallCodes = wallCodes.Concat(verticalWallString);
                wallCodes = wallCodes.Concat(horizontalWallString);

            }
            string wallString = "";
            int piece = 0;
            int accumulator = 0;
            foreach (var wallCode in wallCodes)
            {
                accumulator = accumulator | (wallCode << piece*2);
                piece++;
                if (piece == 3)
                {
                    wallString+=wallPacking[accumulator];
                    piece = 0;
                    accumulator = 0;
                }
            }
            if (piece !=0)
            {
                wallString+=wallPacking[accumulator];
                piece = 0;
                accumulator = 0;
            }
            layoutString += applianceString;
            layoutString += " ";
            layoutString += wallString;
            // string compressed = LZString.LzString.CompressToEncodedUriComponent(layoutString);
            System.Diagnostics.Process.Start($"https://plateupplanner.github.io/workspace#{layoutString}");
            // System.Diagnostics.Process.Start($"https://plateupplanner.github.io/workspace#{compressed}");
            this.Enabled = false;
            Debug.Log(layoutString);
            return layoutString;
        }

        public static void AppendToFile(string line)
        {
            StreamWriter file = new StreamWriter("plannerdata", append: true);
            file.WriteLine(line);
            file.Dispose();
        }
    }
}



/*
In case I need to get appliance ids from names again:

                     var appliances = GameData.Main.Get<Appliance>();
                    foreach (var a in appliances)
                    {
                        if (applianceMap.ContainsKey(a.Name))
                        {
                            var value = applianceMap[a.Name];
                            int id = a.ID;
                            AppendToFile($"(\"{value}\", \"{id}\"),");
                        }
                    }
                }
//*/
