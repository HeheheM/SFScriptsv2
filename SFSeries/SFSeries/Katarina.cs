﻿/* SFKatarina
 *  ________________________  __.       __               .__               
 * /   _____/\_   _____/    |/ _|____ _/  |______ _______|__| ____ _____   
 * \_____  \  |    __) |      < \__  \\   __\__  \\_  __ \  |/    \\__  \  
 * /        \ |     \  |    |  \ / __ \|  |  / __ \|  | \/  |   |  \/ __ \_
 * /_______  / \___  /  |____|__ (____  /__| (____  /__|  |__|___|  (____  /
 *         \/      \/           \/    \/          \/              \/     \/ 
 * 
 * Credits:
 * Snorflake - Making it
 * Fluxy - Re-writing ward jump & Teaching me about vectors and movement packets
 * */

using Color = System.Drawing.Color;

#region References

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

// By iSnorflake
namespace SFSeries
{
    internal class Katarina
    {

#endregion

        #region Declares

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu Config;
        private static Obj_AI_Hero _player;


        // ReSharper disable once UnusedParameter.Local
        public Katarina()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        #endregion

        #region OnGameLoad
        static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Loaded 1");
            _player = ObjectManager.Player;
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);



            Game.PrintChat("Katarina Loaded! By iSnorflake V2");
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            //Create the menu
            Config = new Menu("Katarina", "Katarina", true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            //Add the targer selector to the menu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);



            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm")); // creds tc-crew
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "Use Q").SetValue(
                        true));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "Use W").SetValue(
                        true));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "Freeze!").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            var waveclear = new Menu("Waveclear", "WaveclearMenu");
            waveclear.AddItem(new MenuItem("useQW", "Use Q?").SetValue(true));
            waveclear.AddItem(new MenuItem("useWW", "Use W?").SetValue(true));
            waveclear.AddItem(new MenuItem("Waveclear", "Waveclear").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.AddSubMenu(waveclear); // Thanks to ChewyMoon for the idea of doing the menu this way

            // Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("KillstealQ", "Killsteal with Q").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("Escape", "Escape").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));

            // Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.OrangeRed))));
            Config.AddSubMenu(new Menu("Exploits", "Exploits"));
            Config.SubMenu("Exploits").AddItem(new MenuItem("QNFE", "Q No-Face").SetValue(true));
            // Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E Range").SetValue(new Circle(true, Color.FromArgb(150, Color.DodgerBlue))));

            Config.AddToMainMenu();
            //Add the events we are going to use
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;



        }
        #endregion

        #region OnGameUpdate
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_player.IsDead) return;
            if (IsEnemyInRange()) // If an enemy is in range and im ultimating - dont cancel the ult before their dead
                if (ObjectManager.Player.IsChannelingImportantSpell()) return;
            if (!IsEnemyInRange() && ObjectManager.Player.IsChannelingImportantSpell()) // If the ult isnt hitting anyone
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, ObjectManager.Player); // Cancels ult
            }
            Orbwalker.SetAttacks(true);
            Orbwalker.SetMovement(true);
            var useQks = Config.Item("KillstealQ").GetValue<bool>() && Q.IsReady();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (useQks)
                Killsteal();
            if (Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                Farm();
            if (Config.Item("Waveclear").GetValue<KeyBind>().Active)
            {
                WaveClear();
            }
            Escape();
        }
        #endregion

        #region Farm
        private static void Farm() // Credits TC-CREW
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = Config.Item("UseQFarm").GetValue<bool>();
            var useW = Config.Item("UseWFarm").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var minion in from minion in allMinions where minion != null where _player.Distance3D(minion) < Q.Range where Q.IsKillable(minion) select minion)
                {
                    Q.CastOnUnit(minion);
                    return;
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range) && minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.W))) return;

                W.Cast();
            }
        }
        #endregion

        #region WaveClear
        public static void WaveClear()
        {
            if (!Orbwalking.CanMove(40)) return;

            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var useQ = Config.Item("useQW").GetValue<bool>();
            var useW = Config.Item("useWW").GetValue<bool>();
            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions.Where(minion => minion.IsValidTarget(Q.Range)))
                {
                    Q.CastOnUnit(minion, Config.Item("QNFE").GetValue<bool>());
                    return;
                }
            }
            else if (useW && W.IsReady())
            {
                if (!allMinions.Any(minion => minion.IsValidTarget(W.Range))) return;
                W.Cast();
            }
        }
        #endregion

        #region Combo
        private static void Combo()
        {
            Orbwalker.SetAttacks(true);
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;

            if (GetDamage(target) > target.Health)
            {
                if (!_player.IsChannelingImportantSpell())
                {

                    if (Q.IsReady() && _player.Distance(target) < Q.Range + target.BoundingRadius)
                        Q.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
                    if (E.IsReady() && _player.Distance(target) < E.Range + target.BoundingRadius)
                        E.CastOnUnit(target, Config.Item("QNFE").GetValue<bool>());
                    if (W.IsReady() && _player.Distance(target) < W.Range)
                        W.Cast();
                }
                if (R.IsReady() && _player.Distance(target) < (R.Range - 200))
                    R.Cast();





            }
            else
            {
                if (ObjectManager.Player.Distance(target) < Q.Range && Q.IsReady())
                    Q.CastOnUnit(target, true);

                if (Config.Item("ComboActive").GetValue<KeyBind>().Active &&
                    ObjectManager.Player.Distance(target) < E.Range && E.IsReady())
                    E.CastOnUnit(target);

                if (ObjectManager.Player.Distance(target) < W.Range && W.IsReady())
                    W.Cast();
            }
        }
        #endregion

        #region OnDraw
        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(_player.Position, spell.Range, menuItem.Color);
                // Drawing.DrawText(playerPos[0] - 65, playerPos[1] + 20, drawUlt.Color, "Hit R To kill " + UltTarget + "!");

            }
            //Drawing tempoarily disabled
        }
        #endregion

        #region Killsteal
        private static void Killsteal() // Creds to TC-Crew
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(Q.Range)))
            {
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range && _player.GetSpellDamage(hero, SpellSlot.Q) >= hero.Health)
                {
                    Q.CastOnUnit(hero, Config.Item("QNFE").GetValue<bool>());

                }
            }
        }
        #endregion

        #region Escape
        private static void Escape() // HUGE CREDITS TO FLUXY FOR FIXING THIS
        {
            var basePos = _player.Position.To2D();
            var newPos = (Game.CursorPos.To2D() - _player.Position.To2D());
            var finalVector = basePos + (newPos.Normalized() * (560));
            var movePos = basePos + (newPos.Normalized() * (100));
            if (!Config.Item("Escape").GetValue<KeyBind>().Active) return;
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, movePos.To3D());
            if (!E.IsReady()) return;
            var castWard = true;
            foreach (var esc in ObjectManager.Get<Obj_AI_Base>().Where(esc => esc.Distance(ObjectManager.Player) <= E.Range))
            {
                if (Vector2.Distance(Game.CursorPos.To2D(), esc.ServerPosition.To2D()) <= 175)
                {
                    E.Cast(esc, true);
                    castWard = false;
                }
                if (!esc.Name.Contains("Ward") || !(Vector2.Distance(finalVector, esc.ServerPosition.To2D()) <= 175))
                    continue;
                E.Cast(esc, true);
                castWard = false;
            }
            var ward = FindBestWardItem();
            if (ward != null && castWard)
            {
                ward.UseItem(finalVector.To3D());
            }
        }
        #endregion

        #region Ward jump stuff

        private static SpellDataInst GetItemSpell(InventorySlot invSlot)
        {
            return ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => (int)spell.Slot == invSlot.Slot + 4);
        }
        private static InventorySlot FindBestWardItem()
        {
            InventorySlot slot = Items.GetWardSlot();
            if (slot == default(InventorySlot)) return null;

            SpellDataInst sdi = GetItemSpell(slot);

            if (sdi != default(SpellDataInst) && sdi.State == SpellState.Ready)
            {
                return slot;
            }
            return null;
        }
        #endregion

        #region GetDamage
        private static double GetDamage(Obj_AI_Base enemy) // Creds to TC-Crew
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R, 1);
            return (float)damage;
        }
        private static bool IsEnemyInRange() // Checks if an enemy is in range of my ultimate.
        {
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            if (target == null) return false;
            return true;
        }


    }
}
        #endregion