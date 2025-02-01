using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using ConsoleLib.Console;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    class FluffyTail : BaseMutation
    {
        public string AdditionsManagerID
        {
            get
            {
                return this.ParentObject.ID + "::FluffyTail::Add";
            }
        }

        public FluffyTail()
        {
            DisplayName = "Fluffy Tail";
        }

        public override bool AffectsBodyParts()
        {
            return true;
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "ApplyProne");
            base.Register(Object);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == GetShortDescriptionEvent.ID;
        }

        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            E.Postfix.Append(" " + ParentObject.YouAre + " possessed of a very fluffy tail.");
            return true;
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override string GetDescription()
        {
            string ret = "You have a long, flexible tail covered in thick, wooly fluff. It's incredibly soft and quite cute.";
            return ret;
        }

        public override string GetLevelText(int Level)
        {
            string ret = "It's so fluffy!\n";
            ret += "%50 chance to recover from an attempt to knock you prone\n";
            return ret;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ApplyProne" && !50.in100())
            {
                base.DidX("skid", "around but quickly recover", ".", null, "&R", null, this.ParentObject, false, false, null, null, false, false, false, false, false, null);
                if (base.Visible())
                {
                    this.ParentObject.ParticleText("{{|*skidded*}}", ' ', false, 1.5f, -8f);
                }
                return false;
            }
            return base.FireEvent(E);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            Body body = GO.GetPart("Body") as Body;

            if (body != null)
            {
                // Adds the Fluffy Tail next to the last "Worn on Back" slot of the lowest "Body".
                List<BodyPart> allBodies = this.ParentObject.Body.GetPart("Body");
                BodyPart lastBody = allBodies[allBodies.Count - 1];
                BodyPart theTail = lastBody.AddPartAt(lastBody.GetFirstPart("Back"), "Tail", 0, null, null, null, null, this.AdditionsManagerID);
            }
            
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            base.StatShifter.RemoveStatShifts();
            GO.RemoveBodyPartsByManager(this.AdditionsManagerID, true);
            return base.Unmutate(GO);
        }
    }
}
