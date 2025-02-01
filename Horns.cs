using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	// Token: 0x02000F83 RID: 3971
	[Serializable]
	public class Horns : BaseMutation
	{
		// Token: 0x060099AC RID: 39340 RVA: 0x003B4354 File Offset: 0x003B2554
		public Horns()
		{
			this.DisplayName = "Horns";
		}

		// Token: 0x060099AD RID: 39341 RVA: 0x003B43B4 File Offset: 0x003B25B4
		public override bool GeneratesEquipment()
		{
			return true;
		}

		// Token: 0x060099AE RID: 39342 RVA: 0x003B43B7 File Offset: 0x003B25B7
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == RegenerateDefaultEquipmentEvent.ID;
		}

		// Token: 0x060099AF RID: 39343 RVA: 0x003B43CD File Offset: 0x003B25CD
		public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
		{
			this.RegrowHorns();
			return base.HandleEvent(E);
		}

		// Token: 0x060099B0 RID: 39344 RVA: 0x003B43E0 File Offset: 0x003B25E0
		public override string GetDescription()
        {
            if (this.Variant == null)
            {
                if (HeadsAmount == 1)
                {
                    return "Horns jut out of your head.";
                }
                else
                {
                    return "Horns jut out of your heads.";
                }
            }
            GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(this.Variant);
            string propertyOrTag = blueprint.GetPropertyOrTag("Gender");
            string cachedDisplayNameStripped = blueprint.CachedDisplayNameStripped;
            if (HeadsAmount == 1)
            {
                if (propertyOrTag == "plural")
                {
                    return Grammar.InitCap(cachedDisplayNameStripped) + " jut out of your head.";
                }
                return Grammar.A(cachedDisplayNameStripped, true) + " juts out of your head.";
            }
            if (propertyOrTag == "plural")
            {
                return Grammar.InitCap(cachedDisplayNameStripped) + " jut out of your heads.";
            }
            return Grammar.A(cachedDisplayNameStripped, true) + " juts out of your heads.";
        }

		// Token: 0x060099B1 RID: 39345 RVA: 0x003B444A File Offset: 0x003B264A
		public int GetAV(int Level)
		{
			return 1 + (Level - 1) / 3;
		}

		// Token: 0x060099B2 RID: 39346 RVA: 0x003B4454 File Offset: 0x003B2654
		public string GetBaseDamage(int Level)
		{
			return "2d" + (3 + Level / 2).ToString();
		}

		// Token: 0x060099B3 RID: 39347 RVA: 0x003B4478 File Offset: 0x003B2678
		public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
		{
			Horns horns = base.DeepCopy(Parent, MapInv) as Horns;
			return horns;
		}

		// Token: 0x060099B4 RID: 39348 RVA: 0x003B4490 File Offset: 0x003B2690
		public override string GetLevelText(int Level)
		{
			string baseDamage = this.GetBaseDamage(Level);
			int av = this.GetAV(Level);
			string str = "20% chance on melee attack to gore your opponent\n";
			str = str + "Damage increment: {{rules|" + baseDamage + "}}\n";
			if (Level == base.Level)
			{
				str += "Goring attacks may cause bleeding\n";
			}
			else
			{
				str += "{{rules|Increased bleeding save difficulty and intensity}}\n";
			}
            string a = "plural";
            string word;
			if (this.Variant == null)
			{
				word = "horns";
			}
			else
			{
                GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(this.Variant);
                a = blueprint.GetPropertyOrTag("Gender");
                word = blueprint.CachedDisplayNameStripped;
            }
            if (a == "plural")
            {
                str = str + Grammar.InitCap(word) + " are a short-blade class natural weapon.\n";
            }
            else
            {
                str = str + Grammar.InitCap(word) + " is a short-blade class natural weapon.\n";
            }
            str = str + "+{{rules|" + av.ToString() + " AV}}\n";
            str += "Cannot wear helmets\n";
            return str + "+100 reputation with {{w|antelopes}} and {{w|goatfolk}}";
		}

        // Token: 0x0600969B RID: 38555 RVA: 0x003A308C File Offset: 0x003A128C
        private void RegrowHorns()
        {
            Body body = this.ParentObject.Body;
            if (body == null || this.Variant.IsNullOrEmpty())
            {
                return;
            }
            HeadsAmount = 0;
            string partParameter = GameObjectFactory.Factory.GetBlueprint(this.Variant).GetPartParameter<string>("MeleeWeapon", "Slot", "Head");
            foreach (BodyPart current in body.GetPart(partParameter))
            {
                if (!Mutating)
                {
                    GameObject currentHorn = current.Equipped;
                    if (currentHorn != null)
                    {
                        current.ForceUnequip(true);
                        currentHorn.Destroy();
                    }
                }

                GameObject currentHornsObject = GameObject.Create(this.Variant);
                MeleeWeapon meleeWeapon = currentHornsObject.GetPart("MeleeWeapon") as MeleeWeapon;
                Armor armor = currentHornsObject.GetPart("Armor") as Armor;
                if (string.IsNullOrEmpty(this.HornsName))
                {
                    this.HornsName = this.DisplayName.ToLower();
                }
                currentHornsObject.pRender.DisplayName = this.HornsName;
                meleeWeapon.MaxStrengthBonus = 100;
                armor.WornOn = current.Type;
                meleeWeapon.BaseDamage = this.GetBaseDamage(base.Level);
                armor.AV = this.GetAV(base.Level);
                this.ParentObject.ForceEquipObject(currentHornsObject, current, true, new int?(0));
                this.HornsObject = currentHornsObject;
                HeadsAmount++;
            }

            Mutating = false;
        }

		// Token: 0x060099B6 RID: 39350 RVA: 0x003B46A3 File Offset: 0x003B28A3
		public override bool ChangeLevel(int NewLevel)
		{
			this.RegrowHorns();
			return base.ChangeLevel(NewLevel);
		}

		// Token: 0x060099B8 RID: 39352 RVA: 0x003B46BC File Offset: 0x003B28BC
		public override void SetVariant(string Variant)
		{
            base.SetVariant(Variant);
            if (this.HornsObject != null && this.HornsObject.Blueprint != Variant)
            {
                this.RegrowHorns();
            }
		}

		// Token: 0x060099B9 RID: 39353 RVA: 0x003B4714 File Offset: 0x003B2914
		public override bool Mutate(GameObject GO, int Level)
		{
            Mutating = true;
            if (this.Variant.IsNullOrEmpty())
            {
                this.Variant = this.GetVariants().GetRandomElement(null);
                this.DisplayName = (this.GetVariantName() ?? "Horns");
            }
            return base.Mutate(GO, Level);
		}

		// Token: 0x060099BA RID: 39354 RVA: 0x003B47FA File Offset: 0x003B29FA
		public override bool Unmutate(GameObject GO)
		{
			foreach (BodyPart Head in GO.Body.GetPart("Head"))
			{
                GameObject hornToDestroy = Head.Equipped;
                base.CleanUpMutationEquipment(GO, ref hornToDestroy);
			}
			return base.Unmutate(GO);
		}

		// Token: 0x04003E13 RID: 15891
		public bool Mutating = false;

		// Token: 0x04003E14 RID: 15892
		public string HornsName;
		
		public GameObject HornsObject;
        
        public int HeadsAmount = 1;

		// Token: 0x04003E15 RID: 15893
		[NonSerialized]
		private List<string> variants = new List<string>
		{
			"horns",
			"horn",
			"antlers",
			"casque"
		};
	}
}
