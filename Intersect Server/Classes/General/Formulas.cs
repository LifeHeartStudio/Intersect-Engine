﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Intersect_Library;
using Intersect_Server.Classes.Entities;
using NCalc;

namespace Intersect_Server.Classes.General
{
    public static class Formulas
    {
        private static string PhysicalDamage = "";
        private static string MagicDamage = "";
        private static string TrueDamage = "";
        public static bool LoadFormulas()
        {
            if (!File.Exists(Path.Combine("resources", "formulas.xml")))
            {
                File.WriteAllText(Path.Combine("resources","formulas.xml"),Properties.Resources.formulas);
                Console.WriteLine("Formulas.xml missing. Generated default formulas file.");
            }
            var formulas = new XmlDocument();
            var formulaXml = File.ReadAllText("resources/formulas.xml");
            try
            {
                formulas.LoadXml(formulaXml);
                PhysicalDamage = GetXmlStr(formulas, "//Formulas/PhysicalDamage");
                MagicDamage = GetXmlStr(formulas, "//Formulas/MagicDamage");
                TrueDamage = GetXmlStr(formulas, "//Formulas/TrueDamage");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading formulas! Please make sure the file exists and is free on syntax errors.");
                return false;
            }
        }

        private static string GetXmlStr(XmlDocument xmlDoc, string xmlPath)
        {
            var selectSingleNode = xmlDoc.SelectSingleNode(xmlPath);
            return selectSingleNode.InnerText;
        }

        public static int CalculateDamage(int baseDamage, DamageType damageType, Stats scalingStat, int scaling, double critMultiplier, Entity attacker, Entity victim)
        {
            var expression = "";
            switch (damageType)
            {
                case DamageType.Physical:
                    expression = PhysicalDamage;
                    break;
                case DamageType.Magic:
                    expression = MagicDamage;
                    break;
                case DamageType.True:
                    expression = TrueDamage;
                    break;
                default:
                    expression = TrueDamage;
                    break;
            }
            Expression e = new Expression(expression);
            e.Parameters["BaseDamage"] = baseDamage;
            e.Parameters["ScalingStat"] = attacker.Stat[(int)scalingStat].Value();
            e.Parameters["ScaleFactor"] = scaling/100f;
            e.Parameters["CritFactor"] = critMultiplier;
            e.Parameters["A_Attack"] = attacker.Stat[(int)Stats.Attack].Value();
            e.Parameters["A_Defense"] = attacker.Stat[(int)Stats.Defense].Value();
            e.Parameters["A_Speed"] = attacker.Stat[(int)Stats.Speed].Value();
            e.Parameters["A_AbilityPwr"] = attacker.Stat[(int)Stats.AbilityPower].Value();
            e.Parameters["A_MagicResist"] = attacker.Stat[(int)Stats.MagicResist].Value();
            e.Parameters["V_Attack"] = victim.Stat[(int)Stats.Attack].Value();
            e.Parameters["V_Defense"] = victim.Stat[(int)Stats.Defense].Value();
            e.Parameters["V_Speed"] = victim.Stat[(int)Stats.Speed].Value();
            e.Parameters["V_AbilityPwr"] = victim.Stat[(int)Stats.AbilityPower].Value();
            e.Parameters["V_MagicResist"] = victim.Stat[(int)Stats.MagicResist].Value();
            e.EvaluateFunction += delegate (string name, FunctionArgs args)
            {
                if (name == "Random")
                {
                    int left = (int)Math.Round((double)args.Parameters[0].Evaluate());
                    int right = (int)Math.Round((double)args.Parameters[1].Evaluate());
                    if (left >= right)
                    {
                        args.Result = left;
                    }
                    else
                    {
                        args.Result = Globals.Rand.Next(left,right+1);
                    }
                }
            };
            double result = (double)e.Evaluate();
            return (int)Math.Round(result);
        }
    }
}
