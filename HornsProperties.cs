using System;
using System.Text;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    // Token: 0x02000EC1 RID: 3777
    [Serializable]
    public class HornsProperties : IPart
    {
        // Token: 0x060098BB RID: 39099 RVA: 0x0039D077 File Offset: 0x0039B277
        public override bool SameAs(IPart p)
        {
            return (p as HornsProperties).HornLevel == this.HornLevel && base.SameAs(p);
        }

        // Token: 0x060098BC RID: 39100 RVA: 0x0039D095 File Offset: 0x0039B295
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade) || ID == GetDebugInternalsEvent.ID || ID == GetToHitModifierEvent.ID || ID == GetShortDescriptionEvent.ID || ID == GetMeleeAttackChanceEvent.ID;
        }

        // Token: 0x060098BD RID: 39101 RVA: 0x0039D0C3 File Offset: 0x0039B2C3
        public override bool HandleEvent(GetToHitModifierEvent E)
        {
            if (E.Weapon == this.ParentObject && E.Checking == "Actor")
            {
                E.Modifier += this.GetToHitBonus();
            }
            return base.HandleEvent(E);
        }

        // Token: 0x060098BE RID: 39102 RVA: 0x0039D100 File Offset: 0x0039B300
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

        // Token: 0x060098BF RID: 39103 RVA: 0x0039D164 File Offset: 0x0039B364
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

        // Token: 0x060098C0 RID: 39104 RVA: 0x0039D1B9 File Offset: 0x0039B3B9
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, "HornLevel", this.HornLevel);
            return base.HandleEvent(E);
        }

        // Token: 0x060098C1 RID: 39105 RVA: 0x0039D1D4 File Offset: 0x0039B3D4
        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "WeaponDealDamage");
            base.Register(Object);
        }

        // Token: 0x060098C2 RID: 39106 RVA: 0x0039D1EC File Offset: 0x0039B3EC
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

        // Token: 0x060098C3 RID: 39107 RVA: 0x0039D25C File Offset: 0x0039B45C
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

        // Token: 0x060098C4 RID: 39108 RVA: 0x0039D2A4 File Offset: 0x0039B4A4
        public int GetToHitBonus()
        {
            return this.GetHornLevel() / 2 + 1;
        }

        // Token: 0x060098C5 RID: 39109 RVA: 0x0039D2B0 File Offset: 0x0039B4B0
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

        // Token: 0x04004039 RID: 16441
        public int HornLevel;
    }
}