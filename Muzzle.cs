using System;
using System.Collections.Generic;
using XRL.UI;
using ConsoleLib.Console;
using XRL.Messages;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Muzzle : BaseDefaultEquipmentMutation
    {
        public bool Mutating = false;

        public string BodyPartType = "Face";

        public string MuzzleName = "Muzzle";

        [NonSerialized]
        public List<GameObject> Muzzles = new List<GameObject>();

        //constructor
        public Muzzle()
        {
            this.DisplayName = "Muzzle (&rD&y)";
        }

        public override bool GeneratesEquipment()
        {
            return true;
        }

        //main methods
        public override void SaveData(SerializationWriter Writer)
        {
            Writer.WriteGameObjectList(this.Muzzles);
            base.SaveData(Writer);
        }

        public override void LoadData(SerializationReader Reader)
        {
            Reader.ReadGameObjectList(this.Muzzles, null);
            base.LoadData(Reader);
        }

        public override void Register(GameObject Object)
        {
            base.Register(Object);
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override string GetDescription()
        {
            return "You bear a muzzle.";
        }

        public override string GetLevelText(int level)
        {
            string ret = "";
            ret += "You occasionally bite your opponents" + "\n";
            ret += "You can't wear anything on your face" + "\n";
            return ret;
        }

        public override void OnRegenerateDefaultEquipment(Body body)
        {
            foreach (BodyPart Face in body.GetPart(this.BodyPartType))
            {
                if (Face.VariantType == this.BodyPartType)
                {
                    if (!Mutating)
                    {
                        GameObject muzzleToDestroy = Face.DefaultBehavior;
                        muzzleToDestroy = Face.Equipped;
                        if (muzzleToDestroy != null)
                        {
                            Face.ForceUnequip(true);
                            muzzleToDestroy.Destroy();
                        }
                    }

                    GameObject newMuzzle = GameObjectFactory.Factory.CreateObject("Muzzle");
                    MeleeWeapon meleeWeapon = newMuzzle.GetPart<MeleeWeapon>();
                    newMuzzle.pRender.DisplayName = this.DisplayName;
                    meleeWeapon.Skill = "ShortBlades";
                    meleeWeapon.BaseDamage = "1d3";
                    this.ParentObject.ForceEquipObject(newMuzzle, Face, true, new int?(0));
                }
            }

            Mutating = false;
            base.OnRegenerateDefaultEquipment(body);
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int newLevel)
        {
            return base.ChangeLevel(newLevel);
        }

        public override bool Mutate(GameObject GO, int level)
        {
            Mutating = true;
            return base.Mutate(GO, level);
        }

        public override bool Unmutate(GameObject GO)
        {
            foreach (BodyPart Face in GO.Body.GetPart(this.BodyPartType))
            {
                GameObject muzzleToDestroy;
                muzzleToDestroy = Face.Equipped;
                base.CleanUpMutationEquipment(GO, ref muzzleToDestroy);
            }
            return base.Unmutate(GO);
        }        
    }
}