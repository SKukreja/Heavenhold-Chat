using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using HeavenholdBot.Models;
using HeavenholdBot.Repositories;
using Discord;
using System.Configuration;
using MySqlConnector;
using Interactivity.Pagination;

namespace HeavenholdBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public InteractivityService Interactivity { get; set; }
        private EmojiRepository _emojiList = new EmojiRepository();

        [Command("hero", RunMode = RunMode.Async)]
        public async Task Hero(params string[] name)
        {
            string message = "No Hero Found.";
            if (!name.Equals("") && name != null)
            {
                try
                {
                    HeroInfo heroInfo = Program._heroList.GetHero(name);
                    if (heroInfo != null)
                    {
                        // Define list of pages to store hero information
                        List<PageBuilder> list = new List<PageBuilder>();

                        #region Page 1 - Hero Information
                        string weaponList = "";
                        string description = "";

                        for (int i = 0; i < heroInfo.Acf.BioFields.CompatibleEquipment.Length; i++)
                        {
                            if (i == (heroInfo.Acf.BioFields.CompatibleEquipment.Length - 1))
                                weaponList = weaponList + heroInfo.Acf.BioFields.CompatibleEquipment[i];
                            else
                                weaponList = weaponList + heroInfo.Acf.BioFields.CompatibleEquipment[i] + ", ";
                        }

                        //Get hero bio
                        description = "**Element:** " + heroInfo.Acf.BioFields.Element + System.Environment.NewLine
                            + "**Role:** " + heroInfo.Acf.BioFields.Role + System.Environment.NewLine
                            + "**Species:** " + heroInfo.Acf.BioFields.Species + System.Environment.NewLine
                            + "**Age:** " + heroInfo.Acf.BioFields.Age + System.Environment.NewLine
                            + "**Height:** " + heroInfo.Acf.BioFields.Height + System.Environment.NewLine
                            + "**Weight:** " + heroInfo.Acf.BioFields.Weight + System.Environment.NewLine
                            + "**Released:** " + heroInfo.Acf.BioFields.NaReleaseDate;

                        // Build the page
                        var heroInfoPage = new PageBuilder()
                        .WithTitle(System.Web.HttpUtility.HtmlDecode(heroInfo.Title.Rendered))
                        .WithUrl(heroInfo.Link.ToString())
                        .WithColor(new Color(_emojiList.GetRoleColorCode(heroInfo.Acf.BioFields.Element.ToString())))
                        .WithDescription(description)
                        .WithThumbnailUrl(heroInfo.Acf.Portrait.First().Art.ToString().Remove(heroInfo.Acf.Portrait.First().Art.ToString().Length - 4) + "-150x150.jpg")
                        .AddField("Atk", heroInfo.Acf.StatFields.Atk, true)
                        .AddField("HP", heroInfo.Acf.StatFields.Hp, true)
                        .AddField("Def", heroInfo.Acf.StatFields.Def, true)
                        .AddField("Heal", heroInfo.Acf.StatFields.Heal, true)
                        .AddField("Crit", heroInfo.Acf.StatFields.Crit, true)
                        .AddField("DR", heroInfo.Acf.StatFields.DamageReduction, true);

                        // Add the page to the list
                        list.Add(heroInfoPage);
                        #endregion

                        #region Page 2 - Hero Abilities

                        // Build the page
                        PageBuilder heroAbilitiesPage;

                        // Get hero passives
                        string[] buffs = heroInfo.Acf.AbilityFields.Passives.Split(new string[] { "<br />" }, StringSplitOptions.RemoveEmptyEntries);

                        // Build description with hero information (checks for chain ability to account for 1 star heroes)
                        if (!heroInfo.Acf.AbilityFields.ChainStateTrigger.ToString().ToUpper().Equals("NONE"))
                        {
                            heroAbilitiesPage = new PageBuilder()
                                .WithTitle(System.Web.HttpUtility.HtmlDecode(heroInfo.Title.Rendered))
                            .WithUrl(heroInfo.Link.ToString())
                            .WithColor(new Color(_emojiList.GetRoleColorCode(heroInfo.Acf.BioFields.Element.ToString())))
                            .WithThumbnailUrl(heroInfo.Acf.Portrait.First().Art.ToString().Remove(heroInfo.Acf.Portrait.First().Art.ToString().Length - 4) + "-150x150.jpg")
                            .AddField("Normal Atk - " + heroInfo.Acf.AbilityFields.NormalAtkName, heroInfo.Acf.AbilityFields.NormalAtkDescription.Replace("<br />", System.Environment.NewLine) + System.Environment.NewLine + "_ _" + System.Environment.NewLine + _emojiList.GetEmojiCode(heroInfo.Acf.AbilityFields.ChainStateTrigger.ToString()) + " → " + _emojiList.GetEmojiCode(heroInfo.Acf.AbilityFields.ChainStateResult.ToString()), false)
                            .AddField("Chain Skill - " + heroInfo.Acf.AbilityFields.ChainSkillName, heroInfo.Acf.AbilityFields.ChainSkillDescription.Replace("<br />", System.Environment.NewLine), false)
                            .AddField("Special Ability - " + heroInfo.Acf.AbilityFields.SpecialAbilityName, heroInfo.Acf.AbilityFields.SpecialAbilityDescription.Replace("<br />", System.Environment.NewLine), false)
                            .AddField("Passives", String.Join(System.Environment.NewLine, buffs), false);
                        }
                        else
                        {
                            heroAbilitiesPage = new PageBuilder()
                                .WithTitle(System.Web.HttpUtility.HtmlDecode(heroInfo.Title.Rendered))
                            .WithUrl(heroInfo.Link.ToString())
                            .WithColor(new Color(_emojiList.GetRoleColorCode(heroInfo.Acf.BioFields.Element.ToString())))
                            .WithThumbnailUrl(heroInfo.Acf.Portrait.First().Art.ToString().Remove(heroInfo.Acf.Portrait.First().Art.ToString().Length - 4) + "-150x150.jpg")
                            .AddField("Normal Atk - " + heroInfo.Acf.AbilityFields.NormalAtkName, heroInfo.Acf.AbilityFields.NormalAtkDescription.Replace("<br />", System.Environment.NewLine), false)
                            .AddField("Chain Skill" + heroInfo.Acf.AbilityFields.ChainSkillName, "N/A", false)
                            .AddField("Special Ability", "N/A", false)
                            .AddField("Passives", String.Join(System.Environment.NewLine, buffs), false);
                        }

                        // Add the page to the list
                        list.Add(heroAbilitiesPage);
                        #endregion

                        #region Page 3 - Exclusive Weapon


                        if (heroInfo.Acf.BioFields.ExclusiveWeapon != null)
                        {
                            // Find the weapon
                            ItemInfo itemInfo = Program._itemList.GetItem(heroInfo.Acf.BioFields.ExclusiveWeapon.First().PostTitle.Split(" "));

                            // Build description from item stats
                            string itemStats = "", lbStat = "", itemSubStats = "";

                            // If the item is a costume
                            if (itemInfo.ItemCategories.FirstOrDefault() == 94)
                            {
                                // Check if Super Costume
                                string super = "";
                                if (itemInfo.Acf.Super)
                                {
                                    super += "Super ";
                                }
                                itemStats += super + "Costume" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                            }
                            // If the item is an equipment costume
                            else if (itemInfo.ItemCategories.FirstOrDefault() == 125)
                            {
                                itemStats += "Equipment Costume" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                            }
                            // If item has stats
                            else
                            {
                                // Check the if item is a weapon
                                if (itemInfo.ItemCategories.FirstOrDefault() == 56)
                                {
                                    itemStats += itemInfo.Acf.Rarity + " " + itemInfo.Acf.WeaponType + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                    itemStats += "**" + itemInfo.Acf.DPS + " DPS**" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                    itemStats += "**" + itemInfo.Acf.Element + " Atk** " + itemInfo.Acf.MaxAtk + System.Environment.NewLine;
                                }
                                // If the item is merch
                                else if (itemInfo.ItemCategories.FirstOrDefault() == 124)
                                {
                                    itemStats += itemInfo.Acf.Rarity + " Merch" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                }
                                // If the item is a shield
                                else if (itemInfo.ItemCategories.FirstOrDefault() == 58)
                                {
                                    itemStats += itemInfo.Acf.Rarity + " Shield" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                }
                                // If the item is an accessory
                                else if (itemInfo.ItemCategories.FirstOrDefault() == 57)
                                {
                                    itemStats += itemInfo.Acf.Rarity + " Accessory" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                }
                                // If the item is a card
                                else if (itemInfo.ItemCategories.FirstOrDefault() == 59)
                                {
                                    itemStats += "Card" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                }

                                // Check which stats the item has
                                if(!String.IsNullOrEmpty(itemInfo.Acf.Atk))
                                {
                                    itemStats += "**Atk** +" + itemInfo.Acf.Atk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.Magazine))
                                {
                                    itemStats += "**Magazine Size** " + itemInfo.Acf.Magazine + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.HealFlat))
                                {
                                    itemStats += "**Heal** " + itemInfo.Acf.HealFlat + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.CritChance))
                                {
                                    itemStats += "**Crit Hit Chance** " + itemInfo.Acf.CritChance + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.DefFlat))
                                {
                                    itemStats += "**Def** " + itemInfo.Acf.DefFlat + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.DamageReduction))
                                {
                                    itemStats += "**Damage Reduction** " + itemInfo.Acf.DamageReduction + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.AtkOnKill))
                                {
                                    itemStats += "+" + itemInfo.Acf.AtkOnKill + "% Atk increase on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.HpOnKill))
                                {
                                    itemStats += "+" + itemInfo.Acf.HpOnKill + "% HP recovery on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.ShieldOnKill))
                                {
                                    itemStats += "+" + itemInfo.Acf.ShieldOnKill + "% shield increase on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.ShieldOnStart))
                                {
                                    itemStats += "+" + itemInfo.Acf.ShieldOnStart + "% shield increase on battle start" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.Def))
                                {
                                    itemStats += "**Def** +" + itemInfo.Acf.Def + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.HP))
                                {
                                    itemStats += "**HP** +" + itemInfo.Acf.HP + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.HealPercent))
                                {
                                    itemStats += "**Heal** +" + itemInfo.Acf.HealPercent + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SkillDamage))
                                {
                                    itemStats += "**Skill Damage** +" + itemInfo.Acf.SkillDamage + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SkillRegenSpeed))
                                {
                                    itemStats += "**Weapon Skill Regen Speed** +" + itemInfo.Acf.SkillRegenSpeed + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SkillRegenOnKill))
                                {
                                    itemStats += itemInfo.Acf.SkillRegenOnKill + " seconds of weapon skill Regen time on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.EarthTypeAtk))
                                {
                                    itemStats += "**Earth type Atk** +" + itemInfo.Acf.EarthTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.FireTypeAtk))
                                {
                                    itemStats += "**Fire type Atk** +" + itemInfo.Acf.FireTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.WaterTypeAtk))
                                {
                                    itemStats += "**Water type Atk** +" + itemInfo.Acf.WaterTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.BasicTypeAtk))
                                {
                                    itemStats += "**Basic type Atk** +" + itemInfo.Acf.BasicTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.LightTypeAtk))
                                {
                                    itemStats += "**Light type Atk** +" + itemInfo.Acf.LightTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.DarkTypeAtk))
                                {
                                    itemStats += "**Dark type Atk** +" + itemInfo.Acf.DarkTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.OnHitDamage))
                                {
                                    if(itemInfo.Acf.ExtraDamageType)
                                    {
                                        itemStats += "On hit, extra melee damage of " + itemInfo.Acf.OnHitDamage + "% of DPS once every " + itemInfo.Acf.OnHitDamageSeconds + " second(s)." + System.Environment.NewLine;
                                    }
                                    else
                                    {
                                        itemStats += "On hit, extra ranged damage of " + itemInfo.Acf.OnHitDamage + "% of DPS once every " + itemInfo.Acf.OnHitDamageSeconds + " second(s)." + System.Environment.NewLine;
                                    }
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.OnHitHealAllies))
                                {
                                    itemStats += "On hit, heals all allies by " + itemInfo.Acf.OnHitHealAllies + "% of Heal. Activates once every " + itemInfo.Acf.OnHitHealSeconds + " second(s)." + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.IncreaseDamageAmount))
                                {
                                    if (itemInfo.Acf.IncreaseDamageCondition)
                                    {
                                        itemStats += "Increase damage by " + itemInfo.Acf.IncreaseDamageAmount + " to enemies with HP more than or equal to " + itemInfo.Acf.IncreaseDamageThreshold + "%." + System.Environment.NewLine;
                                    }
                                    else
                                    {
                                        itemStats += "Increase damage by " + itemInfo.Acf.IncreaseDamageAmount + " to enemies with HP less than or equal to " + itemInfo.Acf.IncreaseDamageThreshold + "%." + System.Environment.NewLine;
                                    }
                                }
                                if (itemInfo.Acf.Options.Any(s => s.Contains("negated")))
                                {
                                    string[] negatedOptions = itemInfo.Acf.Options.Where(s => s.Contains("negated")).ToArray();
                                    foreach(string negatedOption in negatedOptions)
                                    {
                                        itemStats += negatedOption + System.Environment.NewLine;
                                    }
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.Lb5Option))
                                {
                                    string equipment_lb5 = itemInfo.Acf.Lb5Option;

                                    if (equipment_lb5 == "Atk (%)"){
                                        lbStat += "**Atk** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                    else if (equipment_lb5 == "HP (%)") {
                                        lbStat += "**HP** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                    else if (equipment_lb5 == "Crit Hit Chance") {
                                        lbStat += "**Crit Hit Chance** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                    else if (equipment_lb5 == "Damage Reduction") {
                                        lbStat += "**Damage Reduction** +" + itemInfo.Acf.Lb5Value;
                                    }
                                    else if (equipment_lb5 == "Def") {
                                        lbStat += "**Def** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                    else if (equipment_lb5 == "Heal (Flat)") {
                                        lbStat += "**Heal** +" + itemInfo.Acf.Lb5Value;
                                    }
                                    else if (equipment_lb5 == "Heal (%)") {
                                        lbStat += "**Heal** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                    else if (equipment_lb5 == "Atk increase on enemy kill") {
                                        lbStat += "+" + itemInfo.Acf.Lb5Value + "% Atk increase on enemy kill";
                                    }
                                    else if (equipment_lb5 == "HP recovery on enemy kill") {
                                        lbStat += "+" + itemInfo.Acf.Lb5Value + "% HP recovery on enemy kill";
                                    }
                                    else if (equipment_lb5 == "Seconds of weapon skill Regen time on enemy kill") {
                                        lbStat += itemInfo.Acf.Lb5Value + " seconds of weapon skill Regen time on enemy kill";
                                    }
                                    else if (equipment_lb5 == "Shield increase on battle start") {
                                        lbStat += "+" + itemInfo.Acf.Lb5Value + "% shield increase on battle start";
                                    }
                                    else if (equipment_lb5 == "Shield increase on enemy kill") {
                                        lbStat += "+" + itemInfo.Acf.Lb5Value + "% shield increease on enemy kill";
                                    }
                                    else if (equipment_lb5 == "Skill Damage") {
                                        lbStat += "**Skill Damage** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                    else if (equipment_lb5 == "Weapon Skill Regen Speed") {
                                        lbStat += "**Weapon Skill Regen Speed** +" + itemInfo.Acf.Lb5Value + "%";
                                    }
                                }

                                // Check which sub stats the item has
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubAtk))
                                {
                                    itemSubStats += "**Atk** +" + itemInfo.Acf.SubAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubHealFlat))
                                {
                                    itemSubStats += "**Heal** " + itemInfo.Acf.SubHealFlat + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubCritChance))
                                {
                                    itemSubStats += "**Crit Hit Chance** " + itemInfo.Acf.SubCritChance + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubDefFlat))
                                {
                                    itemSubStats += "**Def** " + itemInfo.Acf.SubDefFlat + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubDamageReduction))
                                {
                                    itemSubStats += "**Damage Reduction** " + itemInfo.Acf.SubDamageReduction + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubAtkOnKill))
                                {
                                    itemSubStats += "+" + itemInfo.Acf.SubAtkOnKill + "% Atk increase on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubHpOnKill))
                                {
                                    itemSubStats += "+" + itemInfo.Acf.SubHpOnKill + "% HP recovery on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubShieldOnKill))
                                {
                                    itemSubStats += "+" + itemInfo.Acf.SubShieldOnKill + "% shield increase on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubShieldOnStart))
                                {
                                    itemSubStats += "+" + itemInfo.Acf.SubShieldOnStart + "% shield increase on battle start" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubDef))
                                {
                                    itemSubStats += "**Def** +" + itemInfo.Acf.SubDef + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubHP))
                                {
                                    itemSubStats += "**HP** +" + itemInfo.Acf.SubHP + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubHealPercent))
                                {
                                    itemSubStats += "**Heal** +" + itemInfo.Acf.SubHealPercent + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubSkillDamage))
                                {
                                    itemSubStats += "**Skill Damage** +" + itemInfo.Acf.SubSkillDamage + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubSkillRegenSpeed))
                                {
                                    itemSubStats += "**Weapon Skill Regen Speed** +" + itemInfo.Acf.SubSkillRegenSpeed + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubSkillRegenOnKill))
                                {
                                    itemSubStats += itemInfo.Acf.SubSkillRegenOnKill + " seconds of weapon skill Regen time on enemy kill" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubEarthTypeAtk))
                                {
                                    itemSubStats += "**Earth type Atk** +" + itemInfo.Acf.SubEarthTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubFireTypeAtk))
                                {
                                    itemSubStats += "**Fire type Atk** +" + itemInfo.Acf.SubFireTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubWaterTypeAtk))
                                {
                                    itemSubStats += "**Water type Atk** +" + itemInfo.Acf.SubWaterTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubBasicTypeAtk))
                                {
                                    itemSubStats += "**Basic type Atk** +" + itemInfo.Acf.SubBasicTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubLightTypeAtk))
                                {
                                    itemSubStats += "**Light type Atk** +" + itemInfo.Acf.SubLightTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (!String.IsNullOrEmpty(itemInfo.Acf.SubDarkTypeAtk))
                                {
                                    itemSubStats += "**Dark type Atk** +" + itemInfo.Acf.SubDarkTypeAtk + "%" + System.Environment.NewLine;
                                }
                                if (itemInfo.Acf.SubOptions.Any(s => s.Contains("negated")))
                                {
                                    string[] negatedOptions = itemInfo.Acf.SubOptions.Where(s => s.Contains("negated")).ToArray();
                                    foreach (string negatedOption in negatedOptions)
                                    {
                                        itemSubStats += negatedOption + System.Environment.NewLine;
                                    }
                                }
                            }

                            

                            // Build the page
                            var exclusiveWeaponPage = new PageBuilder()
                            .WithTitle(System.Web.HttpUtility.HtmlDecode(itemInfo.Title.Rendered))
                            .WithDescription(itemStats)
                            .WithUrl(itemInfo.Link)
                            .WithColor(new Color(_emojiList.GetRoleColorCode(itemInfo.Acf.Element)))
                            .WithThumbnailUrl("https://heavenhold.com/wp-content/uploads/" + itemInfo.Embedded.FeaturedMedia.FirstOrDefault().MediaDetails.File);

                            // If the item has an effect at max limit break, display it in a field
                            if (!String.IsNullOrEmpty(lbStat))
                            {
                                exclusiveWeaponPage.AddField("[Required Limit Break 5]", lbStat, false);
                            }

                            // For exclusive weapons, display the exclusive effects
                            if (itemInfo.ItemCategories.FirstOrDefault() == 56)
                            {
                                if (itemInfo.Acf.Exclusive)
                                {
                                    exclusiveWeaponPage.AddField("[" + heroInfo.Title.Rendered.Replace("&#8217;", "'") + " only]", itemInfo.Acf.ExclusiveEffects.Replace("<br />", ""), false);
                                }
                            }

                            // If the item has sub-stats, display them in a field
                            if (!String.IsNullOrEmpty(itemSubStats))
                            {
                                exclusiveWeaponPage.AddField("[Sub-Options] (Max " + itemInfo.Acf.MaxLines + ")", itemSubStats, false);
                            }

                            // Add the page to the list
                            list.Add(exclusiveWeaponPage);

                        }
                        #endregion

                        Paginator pager;

                        if (heroInfo.Acf.BioFields.ExclusiveWeapon != null)
                        {
                            pager = new StaticPaginatorBuilder().WithUsers(Context.User).WithPages(list).WithFooter(PaginatorFooter.Users).WithDefaultEmotes().Build();
                        }
                        else
                        {
                            pager = new StaticPaginatorBuilder().WithUsers(Context.User).WithPages(list).WithFooter(PaginatorFooter.Users).WithoutExclusiveEmote().Build();
                        }

                        await Interactivity.SendPaginatorAsync(pager, Context.Channel, TimeSpan.FromMinutes(2));
                    }
                    else
                    {
                        await ReplyAsync(message);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    await ReplyAsync(message);
                }
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        [Command("item", RunMode = RunMode.Async)]
        public async Task Item(params string[] name)
        {
            string message = "No Item Found.";
            if (!name.Equals("") && name != null)
            {
                try
                {
                    // Search for the closest item
                    ItemInfo itemInfo = Program._itemList.GetItem(name);
                    if (itemInfo != null)
                    {
                        // Build description from item stats
                        string itemStats = "", lbStat = "", itemSubStats = "";

                        // If the item is a costume
                        if (itemInfo.ItemCategories.FirstOrDefault() == 94)
                        {
                            // Check if Super Costume
                            string super = "";
                            if (itemInfo.Acf.Super)
                            {
                                super += "Super ";
                            }
                            itemStats += super + "Costume" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                        }
                        // If the item is an equipment costume
                        else if (itemInfo.ItemCategories.FirstOrDefault() == 125)
                        {
                            itemStats += "Equipment Costume" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                        }
                        // If item has stats
                        else
                        {
                            // Check the if item is a weapon
                            if (itemInfo.ItemCategories.FirstOrDefault() == 56)
                            {
                                itemStats += itemInfo.Acf.Rarity + " " + itemInfo.Acf.WeaponType + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                itemStats += "**" + itemInfo.Acf.DPS + " DPS**" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                                itemStats += "**" + itemInfo.Acf.Element + " Atk** " + itemInfo.Acf.MaxAtk + System.Environment.NewLine;
                            }
                            // If the item is merch
                            else if (itemInfo.ItemCategories.FirstOrDefault() == 124)
                            {
                                itemStats += itemInfo.Acf.Rarity + " Merch" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                            }
                            // If the item is a shield
                            else if (itemInfo.ItemCategories.FirstOrDefault() == 58)
                            {
                                itemStats += itemInfo.Acf.Rarity + " Shield" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                            }
                            // If the item is an accessory
                            else if (itemInfo.ItemCategories.FirstOrDefault() == 57)
                            {
                                itemStats += itemInfo.Acf.Rarity + " Accessory" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                            }
                            // If the item is a card
                            else if (itemInfo.ItemCategories.FirstOrDefault() == 59)
                            {
                                itemStats += "Card" + System.Environment.NewLine + "_ _" + System.Environment.NewLine;
                            }

                            // Check which stats the item has
                            if (!String.IsNullOrEmpty(itemInfo.Acf.Atk))
                            {
                                itemStats += "**Atk** +" + itemInfo.Acf.Atk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.Magazine))
                            {
                                itemStats += "**Magazine Size** " + itemInfo.Acf.Magazine + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.HealFlat))
                            {
                                itemStats += "**Heal** " + itemInfo.Acf.HealFlat + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.CritChance))
                            {
                                itemStats += "**Crit Hit Chance** " + itemInfo.Acf.CritChance + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.DefFlat))
                            {
                                itemStats += "**Def** " + itemInfo.Acf.DefFlat + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.DamageReduction))
                            {
                                itemStats += "**Damage Reduction** " + itemInfo.Acf.DamageReduction + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.AtkOnKill))
                            {
                                itemStats += "+" + itemInfo.Acf.AtkOnKill + "% Atk increase on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.HpOnKill))
                            {
                                itemStats += "+" + itemInfo.Acf.HpOnKill + "% HP recovery on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.ShieldOnKill))
                            {
                                itemStats += "+" + itemInfo.Acf.ShieldOnKill + "% shield increase on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.ShieldOnStart))
                            {
                                itemStats += "+" + itemInfo.Acf.ShieldOnStart + "% shield increase on battle start" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.Def))
                            {
                                itemStats += "**Def** +" + itemInfo.Acf.Def + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.HP))
                            {
                                itemStats += "**HP** +" + itemInfo.Acf.HP + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.HealPercent))
                            {
                                itemStats += "**Heal** +" + itemInfo.Acf.HealPercent + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SkillDamage))
                            {
                                itemStats += "**Skill Damage** +" + itemInfo.Acf.SkillDamage + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SkillRegenSpeed))
                            {
                                itemStats += "**Weapon Skill Regen Speed** +" + itemInfo.Acf.SkillRegenSpeed + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SkillRegenOnKill))
                            {
                                itemStats += itemInfo.Acf.SkillRegenOnKill + " seconds of weapon skill Regen time on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.EarthTypeAtk))
                            {
                                itemStats += "**Earth type Atk** +" + itemInfo.Acf.EarthTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.FireTypeAtk))
                            {
                                itemStats += "**Fire type Atk** +" + itemInfo.Acf.FireTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.WaterTypeAtk))
                            {
                                itemStats += "**Water type Atk** +" + itemInfo.Acf.WaterTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.BasicTypeAtk))
                            {
                                itemStats += "**Basic type Atk** +" + itemInfo.Acf.BasicTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.LightTypeAtk))
                            {
                                itemStats += "**Light type Atk** +" + itemInfo.Acf.LightTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.DarkTypeAtk))
                            {
                                itemStats += "**Dark type Atk** +" + itemInfo.Acf.DarkTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.OnHitDamage))
                            {
                                if (itemInfo.Acf.ExtraDamageType)
                                {
                                    itemStats += "On hit, extra melee damage of " + itemInfo.Acf.OnHitDamage + "% of DPS once every " + itemInfo.Acf.OnHitDamageSeconds + " second(s)." + System.Environment.NewLine;
                                }
                                else
                                {
                                    itemStats += "On hit, extra ranged damage of " + itemInfo.Acf.OnHitDamage + "% of DPS once every " + itemInfo.Acf.OnHitDamageSeconds + " second(s)." + System.Environment.NewLine;
                                }
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.OnHitHealAllies))
                            {
                                itemStats += "On hit, heals all allies by " + itemInfo.Acf.OnHitHealAllies + "% of Heal. Activates once every " + itemInfo.Acf.OnHitHealSeconds + " second(s)." + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.IncreaseDamageAmount))
                            {
                                if (itemInfo.Acf.IncreaseDamageCondition)
                                {
                                    itemStats += "Increase damage by " + itemInfo.Acf.IncreaseDamageAmount + " to enemies with HP more than or equal to " + itemInfo.Acf.IncreaseDamageThreshold + "%." + System.Environment.NewLine;
                                }
                                else
                                {
                                    itemStats += "Increase damage by " + itemInfo.Acf.IncreaseDamageAmount + " to enemies with HP less than or equal to " + itemInfo.Acf.IncreaseDamageThreshold + "%." + System.Environment.NewLine;
                                }
                            }
                            if (itemInfo.Acf.Options.Any(s => s.Contains("negated")))
                            {
                                string[] negatedOptions = itemInfo.Acf.Options.Where(s => s.Contains("negated")).ToArray();
                                foreach (string negatedOption in negatedOptions)
                                {
                                    itemStats += negatedOption + System.Environment.NewLine;
                                }
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.Lb5Option))
                            {
                                string equipment_lb5 = itemInfo.Acf.Lb5Option;

                                if (equipment_lb5 == "Atk (%)")
                                {
                                    lbStat += "**Atk** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                                else if (equipment_lb5 == "HP (%)")
                                {
                                    lbStat += "**HP** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                                else if (equipment_lb5 == "Crit Hit Chance")
                                {
                                    lbStat += "**Crit Hit Chance** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                                else if (equipment_lb5 == "Damage Reduction")
                                {
                                    lbStat += "**Damage Reduction** +" + itemInfo.Acf.Lb5Value;
                                }
                                else if (equipment_lb5 == "Def")
                                {
                                    lbStat += "**Def** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                                else if (equipment_lb5 == "Heal (Flat)")
                                {
                                    lbStat += "**Heal** +" + itemInfo.Acf.Lb5Value;
                                }
                                else if (equipment_lb5 == "Heal (%)")
                                {
                                    lbStat += "**Heal** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                                else if (equipment_lb5 == "Atk increase on enemy kill")
                                {
                                    lbStat += "+" + itemInfo.Acf.Lb5Value + "% Atk increase on enemy kill";
                                }
                                else if (equipment_lb5 == "HP recovery on enemy kill")
                                {
                                    lbStat += "+" + itemInfo.Acf.Lb5Value + "% HP recovery on enemy kill";
                                }
                                else if (equipment_lb5 == "Seconds of weapon skill Regen time on enemy kill")
                                {
                                    lbStat += itemInfo.Acf.Lb5Value + " seconds of weapon skill Regen time on enemy kill";
                                }
                                else if (equipment_lb5 == "Shield increase on battle start")
                                {
                                    lbStat += "+" + itemInfo.Acf.Lb5Value + "% shield increase on battle start";
                                }
                                else if (equipment_lb5 == "Shield increase on enemy kill")
                                {
                                    lbStat += "+" + itemInfo.Acf.Lb5Value + "% shield increease on enemy kill";
                                }
                                else if (equipment_lb5 == "Skill Damage")
                                {
                                    lbStat += "**Skill Damage** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                                else if (equipment_lb5 == "Weapon Skill Regen Speed")
                                {
                                    lbStat += "**Weapon Skill Regen Speed** +" + itemInfo.Acf.Lb5Value + "%";
                                }
                            }

                            // Check which sub stats the item has
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubAtk))
                            {
                                itemSubStats += "**Atk** +" + itemInfo.Acf.SubAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubHealFlat))
                            {
                                itemSubStats += "**Heal** " + itemInfo.Acf.SubHealFlat + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubCritChance))
                            {
                                itemSubStats += "**Crit Hit Chance** " + itemInfo.Acf.SubCritChance + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubDefFlat))
                            {
                                itemSubStats += "**Def** " + itemInfo.Acf.SubDefFlat + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubDamageReduction))
                            {
                                itemSubStats += "**Damage Reduction** " + itemInfo.Acf.SubDamageReduction + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubAtkOnKill))
                            {
                                itemSubStats += "+" + itemInfo.Acf.SubAtkOnKill + "% Atk increase on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubHpOnKill))
                            {
                                itemSubStats += "+" + itemInfo.Acf.SubHpOnKill + "% HP recovery on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubShieldOnKill))
                            {
                                itemSubStats += "+" + itemInfo.Acf.SubShieldOnKill + "% shield increase on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubShieldOnStart))
                            {
                                itemSubStats += "+" + itemInfo.Acf.SubShieldOnStart + "% shield increase on battle start" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubDef))
                            {
                                itemSubStats += "**Def** +" + itemInfo.Acf.SubDef + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubHP))
                            {
                                itemSubStats += "**HP** +" + itemInfo.Acf.SubHP + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubHealPercent))
                            {
                                itemSubStats += "**Heal** +" + itemInfo.Acf.SubHealPercent + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubSkillDamage))
                            {
                                itemSubStats += "**Skill Damage** +" + itemInfo.Acf.SubSkillDamage + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubSkillRegenSpeed))
                            {
                                itemSubStats += "**Weapon Skill Regen Speed** +" + itemInfo.Acf.SubSkillRegenSpeed + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubSkillRegenOnKill))
                            {
                                itemSubStats += itemInfo.Acf.SubSkillRegenOnKill + " seconds of weapon skill Regen time on enemy kill" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubEarthTypeAtk))
                            {
                                itemSubStats += "**Earth type Atk** +" + itemInfo.Acf.SubEarthTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubFireTypeAtk))
                            {
                                itemSubStats += "**Fire type Atk** +" + itemInfo.Acf.SubFireTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubWaterTypeAtk))
                            {
                                itemSubStats += "**Water type Atk** +" + itemInfo.Acf.SubWaterTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubBasicTypeAtk))
                            {
                                itemSubStats += "**Basic type Atk** +" + itemInfo.Acf.SubBasicTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubLightTypeAtk))
                            {
                                itemSubStats += "**Light type Atk** +" + itemInfo.Acf.SubLightTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (!String.IsNullOrEmpty(itemInfo.Acf.SubDarkTypeAtk))
                            {
                                itemSubStats += "**Dark type Atk** +" + itemInfo.Acf.SubDarkTypeAtk + "%" + System.Environment.NewLine;
                            }
                            if (itemInfo.Acf.SubOptions.Any(s => s.Contains("negated")))
                            {
                                string[] negatedOptions = itemInfo.Acf.SubOptions.Where(s => s.Contains("negated")).ToArray();
                                foreach (string negatedOption in negatedOptions)
                                {
                                    itemSubStats += negatedOption + System.Environment.NewLine;
                                }
                            }
                        }



                        // Build the embed
                        var itemEmbed = new EmbedBuilder()
                        .WithTitle(System.Web.HttpUtility.HtmlDecode(itemInfo.Title.Rendered))
                        .WithDescription(itemStats)
                        .WithUrl(itemInfo.Link)                                                
                        .WithFooter(footer =>
                        {
                            footer
                            .WithText("Powered by Heavenhold.com")
                            .WithIconUrl(System.Configuration.ConfigurationManager.AppSettings["copyRightIconUrl"]);
                        });

                        // If the item has an additional stat when max limit broken, add a field
                        if (!String.IsNullOrEmpty(lbStat))
                        {
                            itemEmbed.AddField("[Required Limit Break 5]", lbStat, false);
                        }

                        // If the item is a weapon, check if it's an exclusive to add the exclusive effects information
                        if (itemInfo.ItemCategories.FirstOrDefault() == 56)
                        {
                            if (itemInfo.Acf.Exclusive)
                            {
                                itemEmbed.AddField("[" + itemInfo.Acf.Hero.FirstOrDefault().PostTitle.Replace("&#8217;", "'") + " only]", itemInfo.Acf.ExclusiveEffects.Replace("<br />", ""), false);
                            }
                        }

                        // Use a larger image if the item is a costume or equipment costume, otherwise use a thumbnail
                        if (itemInfo.ItemCategories.FirstOrDefault() == 94 || itemInfo.ItemCategories.FirstOrDefault() == 125)
                        {
                            itemEmbed.WithImageUrl("https://heavenhold.com/wp-content/uploads/" + itemInfo.Embedded.FeaturedMedia.FirstOrDefault().MediaDetails.File);
                        }
                        else
                        {
                            itemEmbed.WithThumbnailUrl("https://heavenhold.com/wp-content/uploads/" + itemInfo.Embedded.FeaturedMedia.FirstOrDefault().MediaDetails.File);
                        }

                        // If the item has sub-options, display them in a field field
                        if (!String.IsNullOrEmpty(itemSubStats))
                        {
                            itemEmbed.AddField("[Sub-Options] (Max " + itemInfo.Acf.MaxLines + ")", itemSubStats, false);
                        }

                        // If the item has an element, give it a colour
                        if(!String.IsNullOrEmpty(itemInfo.Acf.Element)) {
                            itemEmbed.WithColor(new Color(_emojiList.GetRoleColorCode(itemInfo.Acf.Element)));
                        }

                        // Send the embed
                        Embed embed = itemEmbed.Build();
                        await ReplyAsync(null, false, embed);
                    }
                    else
                    {
                        await ReplyAsync(message);
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    await ReplyAsync(message);
                }
            }
            else
            {
                await ReplyAsync(message);
            }
        }

        [Command("help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            try
            {
                Embed HelpEmd = null;
                var builder = new EmbedBuilder()
               .WithTitle("Commands")
               .WithColor(Discord.Color.Teal)
               .WithDescription("I can help look up items and heroes on heavenhold.com!")
               .WithFooter(footer =>
               {
                   footer
                   .WithText("Powered by Heavenhold.com")
                   .WithIconUrl(System.Configuration.ConfigurationManager.AppSettings["copyRightIconUrl"]);
               })
               .AddField("!hero [part of a hero's name]", "\"!hero Lupina\" or \"!hero ice witch\" to get Ice Witch Lupina. Use the reactions to view different pages.")
               .AddField("!item [part of an item's name]", "\"!item training staff\"")
               .AddField("!bg", "Quickly get a link to the heavenhold.com beginner guide")
               .AddField("!tl", "Quickly get a link to the heavenhold.com tier list")
               .AddField("!teams", "Quickly get a link to the heavenhold.com suggested teams");
                HelpEmd = builder.Build();
                await ReplyAsync(null, false, HelpEmd);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        [Command("bg", RunMode = RunMode.Async)]
        [Alias("beginner")]
        public async Task BeginnerGuide()
        {
            try
            {
                await ReplyAsync("https://heavenhold.com/guides/beginner-guide");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        [Command("tl", RunMode = RunMode.Async)]
        [Alias("tierlist")]
        public async Task TierList()
        {
            try
            {
                await ReplyAsync("https://heavenhold.com/tier-list");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        [Command("teams", RunMode = RunMode.Async)]
        [Alias("suggestedteams")]
        public async Task Teams()
        {
            try
            {
                await ReplyAsync("https://heavenhold.com/guides/suggested-teams");
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        [Command("refresh")]
        public Task Refresh()
        {
            try
            {
                Program._heroList.RefreshData();
                Program._itemList.RefreshData();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return Task.CompletedTask;
        }
    }
}
