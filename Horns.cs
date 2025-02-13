using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class Horns : BaseMutation
	{
		public Horns()
		{
			this.DisplayName = "Horns";
		}

		public override bool GeneratesEquipment()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == RegenerateDefaultEquipmentEvent.ID;
		}

		public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
		{
			this.RegrowHorns();
			return base.HandleEvent(E);
		}

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

		public int GetAV(int Level)
		{
			return 1 + (Level - 1) / 3;
		}

		public string GetBaseDamage(int Level)
		{
			return "2d" + (3 + Level / 2).ToString();
		}

		public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
		{
			Horns horns = base.DeepCopy(Parent, MapInv) as Horns;
			return horns;
		}

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

		public override bool ChangeLevel(int NewLevel)
		{
			this.RegrowHorns();
			return base.ChangeLevel(NewLevel);
		}

		public override void SetVariant(string Variant)
		{
            base.SetVariant(Variant);
            if (this.HornsObject != null && this.HornsObject.Blueprint != Variant)
            {
                this.RegrowHorns();
            }
		}

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

		public override bool Unmutate(GameObject GO)
		{
			foreach (BodyPart Head in GO.Body.GetPart("Head"))
			{
                GameObject hornToDestroy = Head.Equipped;
                base.CleanUpMutationEquipment(GO, ref hornToDestroy);
			}
			return base.Unmutate(GO);
		}

		public bool Mutating = false;

		public string HornsName;
		
		public GameObject HornsObject;
        
        public int HeadsAmount = 1;

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
