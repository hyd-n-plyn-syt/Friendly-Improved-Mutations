using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MultipleLegs : BaseMutation, IRankedMutation
    {
        public int Rank = 1;

        public int Bonus;

        public String footType = "Feet";

        public string AdditionsManagerID
        {
            get
            {
                return this.ParentObject.ID + "::MultipleLegs::Add";
            }
        }

        public string ChangesManagerID
        {
            get
            {
                return this.ParentObject.ID + "::MultipleLegs::Change";
            }
        }

        public MultipleLegs()
        {
            this.DisplayName = "Multiple Legs";
        }

        public override bool AffectsBodyParts()
        {
            return true;
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == GetItemElementsEvent.ID || ID == GetMaxCarriedWeightEvent.ID;
        }

        public override bool HandleEvent(GetItemElementsEvent E)
        {
            E.Add("travel", 1);
            return true;
        }

        public override bool HandleEvent(GetMaxCarriedWeightEvent E)
        {
            E.AdjustWeight((1.0 + (double)this.GetCarryCapacityBonus(base.Level) / 100.0) * (double)this.Rank);
            return base.HandleEvent(E);
        }

        public override string GetDescription()
        {
            return "You have an extra set of legs.";
        }

        public override string GetLevelText(int Level)
        {
            return "+{{rules|" + this.GetMoveSpeedBonus(Level).ToString() + "}} move speed\n" + "+{{rules|" + this.GetCarryCapacityBonus(Level).ToString() + "%}} carry capacity";
        }

        public int GetMoveSpeedBonus(int Level)
        {
            return Level * 20;
        }

        public int GetCarryCapacityBonus(int Level)
        {
            return Level + 5;
        }

        public int GetRank()
        {
            return this.Rank;
        }

        public int AdjustRank(int amount)
        {
            this.Rank += amount;
            return this.Rank;
        }

        public override bool ChangeLevel(int NewLevel)
        {
            base.StatShifter.SetStatShift(this.ParentObject, "MoveSpeed", -this.GetMoveSpeedBonus(NewLevel), true);
            CarryingCapacityChangedEvent.Send(this.ParentObject);
            return base.ChangeLevel(NewLevel);
        }

        public void AddMoreLegs(GameObject GO, bool ignoreArmlessness = false)
        {
            if (GO == null)
            {
                return;
            }
            Body part = GO.Body;
            if (part != null)
            {
                Mutations mutations = GO.GetPart<Mutations>();
                bool isArmless = mutations.HasMutation("Armless");
                if (ignoreArmlessness)
                {
                    isArmless = false;
                }
                if (part.Anatomy == "Anthro")
                {
                    this.footType = "Paws";
                }
                BodyPart body = part.GetBody();
                BodyPart frontFeet = body.GetFirstAttachedPart(footType, 0, part, true);
                BodyPart upperBack = body.GetFirstAttachedPart("Back", 0, part, true);
                BodyPart lowerBody;
                BodyPart rearFeet;
                BodyPart lowerBack;

                if (!isArmless)
                {
                    if (frontFeet != null)
                    {
                        if (body.IsLateralitySafeToChange(0, part, null))
                        {
                            lowerBody = body.AddPartAt(frontFeet, "Torso", 8, null, null, null, null, this.AdditionsManagerID);
                            body.ChangeLaterality(4);
                            body.Manager = this.ChangesManagerID;
                        }
                        else
                        {
                            lowerBody = body.AddPartAt(frontFeet, "Torso", 0, null, null, null, null, this.AdditionsManagerID);
                        }
                    }
                    else
                    {
                        int? num = new int?(body.Category);
                        string[] orInsertBefore = new string[]
                        {
                            "Roots",
                            "Tail",
                            "Thrown Weapon"
                        };
                        lowerBody = body.AddPartAt("Torso", 0, null, null, null, null, this.AdditionsManagerID, num, null, null, null, null, null, null, null, null, null, null, null, null, footType, orInsertBefore, true);
                    }

                    if (upperBack != null)
                    {
                        if (upperBack.IsLateralitySafeToChange(0, part, null))
                        {
                            lowerBack = lowerBody.AddPart("Back", 8, null, null, null, null, this.AdditionsManagerID);
                            upperBack.ChangeLaterality(4);
                            upperBack.Manager = this.ChangesManagerID;
                        }
                        else
                        {
                            lowerBack = body.AddPart("Back", 0, null, null, null, null, this.AdditionsManagerID);
                        }
                    }
                    else
                    {
                        int? num = new int?(body.Category);
                        string[] orInsertBefore = new string[]
                        {
                            "Roots",
                            "Tail",
                            "Thrown Weapon"
                        };
                        lowerBack = body.AddPartAt("Back", 0, null, null, null, null, this.AdditionsManagerID, num, null, null, null, null, null, null, null, null, null, null, null, null, "Back", orInsertBefore, true);
                    }
                }
                else
                {
                    lowerBody = body;
                }

                if (frontFeet != null)
                {
                    if (frontFeet.IsLateralitySafeToChange(0, part, null))
                    {
                        rearFeet = lowerBody.AddPart(footType, 64, null, null, null, null, this.AdditionsManagerID);
                        frontFeet.ChangeLaterality(16);
                        frontFeet.Manager = this.ChangesManagerID;
                    }
                    else
                    {
                        rearFeet = lowerBody.AddPart(footType, 64, null, null, null, null, this.AdditionsManagerID);
                    }
                }
                else
                {
                    int? num = new int?(body.Category);
                    string[] orInsertBefore = new string[]
                    {
                        "Roots",
                        "Tail",
                        "Thrown Weapon"
                    };
                    rearFeet = body.AddPartAt(footType, 0, null, null, null, null, this.AdditionsManagerID, num, null, null, null, null, null, null, null, null, null, null, null, null, footType, orInsertBefore, true);
                }
            }
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.AddMoreLegs(GO);
            this.ChangeLevel(Level);
            Mutations mutations = GO.GetPart<Mutations>();
            if (mutations.HasMutation("FluffyTail"))
            {
                BaseMutation theTail = mutations.GetMutation("FluffyTail");
                int tailLevel = theTail.Level;
                mutations.RemoveMutation(theTail);
                mutations.AddMutation("FluffyTail", tailLevel);
            }
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            GO.RemoveBodyPartsByManager(this.AdditionsManagerID, true);
            foreach (BodyPart current in GO.GetBodyPartsByManager(this.ChangesManagerID, true))
            {
                if (current.Laterality == 16 && current.IsLateralityConsistent(null))
                {
                    current.ChangeLaterality(current.Laterality & -17);
                }
                if (current.Laterality == 4 && current.IsLateralityConsistent(null))
                {
                    current.ChangeLaterality(current.Laterality & -5);
                }
            }
            base.StatShifter.RemoveStatShifts();
            CarryingCapacityChangedEvent.Send(this.ParentObject);
            return base.Unmutate(GO);
        }
    }
}
