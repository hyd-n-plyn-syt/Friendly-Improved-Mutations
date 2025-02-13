using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class Carapace : BaseDefaultEquipmentMutation
    {
        public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
        {
            Carapace carapace = base.DeepCopy(Parent, MapInv) as Carapace;
            return carapace;
        }

        public Carapace()
        {
            this.DisplayName = "Carapace";
        }

        public override bool GeneratesEquipment()
        {
            return true;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == AIGetDefensiveAbilityListEvent.ID || ID == GetEnergyCostEvent.ID;
        }

        public override bool HandleEvent(AIGetDefensiveAbilityListEvent E)
        {
            int high = Math.Max(this.ParentObject.baseHitpoints - E.Distance, 1);
            if (!this.Tight && this.ACModifier >= 1 && E.Actor.HasStat("Hitpoints") && base.IsMyActivatedAbilityAIUsable(this.ActivatedAbilityID, null) && Stat.Random(0, high) > E.Actor.hitpoints)
            {
                E.Add("CommandTightenCarapace", 1, null, false, false, null, null);
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetEnergyCostEvent E)
        {
            if (this.Tight && (E.Type == null || (!E.Type.Contains("Pass") && !E.Type.Contains("Mental") && !E.Type.Contains("Carapace"))))
            {
                this.Loosen();
                if (this.ParentObject.IsPlayer())
                {
                    Popup.Show("Your carapace loosens. Your AV decreases by {{R|" + this.ACModifier.ToString() + "}}.");
                }
                else
                {
                    IComponent<GameObject>.EmitMessage(this.ParentObject, Grammar.MakePossessive(this.ParentObject.T(int.MaxValue, null, null, false, false, false, false, false, true, true, false, null, false, true, false)) + " carapace loosens.", ' ', false, false, false);
                }
            }
            return base.HandleEvent(E);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "BeginMove");
            Object.RegisterPartEvent(this, "CommandTightenCarapace");
            Object.RegisterPartEvent(this, "IsMobile");
            base.Register(Object);
        }

        public override string GetDescription()
        {
            return "You are protected by a durable carapace.";
        }

        public override string GetLevelText(int Level)
        {
            return "+{{C|" + (3 + (int)(Level / 2)) + "}} AV\n" + "-2 DV\n" + "+{{C|" + (5 + 5 * Level).ToString() + "}} Heat Resistance\n" + "+{{C|" + (5 + 5 * Level).ToString() + "}} Cold Resistance\n" + "You may tighten your carapace to receive double the AV bonus at a -2 DV penalty as long as you remain still.\n" + "Cannot wear body armor.\n" + "+400 reputation with {{w|tortoises}}";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandTightenCarapace")
            {
                this.Loosen();
                int acmodifier = this.ACModifier;
                if (acmodifier < 1)
                {
                    if (this.ParentObject.IsPlayer())
                    {
                        Popup.ShowFail("You fail to tighten your carapace.", true, true, true);
                    }
                    return false;
                }
                base.UseEnergy(1000, "Physical Mutation Tighten Carapace");
                this.Tighten();
                The.Core.RenderBase(true, false);
                if (this.ParentObject.IsPlayer())
                {
                    Popup.Show("You tighten your carapace. Your AV increases by {{G|" + acmodifier.ToString() + "}}.");
                }
                else
                {
                    base.DidX("tighten", this.ParentObject.its + " carapace", null, null, null, null, null, false, false, null, null, false, false, false, true, false, null);
                }
            }
            else if (E.ID == "BeginMove" && this.Tight && !E.HasFlag("Forced") && E.GetStringParameter("Type", null) != "Teleporting")
            {
                this.Loosen();
                if (this.ParentObject.IsPlayer())
                {
                    Popup.Show("Your carapace loosens. Your AV decreases by {{R|" + this.ACModifier.ToString() + "}}.");
                }
                else
                {
                    IComponent<GameObject>.EmitMessage(this.ParentObject, Grammar.MakePossessive(this.ParentObject.The + this.ParentObject.ShortDisplayName) + " carapace loosens.", ' ', false, false, false);
                }
            }
            return base.FireEvent(E);
        }

        public void Tighten()
        {
            if (!this.Tight)
            {
                this.Tight = true;
                this.TightFactor = this.ACModifier;
                this.ParentObject.Statistics["AV"].Bonus += this.TightFactor;
                this.ParentObject.Statistics["DV"].Penalty += 2;
            }
        }

        public void Loosen()
        {
            if (this.Tight)
            {
                this.ParentObject.Statistics["AV"].Bonus -= this.TightFactor;
                this.ParentObject.Statistics["DV"].Penalty -= 2;
                this.Tight = false;
                this.TightFactor = 0;
            }
        }

        public override bool ChangeLevel(int NewLevel)
        {
            this.Loosen();
            this.ACModifier = 3 + (int)(base.Level / 2);
            this.DVModifier = -2;
            if (this.ResistanceMod > 0)
            {
                if (this.ParentObject.HasStat("HeatResistance"))
                {
                    this.ParentObject.GetStat("HeatResistance").Bonus -= this.ResistanceMod;
                }
                if (this.ParentObject.HasStat("ColdResistance"))
                {
                    this.ParentObject.GetStat("ColdResistance").Bonus -= this.ResistanceMod;
                }
                this.ResistanceMod = 0;
            }
            this.ResistanceMod = 5 + 5 * base.Level;
            if (this.ParentObject.HasStat("HeatResistance"))
            {
                this.ParentObject.GetStat("HeatResistance").Bonus += this.ResistanceMod;
            }
            if (this.ParentObject.HasStat("ColdResistance"))
            {
                this.ParentObject.GetStat("ColdResistance").Bonus += this.ResistanceMod;
            }
            foreach (BodyPart torso in this.ParentObject.Body.GetPart("Body"))
            {
                if (torso.Equipped != null)
                {
                    Armor part = torso.Equipped.GetPart<Armor>();
                    part.AV = this.ACModifier;
                    part.DV = this.DVModifier;
                }
            }
            return base.ChangeLevel(NewLevel);
        }

        public override void OnRegenerateDefaultEquipment(Body body)
        {
            foreach (BodyPart torso in body.GetPart("Body"))
            {
                this.AddCarapaceTo(torso);
            }
        }

        public void AddCarapaceTo(BodyPart body)
        {
            if (body == null)
            {
                return;
            }
            GameObject theCarapace = GameObjectFactory.Factory.CreateObject("Carapace");
            if (body.Equipped != null && body.Equipped.Blueprint == "Carapace")
            {
                return;
            }
            body.ForceUnequip(true);
            body.ParentBody.ParentObject.ForceEquipObject(theCarapace, body, true, new int?(0));
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            Body body = GO.Body;
            if (body != null)
            {
                foreach (BodyPart torso in body.GetPart("Body"))
                {
                    this.AddCarapaceTo(torso);
                }
                this.ActivatedAbilityID = base.AddMyActivatedAbility("Tighten " + this.DisplayName, "CommandTightenCarapace", "Physical Mutation", null, "�", null, false, false, false, false, false, false, false, false, true, true, false, -1, null, null, null, null, null, null);
            }
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            this.Loosen();
            if (this.ResistanceMod > 0)
            {
                if (this.ParentObject.HasStat("HeatResistance"))
                {
                    this.ParentObject.GetStat("HeatResistance").Bonus -= this.ResistanceMod;
                }
                if (this.ParentObject.HasStat("ColdResistance"))
                {
                    this.ParentObject.GetStat("ColdResistance").Bonus -= this.ResistanceMod;
                }
                this.ResistanceMod = 0;
            }
            foreach (BodyPart torso in GO.Body.GetPart("Body"))
            {
                GameObject carapaceToRemove = torso.Equipped;
                base.CleanUpMutationEquipment(GO, ref carapaceToRemove);
            }
            base.RemoveMyActivatedAbility(ref this.ActivatedAbilityID, null);
            return base.Unmutate(GO);
        }

        public int ACModifier;

        public int DVModifier;

        public int ResistanceMod;

        public bool Tight;

        public int TightFactor;
    }
}
