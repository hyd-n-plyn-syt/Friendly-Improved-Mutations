using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class HydraHeads : BaseMutation
    {
        public Guid ChangeMaxHeadsActivatedAbilityID = Guid.Empty;
        private int MaxHeads = 9;                           // The amount of heads you can grow
        public int TotalHeads = 2;                          // The amount of heads you are about to grow
        public int CurrentHeads = 2;                        // The amount of heads you currently have
        public int DisembodiedHeads = 0;                    // Only for use with Cranial Separator implant
        public List<int> RegrowTimes = new List<int>();     // A list of times before you regrow a set of heads

        public string AdditionsManagerID
        {
            get
            {
                return this.ParentObject.ID + "::HydraHeads::Add";
            }
        }

        public string ChangesManagerID
        {
            get
            {
                return this.ParentObject.ID + "::HydraHeads::Change";
            }
        }

        public HydraHeads()
        {
            this.DisplayName = "Hydra Heads";
        }

        public override bool AffectsBodyParts()
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "DefendMeleeHit");
            Object.RegisterPartEvent(this, "EndTurn");
            Object.RegisterPartEvent(this, "CommandChangeMaxHeads");
            base.Register(Object);
        }

        public override bool WantEvent (int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == BeforeDismemberEvent.ID;
        }

        public override bool HandleEvent (BeforeDismemberEvent E)
        {
            if (E.Part.Type == "Head")
            {
                this.Decapitate();
                return false;
            }
            return base.HandleEvent(E);
        }

        public override string GetDescription()
        {
            return "You have two heads.\nIf one of your heads gets cut off, you gain two heads in return.\nBeing lit on fire while decapitated will cause you to lose that head permanently";
        }

        public override string GetLevelText(int Level)
        {
            string text = "Small chance whenever you are attacked by a sharp weapon to lose a head\n";
            if (Level == base.Level)
            {
                text += "Whenever you lose a head, you will grow back two extras\n";
            }
            else
            {
                if (Level >= 10)
                {
                    text += "Your extra heads will grow back almost instantaneously\n";
                }
                else
                {
                    text += "Your extra heads will grow back quicker\n";
                }
                text += "You bleed less from decapitations\n";
            }
            return text;
        }

        private void Decapitate(GameObject Attacker = null)
        {
            this.CurrentHeads--;
            this.TotalHeads++;
            Body body = this.ParentObject.Body;

            // If you still have heads left
            if (this.CurrentHeads > 0)
            {
                // Maximum of nine heads to keep things from getting out of hand
                if (this.TotalHeads > this.MaxHeads)
                {
                    TotalHeads = MaxHeads;
                }

                // Always cutting the last head just for simplicity's sake
                BodyPart HeadToCut = body.GetPart("Head")[this.CurrentHeads];
                BodyPart FaceToCut = HeadToCut.GetFirstAttachedPart("Face", body, true);
                
                HeadToCut.UnequipPartAndChildren(true);

                // Cause the decapited head to drop on the ground to be used as an item
                GameObject gameObject = GameObjectFactory.Factory.CreateObject(this.ParentObject.GetPropertyOrTag("SeveredHeadBlueprint", "GenericHead"), 0, 0, null, null, null, "Dismember", null);
                Render pRender = gameObject.pRender;
                pRender.DisplayName = this.ParentObject.DisplayName + "'s &r" + HeadToCut.Name.ToLower();
                this.ParentObject.pPhysics.CurrentCell.AddObject(gameObject);

                // Cut off the head. Yes, this is a bit putting the cart before the horse.
                if (this.ParentObject.IsPlayer())
                {
                    Popup.Show("Your " + HeadToCut.Name.ToLower() + " is dismembered!");
                }
                else
                {
                    if (Attacker == null)
                    {
                        MessageQueue.AddPlayerMessage(this.ParentObject.The + this.ParentObject.DisplayName + " loses one of " + this.ParentObject.its + " heads.");
                    }
                    else if (Attacker.IsPlayer())
                    {
                        MessageQueue.AddPlayerMessage("You cut off one of " + this.ParentObject.the + this.ParentObject.DisplayName + "'s heads.");
                    }
                    else
                    {
                        MessageQueue.AddPlayerMessage(Attacker.The + Attacker.DisplayName + " cuts off one of " + this.ParentObject.the + this.ParentObject.DisplayName + "'s heads.");
                    }

                }
                body.GetBody().RemovePartByID(HeadToCut.ID);
                body.GetBody().RemovePartByID(FaceToCut.ID);
                this.ProcessRemovedLimb(HeadToCut);
                this.ProcessRemovedLimb(FaceToCut);
                this.ParentObject.ApplyEffect(new Bleeding("1d5", 21 - base.Level, Attacker, true));
                this.RegrowTimes.Add(111 - (11 * base.Level));
            }
            // In this case, you don't have any heads left (and you're not a robot)
            else if (this.CurrentHeads >= 0 && this.ParentObject.GetIntProperty("Inorganic", 0) == 0)
            {
                // YASD
                if (this.ParentObject.IsPlayer())
                {
                    if (Attacker == null)
                    {
                        MessageQueue.AddPlayerMessage("You lose your last head!");
                    }
                    else
                    {
                        MessageQueue.AddPlayerMessage("&R" + Attacker.The + Attacker.DisplayName + "&R decapitates your last head!!");
                    }
                }
                else
                {
                    // Keep in mind that this mutation can show up on random mobs
                    if (Attacker != null && Attacker.IsPlayer())
                    {
                        MessageQueue.AddPlayerMessage("&GYou lop off " + this.ParentObject.the + this.ParentObject.ShortDisplayName + "&G's last head!");
                    }
                }

                // Summon particle text in case the hydra getting killed isn't you
                if (!this.ParentObject.IsPlayer())
                {
                    this.ParentObject.ParticleText("&G*Decapitated!*");
                }

                // Still drop the decapitated head on the ground
                GameObject gameObject = GameObjectFactory.Factory.CreateObject(this.ParentObject.GetTag("HeadBase", "Corpse"));
                Render render = gameObject.pRender;
                render.DisplayName = this.ParentObject.DisplayName + "'s &rhead";

                // And now we kill the hydra
                this.ParentObject.pPhysics.CurrentCell.AddObject(gameObject);
                if (Attacker == null)
                {
                    this.ParentObject.Die(null, "decapitation", "You lost your last head.", this.ParentObject.It + " lost " + this.ParentObject.its + "last head.");
                }
                else
                {
                    this.ParentObject.Die(Attacker, "decapitation", "You had your last head cut off by " + Attacker.DisplayName + ".", this.ParentObject.It + " had " + this.ParentObject.its + "last head cut off by " + Attacker.DisplayName);
                }
            }
        }

        private void RegrowHeads()
        {
            Body body = this.ParentObject.Body;
            int PreviousHeads = body.GetPart("Head").Count;

            // You already have 9 heads so we're not going to waste our time with trying to regrow more
            if (this.CurrentHeads >= this.MaxHeads)
            {
                this.CurrentHeads = this.MaxHeads;
                this.RegrowTimes.Clear();
                return;
            }
            // In this case, you probably have 8 heads or there's some other unforeseen situation that leaves you with only one head to regrow
            else if (this.CurrentHeads == this.TotalHeads - 1)
            {
                this.CurrentHeads++;

                if (this.ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("A head grows back from your neck stump.");
                }
                else
                {
                    MessageQueue.AddPlayerMessage(this.ParentObject.The + this.ParentObject.ShortDisplayName + " grows a head back from its neck stump.");
                }
                this.RegrowTimes.Clear();
            }
            else
            {
                this.CurrentHeads += 2;

                if (this.ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("Two heads grow back from your neck stump.");
                }
                else
                {
                    MessageQueue.AddPlayerMessage(this.ParentObject.The + this.ParentObject.ShortDisplayName + " grows two heads back from its neck stump.");
                }
            }

            // The previous if-elsif-else block was just for messages, now we get to the actual re-growing
            if (body != null)
            {
                BodyPart lastHead = body.GetPart("Head")[PreviousHeads - 1];
                for (int i = PreviousHeads; i < this.CurrentHeads; i++)
                {
                    BodyPart newHead = body.GetBody().AddPartAt(lastHead, "Head", 0, null, null, null, null, this.AdditionsManagerID);
                    BodyPart newFace = newHead.AddPart("Face", 0, null, null, null, null, this.AdditionsManagerID);
                    lastHead = newHead;
                }

                this.SetLateralitys();

                // Add in any muzzles, beaks, or horns that need adding
                Mutations allMutations = this.ParentObject.GetPart<Mutations>();
                if (allMutations.HasMutation("Horns"))
                {
                    BaseMutation theHorns = allMutations.GetMutation("Horns");
                    theHorns.ChangeLevel(theHorns.Level);
                }
            }
        }

        // Tells if the hydra has any mental status effects and gives a list of them in Return
        private static bool ContainsMentalStatusEffect(GameObject ToCheck, List<Effect> Return)
        {
            bool ReturnValue = false;
            foreach (Effect current in ToCheck.Effects)
            {
                if (current.IsOfTypes(100663298) && !current.IsOfType(134217728))
                {
                    ReturnValue = true;
                    Return.Add(current);
                }
            }
            return ReturnValue;
        }

        public void SetLateralitys()
        {
            int[] Lateralitys;
            Body body = this.ParentObject.Body;

            switch (this.CurrentHeads)
            {
                case 1:
                    Lateralitys = new int[]
                        {0};
                    break;
                case 2:
                    Lateralitys = new int[]
                        {2, 1};
                    break;
                case 3:
                    Lateralitys = new int[]
                        {2, 32, 1};
                    break;
                case 4:
                    Lateralitys = new int[]
                        {2, 16, 1, 64 };
                    break;
                case 5:
                    Lateralitys = new int[]
                        {16, 2, 32, 1, 64};
                    break;
                case 6:
                    Lateralitys = new int[]
                        {2+16, 32+16, 1+16, 2+64, 32+64, 1+64};
                    break;
                case 7:
                    Lateralitys = new int[]
                        {2+16, 32+16, 1+16, 32, 2+64, 32+64, 1+64};
                    break;
                case 8:
                    Lateralitys = new int[]
                        {2+16, 32+16, 1+16, 32+2, 32+1, 2+64, 32+64, 1+64};
                    break;
                case 9:
                    Lateralitys = new int[]
                        {2+16, 32+16, 1+16, 2+32, 32, 1+32, 2+64, 32+64, 1+64};
                    break;
                default:
                    Lateralitys = null;
                    break;
            }

            for (int i = 0; i < this.CurrentHeads; i++)
            {
                BodyPart currentHead = body.GetPart("Head")[i];
                BodyPart currentFace = currentHead.GetFirstAttachedPart("Face", body, true);

                if (Lateralitys != null)
                {
                    currentHead.ChangeLaterality(Lateralitys[i]);
                    currentFace.ChangeLaterality(Lateralitys[i]);
                }
                else
                {
                    currentHead.ChangeLaterality(0);
                    currentFace.ChangeLaterality(0);
                }
            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeforeDecapitate")
            {
                this.Decapitate();
                return false;
            }
            if (E.ID == "DefendMeleeHit")
            {
                GameObject attacker = E.GetParameter("Attacker") as GameObject;
                GameObject incomingWeapon = E.GetParameter("Weapon") as GameObject;
                String skill = "";
                int chance = 5;
                
                if (incomingWeapon != null && incomingWeapon.HasPart("MeleeWeapon"))
                {
                    skill = incomingWeapon.GetPart<MeleeWeapon>().Skill;
                    if ((skill == "Axe" || skill == "LongBlades") &&                                // The weapon needs to be edged and fairly long for a decapitation to happen
                        ((attacker == this.ParentObject && this.CurrentHeads > 1)                   // Secret trick ;)
                        || (Stat.Random(1, 100) <= chance && attacker != this.ParentObject)))       // 5% chance of a decapitation, 30% if the attacker has the decapitation skill
                    {
                        this.Decapitate(attacker);
                    }
                }
                return true;
            }
            if (E.ID == "CommandChangeMaxHeads")
            {
                int number;
                String theNumber = Popup.AskString("How many potential heads do you want?", "9");
                try
                {
                    number = Convert.ToInt32(theNumber);
                }
                catch (FormatException)
                {
                    Popup.Show("Please enter a number.");
                    return false;
                }
                if (number < 2 || number > 99)
                {
                    Popup.Show("That number is out of range.");
                    return false;
                }
                
                this.MaxHeads = number;
                return true;
            }
            if (E.ID == "EndTurn")
            {
                // The nine-headed hydra could have a head permanently removed by fire so...
                if (this.ParentObject.HasEffect("Burning"))
                {
                    // If you still have heads that need to be regrown
                    if (TotalHeads > CurrentHeads)
                    {
                        MessageQueue.AddPlayerMessage("Your neck stumps are cauterized by the flames.");
                    }

                    // Even if that weren't the case, you're still not getting new heads anytime soon
                    TotalHeads = CurrentHeads;
                    RegrowTimes.Clear();

                    // If you're down to your last head, you can regrow a new one but it's going to take a good deal longer than usual
                    if (CurrentHeads == 1)
                    {
                        TotalHeads = 2;
                        RegrowTimes.Add(1000);
                    }
                }
                else
                {
                    // A check to make sure we don't regrow extra heads unnecessarily
                    if (TotalHeads <= CurrentHeads)
                    {
                        RegrowTimes.Clear();
                    }
                    // Count down the amount of time until you start regrowing heads
                    for (int index = 0; index < RegrowTimes.Count; index++)
                    {
                        RegrowTimes[index]--;

                        // Wait! You've got heads to regrow!
                        if (RegrowTimes[index] <= 0)
                        {
                            this.RegrowHeads();
                            RegrowTimes.RemoveAt(index);
                            index--;
                        }
                    }
                }

                // Lastly, we check to see if there are any mental status effects that need to be shaken off
                List<Effect> AllEffects = new List<Effect>();
                if (ContainsMentalStatusEffect(this.ParentObject, AllEffects))
                {
                    // You get re-rolled for each head
                    for (int i = 2; i <= this.CurrentHeads; i++)
                    {
                        // But you get less of a chance than a straight two-headed mutation for each chance
                        if (Stat.Random(1, 100) <= 5)
                        {
                            if (this.ParentObject.IsPlayer())
                            {
                                MessageQueue.AddPlayerMessage("&gYour extra heads help shake off the effect!");
                            }

                            this.ParentObject.RemoveEffect(AllEffects[0]);

                            // And you only get to shake off one effect at a time
                            return true;
                        }
                    }
                }
            }
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            Body part = GO.Body;
            if (part != null)
            {
                BodyPart body = part.GetBody();
                BodyPart originalHead = body.GetFirstAttachedPart("Head", 0, part, true);
                BodyPart originalFace = (originalHead != null) ? originalHead.GetFirstAttachedPart("Face", 0, part, true) : null;
                BodyPart newHead;
                BodyPart newFace;
                if (originalHead != null && originalFace != null && originalHead.IsLateralitySafeToChange(0, part, originalFace))
                {
                    newHead = body.AddPartAt(originalHead, "Head", 1, null, null, null, null, this.AdditionsManagerID);
                    newFace = newHead.AddPart("Face", 1, null, null, null, null, this.AdditionsManagerID);
                    this.ProcessChangedLimb(originalHead.ChangeLaterality(2));
                    this.ProcessChangedLimb(originalFace.ChangeLaterality(2));
                }
                else
                {
                    if (originalHead != null)
                    {
                        newHead = body.AddPartAt(originalHead, "Head", 0, null, null, null, null, this.AdditionsManagerID);
                    }
                    else
                    {
                        string[] orInsertBefore = new string[]
                        {
                            "Back",
                            "Arm",
                            "Leg",
                            "Foot",
                            "Hands",
                            "Feet",
                            "Roots",
                            "Thrown Weapon"
                        };
                        newHead = body.AddPartAt("Head", 0, null, null, null, null, this.AdditionsManagerID, null, null, null, null, null, null, null, null, null, null, null, null, null, "Head", orInsertBefore, true);
                    }
                    newFace = newHead.AddPart("Face", 0, null, null, null, null, this.AdditionsManagerID);
                }
                Mutations allMutations = this.ParentObject.GetPart<Mutations>();
                if (allMutations.HasMutation("Horns"))
                {
                    BaseMutation theHorns = allMutations.GetMutation("Horns");
                    theHorns.ChangeLevel(theHorns.Level);
                }
            }
            if (Options.EnablePrereleaseContent)
            {
                this.ChangeMaxHeadsActivatedAbilityID = base.AddMyActivatedAbility("Change Maximum Amount of Heads", "CommandChangeMaxHeads", "Physical Mutation");
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        private BodyPart ProcessRemovedLimb(BodyPart Part)
        {
            if (Part != null && Part.Manager != null && Part.Manager == this.AdditionsManagerID)
            {
                Part.Manager = null;
            }
            return Part;
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
                if ((current.Laterality == 2 || current.Laterality == 2+16) && current.IsLateralityConsistent(null))
                {
                    current.ChangeLaterality(0);
                }
            }
            return base.Unmutate(GO);
        }
    }
}