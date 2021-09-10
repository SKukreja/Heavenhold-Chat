using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HeavenholdBot.Models
{
    public partial class ItemInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("date_gmt")]
        public DateTimeOffset DateGmt { get; set; }

        [JsonProperty("guid")]
        public GuidClass Guid { get; set; }

        [JsonProperty("modified")]
        public DateTimeOffset Modified { get; set; }

        [JsonProperty("modified_gmt")]
        public DateTimeOffset ModifiedGmt { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("title")]
        public GuidClass Title { get; set; }

        [JsonProperty("excerpt")]
        public Excerpt Excerpt { get; set; }

        [JsonProperty("featured_media")]
        public string FeaturedMedia { get; set; }

        [JsonProperty("comment_status")]
        public string CommentStatus { get; set; }

        [JsonProperty("ping_status")]
        public string PingStatus { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("item_categories")]
        public int[] ItemCategories { get; set; }

        [JsonProperty("acf")]
        public Acf Acf { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("_embedded")]
        public Embeds Embedded { get; set; }
    }

    public partial class Embeds
    {
        [JsonProperty("wp:featuredmedia")]
        public Media[] FeaturedMedia { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("media_details")]
        public MediaDetails MediaDetails { get; set; }
    }

    public partial class MediaDetails
    {
        [JsonProperty("file")]
        public string File { get; set; }
    }

    public partial class Acf
    {
        [JsonProperty("item_type")]
        public long ItemType { get; set; }

        [JsonProperty("max_level")]
        public string MaxLevel { get; set; }

        [JsonProperty("rarity")]
        public string Rarity { get; set; }

        [JsonProperty("weapon_type")]
        public string WeaponType { get; set; }

        [JsonProperty("options")]
        public string[] Options { get; set; }

        [JsonProperty("sub_options")]
        public string[] SubOptions { get; set; }

        [JsonProperty("hero")]
        public Hero[] Hero { get; set; }

        [JsonProperty("exclusive")]
        public Boolean Exclusive { get; set; }

        [JsonProperty("exclusive_effects")]
        public string ExclusiveEffects { get; set; }

        [JsonProperty("element")]
        public string Element { get; set; }

        [JsonProperty("max_atk")]
        public string MaxAtk { get; set; }

        [JsonProperty("magazine")]
        public string Magazine { get; set; }

        [JsonProperty("dps")]
        public string DPS { get; set; }

        [JsonProperty("weapon_skill")]
        public Boolean WeaponSkill { get; set; }

        [JsonProperty("weapon_skill_name")]
        public string WeaponSkillName { get; set; }

        [JsonProperty("weapon_skill_atk")]
        public string WeaponSkillAtk { get; set; }

        [JsonProperty("weapon_skill_regen_time")]
        public string WeaponSkillRegenTime { get; set; }

        [JsonProperty("weapon_skill_description")]
        public string WeaponSkillDescription { get; set; }

        [JsonProperty("weapon_skill_chain")]
        public string WeaponSkillChain { get; set; }

        [JsonProperty("atk")]
        public string Atk { get; set; }

        [JsonProperty("crit_chance")]
        public string CritChance { get; set; }

        [JsonProperty("damage_reduction")]
        public string DamageReduction { get; set; }

        [JsonProperty("def")]
        public string Def { get; set; }

        [JsonProperty("def_flat")]
        public string DefFlat { get; set; }

        [JsonProperty("heal_percent")]
        public string HealPercent { get; set; }

        [JsonProperty("heal_flat")]
        public string HealFlat { get; set; }

        [JsonProperty("hp")]
        public string HP { get; set; }

        [JsonProperty("atk_on_kill")]
        public string AtkOnKill { get; set; }

        [JsonProperty("hp_on_kill")]
        public string HpOnKill { get; set; }

        [JsonProperty("skill_regen_on_kill")]
        public string SkillRegenOnKill { get; set; }

        [JsonProperty("shield_on_start")]
        public string ShieldOnStart { get; set; }

        [JsonProperty("shield_on_kill")]
        public string ShieldOnKill { get; set; }

        [JsonProperty("skill_damage")]
        public string SkillDamage { get; set; }

        [JsonProperty("skill_regen_speed")]
        public string SkillRegenSpeed { get; set; }

        [JsonProperty("earth_type_atk")]
        public string EarthTypeAtk { get; set; }

        [JsonProperty("fire_type_atk")]
        public string FireTypeAtk { get; set; }

        [JsonProperty("water_type_atk")]
        public string WaterTypeAtk { get; set; }

        [JsonProperty("dark_type_atk")]
        public string DarkTypeAtk { get; set; }

        [JsonProperty("light_type_atk")]
        public string LightTypeAtk { get; set; }

        [JsonProperty("basic_type_atk")]
        public string BasicTypeAtk { get; set; }

        [JsonProperty("crit_hit_multiplier")]
        public string CritMultiplier { get; set; }

        [JsonProperty("on_hit_damage")]
        public string OnHitDamage { get; set; }

        [JsonProperty("on_hit_damage_seconds")]
        public string OnHitDamageSeconds { get; set; }

        [JsonProperty("extra_damage_type")]
        public Boolean ExtraDamageType { get; set; }

        [JsonProperty("on_hit_heal_allies")]
        public string OnHitHealAllies { get; set; }

        [JsonProperty("on_hit_heal_seconds")]
        public string OnHitHealSeconds { get; set; }

        [JsonProperty("increase_damage_amount")]
        public string IncreaseDamageAmount { get; set; }

        [JsonProperty("increase_damage_threshold")]
        public string IncreaseDamageThreshold { get; set; }

        [JsonProperty("increase_damage_condition")]
        public Boolean IncreaseDamageCondition { get; set; }

        [JsonProperty("decrease_damage_taken_by_skill")]
        public string DecreaseDamageTakenBySkill { get; set; }

        [JsonProperty("increase_damage_to_tanks")]
        public string IncreaseDamageToTanks { get; set; }

        [JsonProperty("lb5_option")]
        public string Lb5Option { get; set; }

        [JsonProperty("lb5_value")]
        public string Lb5Value { get; set; }

        [JsonProperty("sub_atk")]
        public string SubAtk { get; set; }

        [JsonProperty("sub_crit_chance")]
        public string SubCritChance { get; set; }

        [JsonProperty("sub_damage_reduction")]
        public string SubDamageReduction { get; set; }

        [JsonProperty("sub_def")]
        public string SubDef { get; set; }

        [JsonProperty("sub_def_flat")]
        public string SubDefFlat { get; set; }

        [JsonProperty("sub_heal_percent")]
        public string SubHealPercent { get; set; }

        [JsonProperty("sub_heal_flat")]
        public string SubHealFlat { get; set; }

        [JsonProperty("sub_hp")]
        public string SubHP { get; set; }

        [JsonProperty("sub_atk_on_kill")]
        public string SubAtkOnKill { get; set; }

        [JsonProperty("sub_hp_on_kill")]
        public string SubHpOnKill { get; set; }

        [JsonProperty("sub_skill_regen_on_kill")]
        public string SubSkillRegenOnKill { get; set; }

        [JsonProperty("sub_shield_on_start")]
        public string SubShieldOnStart { get; set; }

        [JsonProperty("sub_shield_on_kill")]
        public string SubShieldOnKill { get; set; }

        [JsonProperty("sub_skill_damage")]
        public string SubSkillDamage { get; set; }

        [JsonProperty("sub_skill_regen_speed")]
        public string SubSkillRegenSpeed { get; set; }

        [JsonProperty("sub_earth_type_atk")]
        public string SubEarthTypeAtk { get; set; }

        [JsonProperty("sub_fire_type_atk")]
        public string SubFireTypeAtk { get; set; }

        [JsonProperty("sub_water_type_atk")]
        public string SubWaterTypeAtk { get; set; }

        [JsonProperty("sub_dark_type_atk")]
        public string SubDarkTypeAtk { get; set; }

        [JsonProperty("sub_light_type_atk")]
        public string SubLightTypeAtk { get; set; }

        [JsonProperty("sub_basic_type_atk")]
        public string SubBasicTypeAtk { get; set; }

        [JsonProperty("max_lines")]
        public string MaxLines { get; set; }

        [JsonProperty("super")]
        public Boolean Super { get; set; }
    }

    public partial class Hero
    {
        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("post_title")]
        public string PostTitle { get; set; }
    }

}
