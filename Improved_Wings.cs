using System;
using System.Text;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts.Mutation
{
	// Token: 0x020011BB RID: 4539
	[Serializable]
	public class Improved_Wings : BaseDefaultEquipmentMutation, IFlightSource
	{
		// Token: 0x17000AC9 RID: 2761
		// (get) Token: 0x0600B2AD RID: 45741 RVA: 0x0041A1E6 File Offset: 0x004183E6
		public int FlightLevel
		{
			get
			{
				return base.Level;
			}
		}

		// Token: 0x17000ACA RID: 2762
		// (get) Token: 0x0600B2AE RID: 45742 RVA: 0x0041A1EE File Offset: 0x004183EE
		public int FlightBaseFallChance
		{
			get
			{
				return this.BaseFallChance;
			}
		}

		// Token: 0x17000ACB RID: 2763
		// (get) Token: 0x0600B2AF RID: 45743 RVA: 0x0041A1F6 File Offset: 0x004183F6
		public bool FlightRequiresOngoingEffort
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000ACC RID: 2764
		// (get) Token: 0x0600B2B0 RID: 45744 RVA: 0x0041A1F9 File Offset: 0x004183F9
		public string FlightEvent
		{
			get
			{
				return Wings.COMMAND_NAME;
			}
		}

		// Token: 0x17000ACD RID: 2765
		// (get) Token: 0x0600B2B1 RID: 45745 RVA: 0x0041A200 File Offset: 0x00418400
		public string FlightActivatedAbilityClass
		{
			get
			{
				return "Physical Mutation";
			}
		}

		// Token: 0x17000ACE RID: 2766
		// (get) Token: 0x0600B2B2 RID: 45746 RVA: 0x0041A207 File Offset: 0x00418407
		public string FlightSourceDescription
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000ACF RID: 2767
		// (get) Token: 0x0600B2B3 RID: 45747 RVA: 0x0041A20A File Offset: 0x0041840A
		// (set) Token: 0x0600B2B4 RID: 45748 RVA: 0x0041A212 File Offset: 0x00418412
		public bool FlightFlying
		{
			get
			{
				return this._FlightFlying;
			}
			set
			{
				this._FlightFlying = value;
			}
		}

		// Token: 0x17000AD0 RID: 2768
		// (get) Token: 0x0600B2B5 RID: 45749 RVA: 0x0041A21B File Offset: 0x0041841B
		// (set) Token: 0x0600B2B6 RID: 45750 RVA: 0x0041A223 File Offset: 0x00418423
		public Guid FlightActivatedAbilityID
		{
			get
			{
				return this._FlightActivatedAbilityID;
			}
			set
			{
				this._FlightActivatedAbilityID = value;
			}
		}

		// Token: 0x17000AD1 RID: 2769
		// (get) Token: 0x0600B2B7 RID: 45751 RVA: 0x0041A22C File Offset: 0x0041842C
		public string ManagerID
		{
			get
			{
				return this.ParentObject.ID + "::Wings";
			}
		}

		// Token: 0x17000AD2 RID: 2770
		// (get) Token: 0x0600B2B8 RID: 45752 RVA: 0x0041A243 File Offset: 0x00418443
		public string BlueprintName
		{
			get
			{
				return this.Variant.Coalesce("Wings");
			}
		}

		// Token: 0x17000AD3 RID: 2771
		// (get) Token: 0x0600B2B9 RID: 45753 RVA: 0x0041A258 File Offset: 0x00418458
		public GameObjectBlueprint Blueprint
		{
			get
			{
				GameObjectBlueprint result;
				if ((result = this._Blueprint) == null)
				{
					result = (this._Blueprint = GameObjectFactory.Factory.GetBlueprint(this.BlueprintName));
				}
				return result;
			}
		}

		// Token: 0x0600B2BA RID: 45754 RVA: 0x0041A288 File Offset: 0x00418488
		public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
		{
			Wings wings = base.DeepCopy(Parent, MapInv) as Wings;
			wings.WingsObject = null;
			return wings;
		}

		// Token: 0x0600B2BB RID: 45755 RVA: 0x0041A29E File Offset: 0x0041849E
		public Improved_Wings()
		{
			this.DisplayName = "Wings";
		}

		// Token: 0x0600B2BC RID: 45756 RVA: 0x0041A2CE File Offset: 0x004184CE
		public override bool GeneratesEquipment()
		{
			return true;
		}

		// Token: 0x0600B2BD RID: 45757 RVA: 0x0041A2D4 File Offset: 0x004184D4
		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == AIGetOffensiveAbilityListEvent.ID || ID == AIGetPassiveAbilityListEvent.ID || ID == PooledEvent<AttemptToLandEvent>.ID || ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID || ID == PooledEvent<BodyPositionChangedEvent>.ID || ID == PooledEvent<CommandEvent>.ID || ID == EffectAppliedEvent.ID || ID == EffectRemovedEvent.ID || ID == SingletonEvent<EndTurnEvent>.ID || ID == EnteredCellEvent.ID || ID == GetItemElementsEvent.ID || ID == PooledEvent<GetJumpingBehaviorEvent>.ID || ID == GetLostChanceEvent.ID || ID == GetMovementCapabilitiesEvent.ID || ID == PooledEvent<MovementModeChangedEvent>.ID || ID == ObjectStoppedFlyingEvent.ID || ID == ReplicaCreatedEvent.ID || ID == PooledEvent<TransparentToEMPEvent>.ID || ID == TravelSpeedEvent.ID;
		}

		// Token: 0x0600B2BE RID: 45758 RVA: 0x0041A391 File Offset: 0x00418591
		public override void CollectStats(Templates.StatCollector stats, int Level)
		{
			stats.Set("CrashChance", Flight.GetMoveFallChance(this.ParentObject, this), false, 0);
			stats.Set("SwoopCrashChance", Flight.GetSwoopFallChance(this.ParentObject, this), false, 0);
		}

		// Token: 0x0600B2BF RID: 45759 RVA: 0x0041A3C8 File Offset: 0x004185C8
		public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
		{
			base.DescribeMyActivatedAbility(this.FlightActivatedAbilityID, new Action<Templates.StatCollector>(this.CollectStats), this.ParentObject);
			GameObject parentObject = this.ParentObject;
			ActivatedAbilityEntry activatedAbilityEntry = (parentObject != null) ? parentObject.GetActivatedAbilityByCommand(Flight.SWOOP_ATTACK_COMMAND_NAME) : null;
			if (activatedAbilityEntry != null)
			{
				base.DescribeMyActivatedAbility(activatedAbilityEntry.ID, new Action<Templates.StatCollector>(this.CollectStats), this.ParentObject);
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C0 RID: 45760 RVA: 0x0041A438 File Offset: 0x00418638
		public override bool HandleEvent(GetMovementCapabilitiesEvent E)
		{
			if (E.Actor == this.ParentObject)
			{
				ActivatedAbilityEntry activatedAbilityEntry = base.MyActivatedAbility(this.FlightActivatedAbilityID, null);
				E.Add(activatedAbilityEntry.DisplayName, this.FlightEvent, 19000, activatedAbilityEntry, false);
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C1 RID: 45761 RVA: 0x0041A484 File Offset: 0x00418684
		public override bool HandleEvent(CommandEvent E)
		{
			if (E.Command == Wings.COMMAND_NAME)
			{
				if (base.IsMyActivatedAbilityToggledOn(this.FlightActivatedAbilityID, null))
				{
					if (this.ParentObject.IsPlayer() && base.currentCell != null && this.ParentObject.GetEffectCount(typeof(Flying)) <= 1)
					{
						int i = 0;
						int count = base.currentCell.Objects.Count;
						while (i < count)
						{
							GameObject gameObject = base.currentCell.Objects[i];
							StairsDown part = gameObject.GetPart<StairsDown>();
							if (part != null && part.IsLongFall() && Popup.ShowYesNo("It looks like a long way down " + gameObject.t(2147483647, null, null, false, false, false, false, false, true, true, false, null, false, true, false, null, true, null, false) + " you're above. Are you sure you want to stop flying?", null, true, DialogResult.Yes) != DialogResult.Yes)
							{
								return false;
							}
							i++;
						}
					}
					Flight.StopFlying(this.ParentObject, this.ParentObject, this, false, false);
				}
				else
				{
					if (this.ParentObject.IsEMPed() && MutationsSubjectToEMPEvent.Check(this.ParentObject))
					{
						return this.ParentObject.Fail(this.ParentObject.Poss(this.Blueprint.CachedDisplayNameStripped, true) + " will not move!");
					}
					Flight.StartFlying(this.ParentObject, this.ParentObject, this);
				}
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C2 RID: 45762 RVA: 0x0041A5E8 File Offset: 0x004187E8
		public override bool HandleEvent(GetJumpingBehaviorEvent E)
        {
            if (E.AbilityName == "Jump" && !E.CanJumpOverCreatures && base.IsMyActivatedAbilityUsable(this.FlightActivatedAbilityID, null) && !this.ParentObject.HasEffect<Grounded>())
            {
                E.CanJumpOverCreatures = true;
                Templates.StatCollector stats = E.Stats;
                if (stats != null)
                {
                    stats.Set("CanJumpOverCreatures", "true", false, 0);
                }
                Templates.StatCollector stats2 = E.Stats;
                if (stats2 != null)
                {
                    stats2.AddLinearBonusModifier("Range", this.appliedJumpBonus, "wings");
                }
            }
            return base.HandleEvent(E);
        }

		// Token: 0x0600B2C3 RID: 45763 RVA: 0x0041A644 File Offset: 0x00418844
		public override bool HandleEvent(ObjectStoppedFlyingEvent E)
		{
			Acrobatics_Jump.SyncAbility(this.ParentObject, false);
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C4 RID: 45764 RVA: 0x0041A65C File Offset: 0x0041885C
		public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
		{
			if (!this.FlightFlying && E.Actor == this.ParentObject && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor) && (!E.Actor.IsEMPed() || !MutationsSubjectToEMPEvent.Check(E.Actor)))
			{
				E.Add(this.FlightEvent, 1, null, false, false, null, null);
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C5 RID: 45765 RVA: 0x0041A6D0 File Offset: 0x004188D0
		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (!this.FlightFlying && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor) && (!E.Actor.IsEMPed() || !MutationsSubjectToEMPEvent.Check(E.Actor)))
			{
				E.Add(this.FlightEvent, 1, null, false, false, null, null);
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C6 RID: 45766 RVA: 0x0041A733 File Offset: 0x00418933
		public override bool HandleEvent(TransparentToEMPEvent E)
		{
			return !MutationsSubjectToEMPEvent.Check(this.ParentObject) && base.HandleEvent(E);
		}

		// Token: 0x0600B2C7 RID: 45767 RVA: 0x0041A74B File Offset: 0x0041894B
		public override bool HandleEvent(EffectAppliedEvent E)
		{
			this.CheckEMP();
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C8 RID: 45768 RVA: 0x0041A75A File Offset: 0x0041895A
		public override bool HandleEvent(EffectRemovedEvent E)
		{
			this.CheckEMP();
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2C9 RID: 45769 RVA: 0x0041A769 File Offset: 0x00418969
		public override bool HandleEvent(GetLostChanceEvent E)
		{
			E.PercentageBonus += 36 + 4 * base.Level;
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2CA RID: 45770 RVA: 0x0041A78A File Offset: 0x0041898A
		public override bool HandleEvent(ReplicaCreatedEvent E)
		{
			if (E.Object == this.ParentObject)
			{
				Flight.SyncFlying(this.ParentObject, this.ParentObject, this);
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2CB RID: 45771 RVA: 0x0041A7B3 File Offset: 0x004189B3
		public override bool HandleEvent(TravelSpeedEvent E)
		{
			E.PercentageBonus += 50 + 50 * base.Level;
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2CC RID: 45772 RVA: 0x0041A7D8 File Offset: 0x004189D8
		public override bool HandleEvent(BodyPositionChangedEvent E)
		{
			if (this.FlightFlying && E.To != "Flying")
			{
				if (E.Involuntary)
				{
					Flight.FailFlying(this.ParentObject, this.ParentObject, this);
				}
				else
				{
					Flight.StopFlying(this.ParentObject, this.ParentObject, this, false, false);
				}
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2CD RID: 45773 RVA: 0x0041A838 File Offset: 0x00418A38
		public override bool HandleEvent(MovementModeChangedEvent E)
		{
			if (this.FlightFlying && E.To != "Flying")
			{
				if (E.Involuntary)
				{
					Flight.FailFlying(this.ParentObject, this.ParentObject, this);
				}
				else
				{
					Flight.StopFlying(this.ParentObject, this.ParentObject, this, false, false);
				}
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2CE RID: 45774 RVA: 0x0041A898 File Offset: 0x00418A98
		public override bool HandleEvent(AttemptToLandEvent E)
		{
			return (!this.FlightFlying || !Flight.StopFlying(this.ParentObject, this.ParentObject, this, false, false)) && base.HandleEvent(E);
		}

		// Token: 0x0600B2CF RID: 45775 RVA: 0x0041A8C1 File Offset: 0x00418AC1
		public override bool HandleEvent(EndTurnEvent E)
		{
			this.CheckEMP();
			Flight.MaintainFlight(this.ParentObject, this.ParentObject, this);
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2D0 RID: 45776 RVA: 0x0041A8E3 File Offset: 0x00418AE3
		public override bool HandleEvent(EnteredCellEvent E)
		{
			this.CheckEMP();
			Flight.CheckFlight(this.ParentObject, this.ParentObject, this);
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2D1 RID: 45777 RVA: 0x0041A905 File Offset: 0x00418B05
		public override bool HandleEvent(GetItemElementsEvent E)
		{
			if (E.IsRelevantCreature(this.ParentObject))
			{
				E.Add("travel", 1);
			}
			return base.HandleEvent(E);
		}

		// Token: 0x0600B2D2 RID: 45778 RVA: 0x0041A928 File Offset: 0x00418B28
		public override string GetDescription()
		{
			return this.Blueprint.GetTag("VariantDescription", "").Coalesce("You fly.");
		}

		// Token: 0x0600B2D3 RID: 45779 RVA: 0x0041A949 File Offset: 0x00418B49
		public float SprintingMoveSpeedBonus(int Level)
		{
			return 0.1f + 0.1f * (float)Level;
		}

		// Token: 0x0600B2D4 RID: 45780 RVA: 0x0041A959 File Offset: 0x00418B59
		public int GetJumpDistanceBonus(int Level)
		{
			return 1 + Level / 3;
		}

		// Token: 0x0600B2D5 RID: 45781 RVA: 0x0041A960 File Offset: 0x00418B60
		public int GetChargeDistanceBonus(int Level)
		{
			return 2 + Level / 3;
		}

		// Token: 0x0600B2D6 RID: 45782 RVA: 0x0041A968 File Offset: 0x00418B68
		public override string GetLevelText(int Level)
		{
			int num = Math.Max(0, this.FlightBaseFallChance - Level);
			StringBuilder stringBuilder = Event.NewStringBuilder("");
			stringBuilder.Append("You travel on the world map at {{rules|").Append(1.5 + 0.5 * (double)Level).Append("x}} speed.\n");
			stringBuilder.Append("{{rules|" + (36 + Level * 4).ToString()).Append("%}} reduced chance of becoming lost\n");
			stringBuilder.Append("While outside, you may fly. You cannot be hit in melee by grounded creatures while flying.\n");
			stringBuilder.Append("{{rules|" + num.ToString()).Append("%}} chance of falling clumsily to the ground\n");
			stringBuilder.Append("{{rules|" + ((int)(this.SprintingMoveSpeedBonus(Level) * 100f)).Signed(false) + "%}} move speed while sprinting\n");
			stringBuilder.Append("You can jump {{rules|" + this.GetJumpDistanceBonus(Level).ToString() + ((this.GetJumpDistanceBonus(Level) == 1) ? "}} square" : "}} squares") + " farther.\n");
			stringBuilder.Append("You can charge {{rules|" + this.GetChargeDistanceBonus(Level).ToString() + ((this.GetChargeDistanceBonus(Level) == 1) ? "}} square" : "}} squares") + " farther.\n");
			stringBuilder.Append("+300 reputation with {{w|birds}} and {{w|winged mammals}}");
			return stringBuilder.ToString();
		}

		// Token: 0x0600B2D7 RID: 45783 RVA: 0x0041AAC4 File Offset: 0x00418CC4
		public override void OnRegenerateDefaultEquipment(Body body)
		{
			/*string partParameter = this.Blueprint.GetPartParameter<string>("Armor", "WornOn", "Back");
			BodyPart bodyPart;
			if (!base.TryGetRegisteredSlot(body, partParameter, out bodyPart, false))
			{
				bodyPart = (body.GetFirstPart(partParameter) ?? this.AddBodyPart(body));
				if (bodyPart != null)
				{
					base.RegisterSlot(partParameter, bodyPart);
				}
			}*/
            foreach (BodyPart currentBack in body.GetPart(this.BodyPartType))
            {
                currentBack.Description = this.Blueprint.GetTag("PartDescription", "Worn around Wings");
                currentBack.DescriptionPrefix = null;
                currentBack.DefaultBehavior = GameObject.Create(this.Blueprint, 0, 0, null, null, null, null, null);
            }
		}

		// Token: 0x0600B2D8 RID: 45784 RVA: 0x0041AB58 File Offset: 0x00418D58
		public BodyPart AddBodyPart(Body Body)
		{
			BodyPart body = Body.GetBody();
			string partParameter = this.Blueprint.GetPartParameter<string>("Armor", "WornOn", "Back");
			BodyPart bodyPart = body;
			string @base = partParameter;
			int laterality = 0;
			string defaultBehavior = null;
			string supportsDependent = null;
			string dependsOn = null;
			string requiresType = null;
			int? num = new int?(body.Category);
			string managerID = this.ManagerID;
			int? category = num;
			string tag = this.Blueprint.GetTag("InsertPartAfter", "Head");
			string[] orInsertBefore = new string[]
			{
				"Arm",
				"Missile Weapon",
				"Hands"
			};
			return bodyPart.AddPartAt(@base, laterality, defaultBehavior, supportsDependent, dependsOn, requiresType, managerID, category, null, null, null, null, null, null, null, null, null, null, null, null, tag, orInsertBefore, true);
		}

		// Token: 0x0600B2D9 RID: 45785 RVA: 0x0041AC5C File Offset: 0x00418E5C
		public override bool ChangeLevel(int NewLevel)
		{
			if (this.appliedChargeBonus > 0)
			{
				this.ParentObject.ModIntProperty("ChargeRangeModifier", -this.appliedChargeBonus, false);
			}
			if (this.appliedJumpBonus > 0)
			{
				this.ParentObject.ModIntProperty("JumpRangeModifier", -this.appliedJumpBonus, false);
			}
			this.appliedChargeBonus = this.GetChargeDistanceBonus(NewLevel);
			this.appliedJumpBonus = this.GetJumpDistanceBonus(NewLevel);
			this.ParentObject.ModIntProperty("ChargeRangeModifier", this.appliedChargeBonus, false);
			this.ParentObject.ModIntProperty("JumpRangeModifier", this.appliedJumpBonus, false);
			Acrobatics_Jump.SyncAbility(this.ParentObject, false);
			return base.ChangeLevel(NewLevel);
		}

		// Token: 0x0600B2DA RID: 45786 RVA: 0x0041AD0A File Offset: 0x00418F0A
		public override void SetVariant(string Variant)
		{
			base.SetVariant(Variant);
			this._Blueprint = null;
		}

		// Token: 0x0600B2DB RID: 45787 RVA: 0x0041AD1A File Offset: 0x00418F1A
		public override bool Mutate(GameObject GO, int Level)
		{
			Flight.AbilitySetup(GO, GO, this);
			Acrobatics_Jump.SyncAbility(GO, true);
			GO.WantToReequip();
			return base.Mutate(GO, Level);
		}

		// Token: 0x0600B2DC RID: 45788 RVA: 0x0041AD3C File Offset: 0x00418F3C
		public override bool Unmutate(GameObject GO)
		{
			if (this.FlightFlying)
			{
				Flight.FailFlying(GO, GO, this);
			}
			GO.ModIntProperty("ChargeRangeModifier", -this.appliedChargeBonus, true);
			GO.ModIntProperty("JumpRangeModifier", -this.appliedJumpBonus, true);
			GO.RemoveBodyPartsByManager(this.ManagerID, true);
			this.appliedChargeBonus = 0;
			this.appliedJumpBonus = 0;
            foreach (BodyPart back in ParentObject.Body.GetPart("Back"))
            {
                GameObject wingsToDestroy = back.DefaultBehavior;
                base.CleanUpMutationEquipment(GO, ref wingsToDestroy);
                back.Description = "Worn on Back";
            }
			Flight.AbilityTeardown(GO, GO, this);
			Acrobatics_Jump.SyncAbility(GO, false);
			GO.WantToReequip();
			return base.Unmutate(GO);
		}

		// Token: 0x0600B2DD RID: 45789 RVA: 0x0041ADBB File Offset: 0x00418FBB
		public void CheckEMP()
		{
			if (this.FlightFlying && this.ParentObject.IsEMPed() && MutationsSubjectToEMPEvent.Check(this.ParentObject))
			{
				Flight.FailFlying(this.ParentObject, this.ParentObject, this);
			}
		}

		// Token: 0x0400478C RID: 18316
		public static readonly string COMMAND_NAME = "CommandFlight";

		// Token: 0x0400478D RID: 18317
		public GameObject WingsObject;

		// Token: 0x0400478E RID: 18318
		public string BodyPartType = "Back";

		// Token: 0x0400478F RID: 18319
		public int BaseFallChance = 6;

		// Token: 0x04004790 RID: 18320
		public bool _FlightFlying;

		// Token: 0x04004791 RID: 18321
		public Guid _FlightActivatedAbilityID = Guid.Empty;

		// Token: 0x04004792 RID: 18322
		[NonSerialized]
		protected GameObjectBlueprint _Blueprint;

		// Token: 0x04004793 RID: 18323
		public int appliedChargeBonus;

		// Token: 0x04004794 RID: 18324
		public int appliedJumpBonus;
	}
}
