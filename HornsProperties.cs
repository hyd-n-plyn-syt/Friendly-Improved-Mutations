using System;
using System.Text;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    [Serializable]
    public class HornsProperties : IPart
    {
        public override bool SameAs(IPart p)
        {
            return (p as HornsProperties).HornLevel == this.HornLevel && base.SameAs(p);
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == GetDebugInternalsEvent.ID || ID == GetToHitModifierEvent.ID || ID == GetShortDescriptionEvent.ID || ID == GetMeleeAttackChanceEvent.ID;
        }

        public override bool HandleEvent(GetToHitModifierEvent E)
        {
            if (E.Weapon == this.ParentObject && E.Checking == "Actor")
            {
                E.Modifier += this.GetToHitBonus();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetShortDescriptionEvent E)
        {
            string value;
            int value2;
            this.GetBleedingPerformance(out value, out value2);
            StringBuilder stringBuilder = Event.NewStringBuilder("");
            stringBuilder.Append("On penetration, this weapon causes bleeding: ").Append(value).Append(" damage per round; save difficulty ").Append(value2).Append(".");
            E.Postfix.AppendRules(stringBuilder.ToString());
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetMeleeAttackChanceEvent E)
        {
            if (E.Intrinsic && !E.Primary && E.Weapon == this.ParentObject)
            {
                if (E.Properties.HasDelimitedSubstring(',', "Charging", StringComparison.Ordinal))
                {
                    E.SetFinalizedChance(100);
                    return false;
                }
                E.Chance = 20;
                return true;
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, "HornLevel", this.HornLevel);
            return base.HandleEvent(E);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "WeaponDealDamage");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "WeaponDealDamage" && E.GetIntParameter("Penetrations", 0) > 0)
            {
                GameObject gameObjectParameter = E.GetGameObjectParameter("Defender");
                if (gameObjectParameter != null)
                {
                    GameObject gameObjectParameter2 = E.GetGameObjectParameter("Attacker");
                    string damage;
                    int saveTarget;
                    this.GetBleedingPerformance(out damage, out saveTarget);
                    gameObjectParameter.ApplyEffect(new Bleeding(damage, saveTarget, gameObjectParameter2, true, false, false, false), null);
                }
            }
            return base.FireEvent(E);
        }

        public void GetBleedingPerformance(out string Damage, out int SaveTarget)
        {
            int hornLevel = this.GetHornLevel();
            Damage = "1";
            if (hornLevel > 4)
            {
                Damage = "1d2";
                int num = (hornLevel - 4) / 4;
                if (num > 0)
                {
                    Damage += num.Signed(false);
                }
            }
            SaveTarget = 20 + 2 * hornLevel;
        }

        public int GetToHitBonus()
        {
            return this.GetHornLevel() / 2 + 1;
        }

        public int GetHornLevel()
        {
            int result = 1;
            if (this.HornLevel != 0)
            {
                result = this.HornLevel;
            }
            else
            {
                GameObject parentObject = this.ParentObject;
                object obj;
                if (parentObject == null)
                {
                    obj = null;
                }
                else
                {
                    GameObject equipped = parentObject.Equipped;
                    obj = ((equipped != null) ? equipped.GetPart("Mutations") : null);
                }
                Mutations mutations = obj as Mutations;
                if (mutations != null)
                {
                    Horns horns = mutations.GetMutation("Horns") as Horns;
                    if (horns != null)
                    {
                        result = horns.Level;
                    }
                }
            }
            return result;
        }

        public int HornLevel;
    }
}