using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	// Token: 0x020010CA RID: 4298
	[Serializable]
	public class BurrowingClaws : BaseDefaultEquipmentMutation
	{
		// Token: 0x17000A45 RID: 2629
		// (get) Token: 0x0600A805 RID: 43013 RVA: 0x003F4770 File Offset: 0x003F2970
		public GameObjectBlueprint Blueprint
		{
			get
			{
				if (this._Blueprint == null)
				{
					this._Blueprint = GameObjectFactory.Factory.GetBlueprint(this.GetBlueprintName());
				}
				return this._Blueprint;
			}
		}

		// Token: 0x0600A806 RID: 43014 RVA: 0x003F4798 File Offset: 0x003F2998
		public BurrowingClaws()
		{
			this.DisplayName = "Burrowing Claws";
		}

		// Token: 0x0600A807 RID: 43015 RVA: 0x003F47E9 File Offset: 0x003F29E9
		public override bool GeneratesEquipment()
		{
			return true;
		}

		// Token: 0x0600A808 RID: 43016 RVA: 0x003F47EC File Offset: 0x003F29EC
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == AfterGameLoadedEvent.ID || ID == PartSupportEvent.ID || ID == PathAsBurrowerEvent.ID || ID == PreferDefaultBehaviorEvent.ID;
		}

		// Token: 0x0600A809 RID: 43017 RVA: 0x003F481A File Offset: 0x003F2A1A
		public override bool HandleEvent(PathAsBurrowerEvent E)
		{
			return (!this.PathAsBurrower || !base.IsMyActivatedAbilityToggledOn(this.EnableActivatedAbilityID, null)) && base.HandleEvent(E);
		}

		// Token: 0x0600A80A RID: 43018 RVA: 0x003F483C File Offset: 0x003F2A3C
		public override bool HandleEvent(PartSupportEvent E)
		{
			return (E.Skip == this || !(E.Type == "Digging") || !base.IsMyActivatedAbilityToggledOn(this.EnableActivatedAbilityID, null)) && base.HandleEvent(E);
		}

		// Token: 0x0600A80B RID: 43019 RVA: 0x003F4871 File Offset: 0x003F2A71
		public override bool HandleEvent(AfterGameLoadedEvent E)
		{
			NeedPartSupportEvent.Send(this.ParentObject, "Digging", null);
			return base.HandleEvent(E);
		}

		// Token: 0x0600A80C RID: 43020 RVA: 0x003F488B File Offset: 0x003F2A8B
		public override void Register(GameObject Object)
		{
			Object.RegisterPartEvent(this, "CommandDigDown");
			Object.RegisterPartEvent(this, "CommandDigUp");
			Object.RegisterPartEvent(this, "CommandToggleBurrowingClaws");
			base.Register(Object);
		}

		// Token: 0x0600A80D RID: 43021 RVA: 0x003F48B8 File Offset: 0x003F2AB8
		public string GetBlueprintName()
		{
			return this.Variant.Coalesce(BurrowingClaws.OBJECT_BLUEPRINT_NAME);
		}

		// Token: 0x0600A80E RID: 43022 RVA: 0x003F48CA File Offset: 0x003F2ACA
		public bool CheckDig()
		{
			if (this.ParentObject.AreHostilesNearby())
			{
				Popup.ShowFail("You can't excavate with hostiles nearby.", true, true, true);
				return false;
			}
			return true;
		}

		// Token: 0x0600A80F RID: 43023 RVA: 0x003F48EC File Offset: 0x003F2AEC
		public override bool FireEvent(Event E)
		{
			if (E.ID == "CommandToggleBurrowingClaws")
			{
				base.ToggleMyActivatedAbility(this.EnableActivatedAbilityID, null, false, null);
				if (base.IsMyActivatedAbilityToggledOn(this.EnableActivatedAbilityID, null))
				{
					this.ParentObject.RequirePart<Digging>(false);
				}
				else
				{
					NeedPartSupportEvent.Send(this.ParentObject, "Digging", null);
				}
				this.ParentObject.RemoveStringProperty("Burrowing");
			}
			else if (E.ID == "CommandDigDown")
			{
				if (this.CheckDig())
				{
					Cell cellFromDirection = this.ParentObject.CurrentCell.GetCellFromDirection("D", false);
					this.ParentObject.CurrentCell.AddObject("StairsDown", null, null, null, null);
					cellFromDirection.AddObject("StairsUp", null, null, null, null);
					this.ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_burrowingClaws_burrow", 0.5f, 0f, false, 0f, 1f, 1f, int.MaxValue);
				}
			}
			else if (E.ID == "CommandDigUp")
			{
				if (this.ParentObject.CurrentZone.IsOutside())
				{
					Popup.ShowFail("You can't excavate the sky!", true, true, true);
					return true;
				}
				if (this.CheckDig())
				{
					Cell cellFromDirection2 = this.ParentObject.CurrentCell.GetCellFromDirection("U", false);
					this.ParentObject.CurrentCell.AddObject("StairsUp", null, null, null, null);
					cellFromDirection2.AddObject("StairsDown", null, null, null, null);
					GameObject parentObject = this.ParentObject;
					if (parentObject != null)
					{
						parentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_burrowingClaws_burrow", 0.5f, 0f, false, 0f, 1f, 1f, int.MaxValue);
					}
				}
			}
			return base.FireEvent(E);
		}

		// Token: 0x0600A810 RID: 43024 RVA: 0x003F4AB0 File Offset: 0x003F2CB0
		public string GetPenetration()
		{
			return BurrowingClaws.GetPenetration(base.Level);
		}

		// Token: 0x0600A811 RID: 43025 RVA: 0x003F4AC0 File Offset: 0x003F2CC0
		public static string GetPenetration(int Level)
		{
			return "1d6+" + (3 * Level).ToString();
		}

		// Token: 0x0600A812 RID: 43026 RVA: 0x003F4AE2 File Offset: 0x003F2CE2
		public int GetWallBonusPenetration()
		{
			return BurrowingClaws.GetWallBonusPenetration(base.Level);
		}

		// Token: 0x0600A813 RID: 43027 RVA: 0x003F4AEF File Offset: 0x003F2CEF
		public static int GetWallBonusPenetration(int Level)
		{
			return Level * 3;
		}

		// Token: 0x0600A814 RID: 43028 RVA: 0x003F4AF4 File Offset: 0x003F2CF4
		public double GetWallBonusPercentage()
		{
			return BurrowingClaws.GetWallBonusPercentage(base.Level, this.ParentObject);
		}

		// Token: 0x0600A815 RID: 43029 RVA: 0x003F4B08 File Offset: 0x003F2D08
		public static double GetWallBonusPercentage(int Level, GameObject Mutant = null)
		{
			int num = 25;
			if (Mutant != null && Mutant.IsGiganticCreature)
			{
				num *= 2;
			}
			return (double)num;
		}

		// Token: 0x0600A816 RID: 43030 RVA: 0x003F4B29 File Offset: 0x003F2D29
		public int GetWallHitsRequired()
		{
			return BurrowingClaws.GetWallHitsRequired(base.Level, this.ParentObject);
		}

		// Token: 0x0600A817 RID: 43031 RVA: 0x003F4B3C File Offset: 0x003F2D3C
		public static int GetWallHitsRequired(int Level, GameObject Mutant = null)
		{
			return Drill.GetWallHitsRequired(BurrowingClaws.GetWallBonusPercentage(Level, Mutant));
		}

		// Token: 0x0600A818 RID: 43032 RVA: 0x003F4B4A File Offset: 0x003F2D4A
		public int GetAV(int Level)
		{
			if (Level < 5)
			{
				return 1;
			}
			if (Level < 9)
			{
				return 2;
			}
			return 3;
		}

		// Token: 0x0600A819 RID: 43033 RVA: 0x003F4B5A File Offset: 0x003F2D5A
		public override string GetDescription()
		{
			return this.Blueprint.GetTag("VariantDescription", "").Coalesce("You bear spade-like claws that can burrow through the earth.");
		}

		// Token: 0x0600A81A RID: 43034 RVA: 0x003F4B7C File Offset: 0x003F2D7C
		public override string GetLevelText(int Level)
		{
			string cachedDisplayNameStrippedTitleCase = this.Blueprint.CachedDisplayNameStrippedTitleCase;
			string value = Grammar.Pluralize(cachedDisplayNameStrippedTitleCase);
			int wallBonusPenetration = BurrowingClaws.GetWallBonusPenetration(Level);
			StringBuilder stringBuilder = Event.NewStringBuilder("").Append(cachedDisplayNameStrippedTitleCase).Append(" penetration vs. walls: {{rules|").Append(wallBonusPenetration.Signed(false)).Append("}}\n");
			int wallHitsRequired = BurrowingClaws.GetWallHitsRequired(Level, this.ParentObject);
			if (wallHitsRequired > 0)
			{
				stringBuilder.Append(value).Append(" destroy walls after ").Append(wallHitsRequired).Append(" penetrating hits.\n");
			}
			if (Options.EnablePrereleaseContent)
			{
				stringBuilder.Append("Can dig passages up or down when outside of combat\n");
			}
			stringBuilder.Append(value).Append(" are also a ").Append(this.GetWeaponClass()).Append(" class natural weapon that deal {{rules|").Append(this.GetClawsDamage(Level)).Append("}} base damage to non-walls.");
			return Event.FinalizeString(stringBuilder);
		}

		// Token: 0x0600A81B RID: 43035 RVA: 0x003F4C5C File Offset: 0x003F2E5C
		public string GetWeaponClass()
		{
			string partParameter = this.Blueprint.GetPartParameter<string>("MeleeWeapon", "Skill", "ShortBlades");
			string result;
			if (Skills.WeaponClassName.TryGetValue(partParameter, out result))
			{
				return result;
			}
			return "short-blade";
		}

		// Token: 0x0600A81C RID: 43036 RVA: 0x003F4C9C File Offset: 0x003F2E9C
		public string GetClawsDamage(int Level)
		{
			ReadOnlySpan<char> value = default(ReadOnlySpan<char>);
			foreach (ReadOnlySpan<char> span in this.Blueprint.GetTag("VariantDamage", "").DelimitedBy(','))
			{
				int num = span.IndexOf(':');
				int num2;
				if (int.TryParse(span.Slice(0, num), out num2) && num2 <= Level)
				{
					value = span.Slice(num + 1);
				}
			}
			if (value.Length == 0)
			{
				return "1d2";
			}
			return new string(value);
		}

		// Token: 0x0600A81D RID: 43037 RVA: 0x003F4D2C File Offset: 0x003F2F2C
		public override void OnRegenerateDefaultEquipment(Body body)
		{
			GameObjectBlueprint blueprint = this.Blueprint;
			string partParameter = blueprint.GetPartParameter<string>("MeleeWeapon", "Slot", "Hand");
			List<BodyPart> part = body.GetPart(partParameter);
			int level = base.Level;
			int num = 0;
			while (num < part.Count /*&& num < 2*/)
			{
				BodyPart bodyPart = part[num];
				if (bodyPart.DefaultBehavior == null || bodyPart.DefaultBehavior.GetBlueprint(true) != blueprint)
				{
					bodyPart.DefaultBehavior = GameObject.Create(blueprint, 0, 0, null, null, null, null, null);
				}
				MeleeWeapon meleeWeapon;
				if (bodyPart.DefaultBehavior.TryGetPart<MeleeWeapon>(out meleeWeapon))
				{
					meleeWeapon.BaseDamage = this.GetClawsDamage(level);
				}
				BurrowingClawsProperties burrowingClawsProperties;
				if (bodyPart.DefaultBehavior.TryGetPart<BurrowingClawsProperties>(out burrowingClawsProperties))
				{
					burrowingClawsProperties.WallBonusPenetration = BurrowingClaws.GetWallBonusPenetration(level);
					burrowingClawsProperties.WallBonusPercentage = BurrowingClaws.GetWallBonusPercentage(level, null);
				}
				num++;
			}
		}

		// Token: 0x0600A81E RID: 43038 RVA: 0x003F4E05 File Offset: 0x003F3005
		public override void SetVariant(string Variant)
		{
			base.SetVariant(Variant);
			this._Blueprint = null;
		}

		// Token: 0x0600A81F RID: 43039 RVA: 0x003F4E15 File Offset: 0x003F3015
		public override bool ChangeLevel(int NewLevel)
		{
			return base.ChangeLevel(NewLevel);
		}

		// Token: 0x0600A820 RID: 43040 RVA: 0x003F4E20 File Offset: 0x003F3020
		public override bool Mutate(GameObject GO, int Level)
		{
			if (Options.EnablePrereleaseContent)
			{
                this.DigUpActivatedAbilityID = base.AddMyActivatedAbility("Excavate up", "CommandDigUp", "Physical Mutation", null, "\u0018", null, false, false, false, false, false, false, false, false, true, true, false, -1, null, null, null, null, null, null);
                this.DigDownActivatedAbilityID = base.AddMyActivatedAbility("Excavate down", "CommandDigDown", "Physical Mutation", null, "\u0019", null, false, false, false, false, false, false, false, false, true, true, false, -1, null, null, null, null, null, null);
			}
			this.EnableActivatedAbilityID = base.AddMyActivatedAbility(this.DisplayName, "CommandToggleBurrowingClaws", "Physical Mutation", null, "ë", null, true, true, true, false, false, true, false, false, true, true, false, -1, null, null, null, null, null, null);
			if (base.IsMyActivatedAbilityToggledOn(this.EnableActivatedAbilityID, null))
			{
				GO.RequirePart<Digging>(false);
			}
			return base.Mutate(GO, Level);
		}

		// Token: 0x0600A821 RID: 43041 RVA: 0x003F4EED File Offset: 0x003F30ED
		public override bool Unmutate(GameObject GO)
		{
			NeedPartSupportEvent.Send(GO, "Digging", this);
			base.RemoveMyActivatedAbility(ref this.DigUpActivatedAbilityID, null);
			base.RemoveMyActivatedAbility(ref this.DigDownActivatedAbilityID, null);
			base.RemoveMyActivatedAbility(ref this.EnableActivatedAbilityID, null);
			return base.Unmutate(GO);
		}

		// Token: 0x040043E5 RID: 17381
		public static readonly string OBJECT_BLUEPRINT_NAME = "Burrowing Claws Claw";

		// Token: 0x040043E6 RID: 17382
		public string BodyPartType = "Hands";

		// Token: 0x040043E7 RID: 17383
		public Guid DigUpActivatedAbilityID = Guid.Empty;

		// Token: 0x040043E8 RID: 17384
		public Guid DigDownActivatedAbilityID = Guid.Empty;

		// Token: 0x040043E9 RID: 17385
		public Guid EnableActivatedAbilityID = Guid.Empty;

		// Token: 0x040043EA RID: 17386
		public bool PathAsBurrower = true;

		// Token: 0x040043EB RID: 17387
		[NonSerialized]
		protected GameObjectBlueprint _Blueprint;
	}
}
