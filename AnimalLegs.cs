using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class AnimalLegs : BaseDefaultEquipmentMutation
    {
        public bool mutate = false;

        public string BodyPartType = "Feet";

        public string feetName = "Paws";

        [NonSerialized]
        private List<string> variants = new List<string>
        {
            "Paws",
            "Hooves"
        };

        //constructor
        public AnimalLegs()
        {
            this.DisplayName = "Animal Legs";
            this.Type = "Physical";
        }

        //main methods
        public override bool GeneratesEquipment()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override string GetDescription()
        {
            string retval = "Your legs are shaped like those of an animal";
            if (feetName != null)
            {
                return retval + ", ending in " + feetName.ToLower() + ".";
            }
            return retval + ".";
        }

        public override string GetLevelText(int level)
        {
            string ret = "";
            ret += "+" + GetSpeedBonus(level) + " quickness\n";
            ret += "Cannot wear boots\n";
            return ret;
        }

        public override void OnRegenerateDefaultEquipment(Body body)
        {
            foreach (BodyPart Feet in body.GetPart(this.BodyPartType))
            {
                if (mutate)
                {
                    Feet.ForceUnequip(true);
                }
                else
                {
                    GameObject animalLeg = Feet.Equipped;
                    if (animalLeg != null)
                    {
                        Feet.ForceUnequip(true);
                        animalLeg.Destroy();
                    }
                }
                GameObject currentFoot = GameObjectFactory.Factory.CreateObject("AnimalLegs");
                Armor armor = currentFoot.Armor;
                armor.WornOn = Feet.Type;
                Render render = currentFoot.pRender;
                render.DisplayName = this.feetName;
                this.ParentObject.ForceEquipObject(currentFoot, Feet, true, new int?(0));
            }
            
            mutate = false;
            base.OnRegenerateDefaultEquipment(body);
        }

        public override bool ChangeLevel(int newLevel)
        {
            base.StatShifter.SetStatShift(this.ParentObject, "Speed", this.GetSpeedBonus(newLevel), true);

            return base.ChangeLevel(newLevel);
        }

        public override List<string> GetVariants()
        {
            return this.variants;
        }

        public override void SetVariant(String variant)
        {
            this.DisplayName = "Animal Legs (" + variant + ")";
            this.feetName = variant;
            base.SetVariant(variant);
        }

        public override bool Mutate(GameObject GO, int level)
        {
            if (this.ParentObject.GetGenotype() == "Anthropomorphic")
            {
                this.BodyPartType = "Paws";
            }
            mutate = true;
            this.ChangeLevel(level);
            return base.Mutate(GO, level);
        }

        public override bool Unmutate(GameObject GO)
        {
            base.StatShifter.RemoveStatShifts();
            Body body = GO.Body;
            foreach (BodyPart foot in body.GetPart("Feet"))
            {
                GameObject animalLeg = foot.Equipped;
                base.CleanUpMutationEquipment(GO, ref animalLeg);
            }
            return base.Unmutate(GO);
        }

        //utility methods
        public int GetSpeedBonus(int nextLevel)
        {
            int bonus = (3 * nextLevel);
            return bonus;
        }

        public int GetSpeedBonus()
        {
            int bonus = (3 * this.Level);
            return bonus;
        }

    }
}