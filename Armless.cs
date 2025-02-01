using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class Armless : BaseMutation
    {
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

        public Armless()
        {
            this.DisplayName = "Armless (&rD&y)";
        }

        public override bool CanLevel()
        {
            return false;
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
            return base.WantEvent(ID, cascade) || ID == GetItemElementsEvent.ID;
        }

        public override bool HandleEvent(GetItemElementsEvent E)
        {
            E.Add("travel", 1);
            return true;
        }

        public override string GetDescription()
        {
            return "You have no arms.";
        }

        public override string GetLevelText(int Level)
        {
            return "You are unable to carry any weapons";
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            Body body = GO.Body;
            foreach (BodyPart current in body.GetParts())
            {
                if (current.Type == "Arm")
                {
                    body.RemovePart(current);
                }
            }
            this.ChangeLevel(Level);
            Mutations mutations = GO.GetPart<Mutations>();
            if (mutations.HasMutation("MultipleLegs"))
            {
                MultipleLegs theLegs = mutations.GetMutation("MultipleLegs") as MultipleLegs;
                foreach (BodyPart lowerTorso in body.GetPart("Torso", 8))
                {
                    body.RemovePart(lowerTorso);
                }
                body.GetBody().ChangeLaterality(0);
                body.GetBody().GetFirstPart("Back").ChangeLaterality(0);
                body.GetBody().AddPart("Feet", 64, null, null, null, null, this.AdditionsManagerID);
            }
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            BodyPart mainBody = GO.Body.GetBody();
            int? num = new int?(mainBody.Category);
            string[] orInsertBefore = new string[]
            {
                "Hands",
                "Feet",
                "Roots",
                "Thrown Weapon"
            };
            BodyPart newRightArm = mainBody.AddPartAt("Arm", 2, null, null, null, null, null, num, null, null, null, null, null, null, null, null, null, null, null, null, "Arm", orInsertBefore, true);
            newRightArm.AddPart("Hand", 2);
            BodyPart newLeftArm = mainBody.AddPartAt("Arm", 1, null, null, null, null, null, num, null, null, null, null, null, null, null, null, null, null, null, null, "Arm", orInsertBefore, true);
            newLeftArm.AddPart("Hand", 1);
            Mutations mutations = GO.GetPart<Mutations>();
            if (mutations.HasMutation("MultipleLegs"))
            {
                GO.RemoveBodyPartsByManager(this.AdditionsManagerID, true);
                mainBody.GetFirstPart("Feet").ChangeLaterality(0);
                MultipleLegs theLegs = mutations.GetMutation("MultipleLegs") as MultipleLegs;
                theLegs.AddMoreLegs(GO, true);
            }
            return base.Unmutate(GO);
        }
    }
}
