using System;
using System.Text;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class Spinnerets : BaseMutation
    {
        public const string SAVE_BONUS_VS = "Move";

        public bool Phase;

        public Spinnerets()
        {
            this.DisplayName = "Spinnerets";
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == AIGetOffensiveAbilityListEvent.ID || ID == ModifyDefendingSaveEvent.ID;
        }

        public override bool HandleEvent(ModifyDefendingSaveEvent E)
        {
            if (SavingThrows.Applicable(SAVE_BONUS_VS, E))
            {
                E.Roll += this.GetMoveSaveModifier();
            }
            return base.HandleEvent(E);
        }
        
        public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
        {
            if (E.Distance > 1 && base.IsMyActivatedAbilityAIUsable(this.ActivatedAbilityID, null))
            {
                E.Add("CommandShootWeb", 1, null, false, false, null, null);
            }
            return base.HandleEvent(E);
        }

        public override bool AllowStaticRegistration()
        {
            return false;
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "CommandShootWeb");
            Object.RegisterPartEvent(this, "ApplyStuck");
            base.Register(Object);
        }

        public override string GetDescription()
        {
            return "You can shoot sticky silk webs to hinder enemy movement.";
        }

        public int GetMoveSaveModifier()
        {
            return 5 + base.Level;
        }

        // These next two methods were stolen from the BreatherBase class
        public int GetConeLength(int L = -1)
        {
            if (L == -1)
            {
                return 4 + base.Level;
            }
            return 4 + L;
        }

        public int GetConeAngle(int L = -1)
        {
            if (L == -1)
            {
                return 20 + 2 * base.Level;
            }
            return 20 + 2 * L;
        }

        public override string GetLevelText(int Level)
        {
            StringBuilder stringBuilder = Event.NewStringBuilder("");
            stringBuilder.Compound("You can shoot a cone of spider webs in a set direction.", '\n');
            if (Level != base.Level)
            {
                stringBuilder.Compound("{{rules|Increased web strength}}", '\n');
            }
            stringBuilder.Compound("Duration: {{rules|", '\n').Append(5 + Level).Append("}} move actions");
            SavingThrows.AppendSaveBonusDescription(stringBuilder, this.GetMoveSaveModifier(), SAVE_BONUS_VS, true, false);
            stringBuilder.Compound("Cooldown: 80 rounds", '\n');
            stringBuilder.Compound("You don't get stuck in other creatures' webs.", '\n');
            stringBuilder.Compound("+300 reputation with {{w|arachnids}}", '\n');
            return stringBuilder.ToString();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ApplyStuck")
            {
                return false;
            }
            if (E.ID == "CommandShootWeb")
            {
                if (this.ParentObject != null)
                {
                    this.ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_physical_generic_activate", 0.5f, 0f, false, 0f);
                }
                if (this.ParentObject.OnWorldMap())
                {
                    if (this.ParentObject.IsPlayer())
                    {
                        Popup.ShowFail("You cannot do that on the world map.", true, true, true);
                    }
                    return false;
                }
                if (ShootWebs())
                {
                    base.CooldownMyActivatedAbility(this.ActivatedAbilityID, 80, null, null);
                    base.UseEnergy(1000);
                }
                return true;
            }
            return base.FireEvent(E);
        }

        // This is the biggest change from base
        public bool ShootWebs()
        {
            String direction = base.PickDirectionS();
            Physics physics = this.ParentObject.pPhysics;
            Cell center = physics.CurrentCell;
            // This allows the player to pick where to send the webs
            List<Cell> targetCells = this.PickCone(this.GetConeLength(-1), this.GetConeAngle(-1), AllowVis.Any, null);
            if (targetCells == null)
            {
                return false;
            }

            // Add webs in each of the chosen cells
            foreach(Cell current in targetCells)
            {
                GameObject gameObject;
                if (!this.Phase && !this.ParentObject.HasEffect("Phased"))
                {
                    gameObject = GameObjectFactory.Factory.CreateObject("Web");
                    Sticky sticky = gameObject.GetPart<Sticky>();
                    sticky.SaveTarget = 15 + base.Level;
                    sticky.MaxWeight = 120 + 80 * base.Level;
                }
                else
                {
                    gameObject = GameObjectFactory.Factory.CreateObject("PhaseWeb");
                    gameObject.ApplyEffect(new Phased());
                    PhaseSticky phaseSticky = gameObject.GetPart<PhaseSticky>();
                    phaseSticky.SaveTarget = 25 + base.Level;
                    phaseSticky.MaxWeight = 520 + 80 * base.Level;
                }

                current.AddObject(gameObject);
            }

            return true;
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.ActivatedAbilityID = base.AddMyActivatedAbility("Shoot Webs", "CommandShootWeb", "Physical Mutation");
            if (this.Phase)
            {
                GO.ApplyEffect(new Phased(9999));
            }
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            base.RemoveMyActivatedAbility(ref this.ActivatedAbilityID, null);
            return base.Unmutate(GO);
        }
    }
}
