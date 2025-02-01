using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    internal class HooksForFeet2 : BaseDefaultEquipmentMutation
    {
        public string BodyPartType = "Feet";

        public bool Mutating = false;

        [NonSerialized]
        private List<string> variants = new List<string>
        {
            "Hooks",
            "Talons"
        };

        public HooksForFeet2()
        {
            this.DisplayName = "Hooks for Feet ({{r|D}})";
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override bool GeneratesEquipment()
        {
            return true;
        }

        public override string GetDescription()
        {
            return "You have hooks for feet.\n\nYou cannot wear shoes.";
        }

        public override string GetLevelText(int Level)
        {
            return "";
        }

        public override void OnRegenerateDefaultEquipment(Body body)
        {
            foreach (BodyPart foot in body.GetPart("Feet"))
            {
                if (foot.VariantType != "Legs")
                {
                    if (!Mutating)
                    {
                        GameObject currentHook = foot.Equipped;
                        if (currentHook != null)
                        {
                            foot.ForceUnequip(true);
                            currentHook.Destroy();
                        }
                    }

                    GameObject newHook = GameObjectFactory.Factory.CreateObject("Hooks");
                    MeleeWeapon meleeWeapon = newHook.GetPart<MeleeWeapon>();
                    Armor armor = newHook.Armor;
                    Render render = newHook.pRender;
                    render.DisplayName = this.DisplayName;
                    meleeWeapon.Skill = "ShortBlades";
                    meleeWeapon.BaseDamage = "1";
                    armor.WornOn = foot.Type;
                    armor.AV = 0;
                    this.ParentObject.ForceEquipObject(newHook, foot, true, new int?(0));
                }
            }

            Mutating = false;
            base.OnRegenerateDefaultEquipment(body);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override List<string> GetVariants()
        {
            return this.variants;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            Mutating = true;
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            foreach (BodyPart foot in GO.Body.GetPart("Feet"))
            {
                GameObject currentHook = foot.Equipped;
                base.CleanUpMutationEquipment(GO, ref currentHook);
            }
            return base.Unmutate(GO);
        }
    }
}
