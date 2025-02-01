using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class Improved_MultipleArms : BaseMutation, IRankedMutation
    {
        public int Rank = 1;

        public int Pairs = 1;
        
        public override bool CanSelectVariant
        {
            get
            {
                return false;
            }
        }

        public string AdditionsManagerID
        {
            get
            {
                return this.ParentObject.ID + "::MultipleArms::Add";
            }
        }

        public string ChangesManagerID
        {
            get
            {
                return this.ParentObject.ID + "::MultipleArms::Change";
            }
        }

        public Improved_MultipleArms()
        {
            this.DisplayName = "Multiple Arms";
        }

        public override bool AffectsBodyParts()
        {
            return true;
        }

        public override string GetDescription()
        {
            if (Pairs == 1)
            {
                return "You have an extra set of arms.";
            }
            else
            {
                return "You have " + this.Pairs + " extra sets of arms.";
            }
        }

        public override string GetLevelText(int Level)
        {
            return "{{rules|" + this.GetAttackChance(Level).ToString() + "%}} chance for each extra arm to deliver an additional melee attack whenever you make a melee attack";
        }

        public int GetAttackChance(int Level)
        {
            return 7 + Level * 3;
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public int GetRank()
        {
            return this.Rank;
        }

        public int AdjustRank(int amount)
        {
            this.Rank += amount;
            for (int i = 0; i < amount; i++)
            {
                this.AddMoreArms(this.ParentObject);
            }
            return this.Rank;
        }

        public void AddMoreArms(GameObject GO)
        {
            if (GO == null)
            {
                return;
            }
            Body body = GO.Body;
            if (body != null)
            {
                BodyPart mainBody = body.GetBody();
                BodyPart upperRightArm = mainBody.GetFirstAttachedPart("Arm", 2, body, true);
                BodyPart upperRightHand = (upperRightArm == null) ? null : upperRightArm.GetFirstAttachedPart("Hand", 2, body, true);
                BodyPart upperRightMissileWeapon = mainBody.GetFirstAttachedPart("Missile Weapon", 2, body, true);
                if (upperRightMissileWeapon == null)
                {
                    upperRightMissileWeapon = mainBody.GetFirstAttachedPart("Missile Weapon", 0, body, true);
                    this.ProcessChangedLimb(upperRightMissileWeapon.ChangeLaterality(2 + 4));
                }
                BodyPart upperLeftArm = mainBody.GetFirstAttachedPart("Arm", 1, body, true);
                BodyPart upperLeftHand = (upperLeftArm == null) ? null : upperLeftArm.GetFirstAttachedPart("Hand", 1, body, true);
                BodyPart upperLeftMissileWeapon = mainBody.GetFirstAttachedPart("Missile Weapon", 1, body, true);
                if (upperLeftMissileWeapon == null)
                {
                    upperLeftMissileWeapon = mainBody.GetFirstAttachedPart("Missile Weapon", 0, body, true);
                    this.ProcessChangedLimb(upperLeftMissileWeapon.ChangeLaterality(1 + 4));
                }
                BodyPart upperHands = mainBody.GetFirstAttachedPart("Hands", 0, body, true);
                BodyPart lastArm = upperLeftArm;
                BodyPart lastMissileWeapon = upperLeftMissileWeapon;
                BodyPart lastHands = upperHands;

                List<int> Lateralitys = new List<int>();
                switch (this.Pairs)
                {
                    case 1:
                        Lateralitys.Add(8);
                        break;
                    case 2:
                        Lateralitys.Add(32);
                        Lateralitys.Add(8);
                        break;
                    case 3:
                        Lateralitys.Add(32 + 4);
                        Lateralitys.Add(32 + 8);
                        Lateralitys.Add(8);
                        break;
                    case 4:
                        Lateralitys.Add(32 + 4);
                        Lateralitys.Add(32);
                        Lateralitys.Add(32 + 8);
                        Lateralitys.Add(8);
                        break;
                    default:
                        Lateralitys = null;
                        break;
                }

                for (int i = 0; i < this.Pairs; i++)
                {
                    string text = (i == 0) ? "Multiple Arms Hands" : ("Multiple Arms Hands " + (i + 1).ToString());
                    if (i == 0 && upperRightArm != null && upperRightArm.Manager == null && upperRightHand != null && upperRightHand.Manager == null && upperLeftArm != null && upperLeftArm.Manager == null && upperLeftHand != null && upperLeftHand.Manager == null && upperHands != null && upperHands.Manager == null && upperRightArm.IsLateralitySafeToChange(2, body, upperRightHand) && upperLeftArm.IsLateralitySafeToChange(1, body, upperLeftHand) && upperHands.IsLateralitySafeToChange(0, body, null) && upperHands.DependsOn == upperRightHand.SupportsDependent && upperHands.DependsOn == upperLeftHand.SupportsDependent)
                    {
                        int? num = new int?(mainBody.Category);
                        BodyPart newRightArm = mainBody.AddPartAt(lastArm, "Arm", 2 + Lateralitys[i], null, null, null, null, this.AdditionsManagerID, num);
                        lastArm = newRightArm;
                        newRightArm.AddPart("Hand", 2 + Lateralitys[i], null, text, null, null, this.AdditionsManagerID, num);
                        BodyPart newRightMissileWeapon = mainBody.AddPartAt(lastMissileWeapon, "Missile Weapon", 2 + Lateralitys[i], null, null, null, null, this.AdditionsManagerID, null, 2 + Lateralitys[i]);
                        lastMissileWeapon = newRightMissileWeapon;
                        BodyPart newLeftArm = mainBody.AddPartAt(lastArm, "Arm", 1 + Lateralitys[i], null, null, null, null, this.AdditionsManagerID, num);
                        lastArm = newLeftArm;
                        newLeftArm.AddPart("Hand", 1 + Lateralitys[i], null, text, null, null, this.AdditionsManagerID, num);
                        BodyPart newLeftMissileWeapon = mainBody.AddPartAt(lastMissileWeapon, "Missile Weapon", 1 + Lateralitys[i], null, null, null, null, this.AdditionsManagerID, null, 1 + Lateralitys[i]);
                        lastMissileWeapon = newLeftMissileWeapon;
                        lastHands = mainBody.AddPartAt(lastHands, "Hands", Lateralitys[i], null, null, text, null, this.AdditionsManagerID, num);
                        this.ProcessChangedLimb(upperRightArm.ChangeLaterality(upperRightArm.Laterality | 4));
                        this.ProcessChangedLimb(upperRightHand.ChangeLaterality(upperRightHand.Laterality | 4));
                        this.ProcessChangedLimb(upperRightMissileWeapon.ChangeLaterality(upperRightMissileWeapon.Laterality | 4));
                        this.ProcessChangedLimb(upperLeftArm.ChangeLaterality(upperLeftArm.Laterality | 4));
                        this.ProcessChangedLimb(upperLeftHand.ChangeLaterality(upperLeftHand.Laterality | 4));
                        this.ProcessChangedLimb(upperLeftMissileWeapon.ChangeLaterality(upperLeftMissileWeapon.Laterality | 4));
                        this.ProcessChangedLimb(upperHands.ChangeLaterality(upperHands.Laterality | 4));
                        upperRightMissileWeapon.RequiresLaterality = upperRightHand.Laterality;
                        upperLeftMissileWeapon.RequiresLaterality = upperLeftHand.Laterality;
                    }
                    else
                    {
                        int rightLaterality = 2;
                        int leftLaterality = 1;
                        int handsLaterality = 0;

                        if (Lateralitys != null)
                        {
                            rightLaterality |= Lateralitys[i];
                            leftLaterality |= Lateralitys[i];
                            handsLaterality |= Lateralitys[i];
                        }
                        int? num = new int?(mainBody.Category);
                        BodyPart newRightArm;
                        if (lastArm != null)
                        {
                            lastArm = (newRightArm = mainBody.AddPartAt(lastArm, "Arm", rightLaterality, null, null, null, null, this.AdditionsManagerID, num));
                        }
                        else
                        {
                            string[] orInsertBefore = new string[]
                            {
                                "Hands",
                                "Feet",
                                "Roots",
                                "Thrown Weapon"
                            };
                            lastArm = (newRightArm = mainBody.AddPartAt("Arm", rightLaterality, null, null, null, null, this.AdditionsManagerID, num, null, null, null, null, null, null, null, null, null, null, null, null, "Arm", orInsertBefore, true));
                        }
                        newRightArm.AddPart("Hand", rightLaterality, null, text, null, null, this.AdditionsManagerID, num);
                        BodyPart newRightMissileWeapon = mainBody.AddPartAt(lastMissileWeapon, "Missile Weapon", rightLaterality, null, null, null, null, this.AdditionsManagerID, null, rightLaterality);
                        lastMissileWeapon = newRightMissileWeapon;
                        BodyPart newLeftArm = mainBody.AddPartAt(lastArm, "Arm", leftLaterality, null, null, null, null, this.AdditionsManagerID, num);
                        lastArm = newLeftArm;
                        newLeftArm.AddPart("Hand", leftLaterality, null, text, null, null, this.AdditionsManagerID, num);
                        BodyPart newLeftMissileWeapon = mainBody.AddPartAt(lastMissileWeapon, "Missile Weapon", leftLaterality, null, null, null, null, this.AdditionsManagerID, null, leftLaterality);
                        lastMissileWeapon = newLeftMissileWeapon;
                        if (lastHands != null)
                        {
                            lastHands = mainBody.AddPartAt(lastHands, "Hands", handsLaterality, null, null, text, null, this.AdditionsManagerID, num);
                        }
                        else
                        {
                            string[] orInsertBefore = new string[]
                            {
                                "Feet",
                                "Roots",
                                "Thrown Weapon"
                            };
                            lastHands = mainBody.AddPartAt("Hands", handsLaterality, null, null, text, null, this.AdditionsManagerID, num, null, null, null, null, null, null, null, null, null, null, null, null, "Hands", orInsertBefore, true);
                        }
                    }
                }
            }
        }
        
        public override void SetVariant(string Variant)
        {
            this.Pairs = Convert.ToInt32(Variant);
            this.DisplayName = "Multiple Arms (" + (2 * (this.Pairs + 1)) + ")";
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.AddMoreArms(GO);
            //GO.RegisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChance");
            return base.Mutate(GO, Level);
        }

        private BodyPart ProcessChangedLimb(BodyPart Part)
        {
            if (Part != null)
            {
                Part.Manager = this.ChangesManagerID;
            }
            return Part;
        }

        public override bool Unmutate(GameObject GO)
        {
            GO.RemoveBodyPartsByManager(this.AdditionsManagerID, true);
            foreach (BodyPart current in GO.GetBodyPartsByManager(this.ChangesManagerID, true))
            {
                if (current.HasLaterality(4) && current.IsLateralityConsistent(null))
                {
                    current.ChangeLaterality(current.Laterality & -5);
                }
            }
            //GO.UnregisterPartEvent(this, "AttackerQueryWeaponSecondaryAttackChance");
            return base.Unmutate(GO);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == GetMeleeAttackChanceEvent.ID;
        }

        public override bool HandleEvent(GetMeleeAttackChanceEvent E)
        {
            if (E.Intrinsic && !E.Primary)
            {
                BodyPart bodyPart = E.BodyPart;
                if (((bodyPart != null) ? bodyPart.Manager : null) != null && E.BodyPart.Manager == this.AdditionsManagerID)
                {
                    E.Chance += this.GetAttackChance(base.Level) - RuleSettings.BASE_SECONDARY_ATTACK_CHANCE;
                }
            }
            return base.HandleEvent(E);
        }
    }
}