using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MultipleHeads : BaseMutation
    {
        private static List<Effect> targetEffects = new List<Effect>();

        public int extraHeads = 1;

        private int shakeOffHead = -1;
        
        public string headType = "Head";
        
        public string faceType = "Face";

        public string AdditionsManagerID
        {
            get
            {
                return this.ParentObject.ID + "::MultipleHeads::Add";
            }
        }

        public string ChangesManagerID
        {
            get
            {
                return this.ParentObject.ID + "::MultipleHeads::Change";
            }
        }

        public MultipleHeads()
        {
            this.DisplayName = "2-Headed";
        }

        public override bool AffectsBodyParts()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "UsingEnergy");
            Object.RegisterPartEvent(this, "EndTurn");
            base.Register(Object);
        }

        public override string GetDescription()
        {
            return "You have " + (this.extraHeads + 1) + " heads.";
        }

        public int GetReducedMentalActionCost(int Level)
        {
            return Convert.ToInt32((1.0 - Math.Pow((1.0 - (0.15 + 0.05 * (double)Level)), (double)this.extraHeads)) * 100.0);
        }

        public int GetShakeOff(int Level)
        {
            return 5 + 2 * Level;
        }

        private bool ShakeItOff(int Level)
        {
            for (int i = 0; i < this.extraHeads; i++)
            {
                if (this.GetShakeOff(Level).in100())
                {
                    shakeOffHead = i;
                    return true;
                }
            }
            shakeOffHead = -1;
            return false;
        }

        public override string GetLevelText(int Level)
        {
            string text = "Mental actions have {{rules|" + this.GetReducedMentalActionCost(Level) + "%}} lower action costs\n";
            return string.Concat(new object[]
            {
                text,
                "{{rules|",
                this.GetShakeOff(Level),
                "%}} chance per extra head initially and each round to shake off a negative mental status effect"
            });
        }

        public BodyPart FindExtraHead()
        {
            List<BodyPart> bodyPartsByManager = this.ParentObject.GetBodyPartsByManager(this.AdditionsManagerID, false);
            List<BodyPart> allHeads = new List<BodyPart>();
            if (bodyPartsByManager == null)
            {
                return null;
            }
            foreach (BodyPart current in bodyPartsByManager)
            {
                if (current.Type == "Head")
                {
                    allHeads.Add(current);
                }
            }
            Body body = this.ParentObject.GetPart("Body") as Body;
            if (body == null)
            {
                return null;
            }
            if (body.GetPartCount("Head") < this.extraHeads + 1)
            {
                return null;
            }
            if (shakeOffHead != -1)
            {
                return allHeads[shakeOffHead];
            }
            else
            {
                return null;
            }
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == ApplyEffectEvent.ID || ID == EndTurnEvent.ID;
        }

        private bool AffectEffect(Effect FX)
        {
            return FX.IsOfTypes(100663298) && !FX.IsOfType(134217728);
        }

        public override bool HandleEvent(ApplyEffectEvent E)
        {
            if (this.AffectEffect(E.Effect) && ShakeItOff(base.Level))
            {
                BodyPart bodyPart = this.FindExtraHead();
                if (bodyPart != null && this.ParentObject.IsPlayer())
                {
                    if (E.Effect.ClassName == E.Effect.DisplayName)
                    {
                        IComponent<GameObject>.AddPlayerMessage(string.Concat(new string[]
                        {
                            "Your ",
                            bodyPart.GetOrdinalName(),
                            " ",
                            bodyPart.Plural ? "help" : "helps",
                            " shake off the effect!"
                        }), 'g');
                    }
                    else
                    {
                        IComponent<GameObject>.AddPlayerMessage(string.Concat(new string[]
                        {
                            "Your ",
                            bodyPart.GetOrdinalName(),
                            " ",
                            bodyPart.Plural ? "help" : "helps",
                            " shake off being ",
                            E.Effect.DisplayName,
                            "!"
                        }), 'g');
                    }
                }
            }
            return true;
        }

        public override bool HandleEvent(EndTurnEvent E)
        {
            if (this.ParentObject.Effects != null && ShakeItOff(base.Level))
            {
                MultipleHeads.targetEffects.Clear();
                int i = 0;
                int count = this.ParentObject.Effects.Count;
                while (i < count)
                {
                    if (this.AffectEffect(this.ParentObject.Effects[i]))
                    {
                        MultipleHeads.targetEffects.Add(this.ParentObject.Effects[i]);
                    }
                    i++;
                }
                if (MultipleHeads.targetEffects.Count > 0)
                {
                    BodyPart bodyPart = this.FindExtraHead();
                    if (bodyPart != null)
                    {
                        Effect randomElement = MultipleHeads.targetEffects.GetRandomElement(null);
                        if (randomElement != null)
                        {
                            if (this.ParentObject.IsPlayer())
                            {
                                if (randomElement.DisplayName == randomElement.ClassName)
                                {
                                    IComponent<GameObject>.AddPlayerMessage(string.Concat(new string[]
                                    {
                                        "Your ",
                                        bodyPart.GetOrdinalName(),
                                        " ",
                                        bodyPart.Plural ? "help" : "helps",
                                        " you shake off a mental state!"
                                    }), 'g');
                                }
                                else
                                {
                                    IComponent<GameObject>.AddPlayerMessage(string.Concat(new string[]
                                    {
                                        "Your ",
                                        bodyPart.GetOrdinalName(),
                                        " ",
                                        bodyPart.Plural ? "help" : "helps",
                                        " you shake off being ",
                                        randomElement.DisplayName,
                                        "!"
                                    }), 'g');
                                }
                            }
                            this.ParentObject.RemoveEffect(randomElement);
                        }
                    }
                }
                MultipleHeads.targetEffects.Clear();
            }
            return true;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "UsingEnergy" && E.GetStringParameter("Type", "").Contains("Mental"))
            {
                E.SetParameter("Amount", (int)((double)E.GetIntParameter("Amount", 0) * Math.Pow((1.0 - (0.15 + 0.05 * (double)base.Level)), (double)this.extraHeads)));
            }
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override void SetVariant(String Variant)
        {
            this.extraHeads = Convert.ToInt32(Variant);
            this.DisplayName = (this.extraHeads + 1) + "-Headed";
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            Body part = GO.Body;
            if (part != null)
            {
                if (part.Anatomy == "Anthro")
                {
                    this.faceType = "Muzzle";
                }
                BodyPart body = part.GetBody();
                BodyPart firstHead = body.GetFirstAttachedPart(headType, 0, part, true);
                BodyPart firstFace = (firstHead != null) ? firstHead.GetFirstAttachedPart(faceType, 0, part, true) : null;
                BodyPart lastHead = firstHead;

                List<int> Lateralitys = new List<int>();
                switch (this.extraHeads)
                {
                    case 1:
                        Lateralitys.Add(1);
                        break;
                    case 2:
                        Lateralitys.Add(32);
                        Lateralitys.Add(1);
                        break;
                    case 3:
                        Lateralitys.Add(32 + 2);
                        Lateralitys.Add(32 + 1);
                        Lateralitys.Add(1);
                        break;
                    case 4:
                        Lateralitys.Add(32 + 2);
                        Lateralitys.Add(32);
                        Lateralitys.Add(32 + 1);
                        Lateralitys.Add(1);
                        break;
                    default:
                        Lateralitys = null;
                        break;
                }

                if (firstHead != null && firstFace != null && firstHead.IsLateralitySafeToChange(0, part, firstFace))
                {
                    firstHead.ChangeLaterality(2);
                    firstFace.ChangeLaterality(2);
                    firstHead.Manager = this.ChangesManagerID;
                    firstFace.Manager = this.ChangesManagerID;
                }

                for (int i = 0; i < this.extraHeads; i++)
                {
                    BodyPart newHead;
                    if (lastHead != null)
                    {
                        newHead = body.AddPartAt(lastHead, headType, Lateralitys[i], null, null, null, null, this.AdditionsManagerID);
                    }
                    else
                    {
                        newHead = body.AddPartAt(headType, 0, null, null, null, null, this.AdditionsManagerID, null, null, null, null, null, null, null, null, null, null, null, null, null, headType, new string[]
                        {
                            "Back",
                            "Arm",
                            "Leg",
                            "Foot",
                            "Hands",
                            "Feet",
                            "Roots",
                            "Thrown Weapon"
                        }, true);
                    }
                    lastHead = newHead;
                    BodyPart newFace = newHead.AddPart(faceType, Lateralitys[i], null, null, null, null, this.AdditionsManagerID);
                }
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            GO.RemoveBodyPartsByManager(this.AdditionsManagerID, true);
            foreach (BodyPart current in GO.GetBodyPartsByManager(this.ChangesManagerID, true))
            {
                if (current.Laterality == 2 && current.IsLateralityConsistent(null))
                {
                    current.ChangeLaterality(0, false);
                }
            }
            return base.Unmutate(GO);
        }
    }
}
