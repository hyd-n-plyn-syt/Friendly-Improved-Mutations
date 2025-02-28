using System;
using System.Text;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts.Mutation
{
	[Serializable]
	public class Improved_Wings : BaseDefaultEquipmentMutation, IFlightSource
	{
		public int FlightLevel
		{
			get
			{
				return base.Level;
			}
		}

		public int FlightBaseFallChance
		{
			get
			{
				return this.BaseFallChance;
			}
		}

		public bool FlightRequiresOngoingEffort
		{
			get
			{
				return true;
			}
		}

		public string FlightEvent
		{
			get
			{
				return Wings.COMMAND_NAME;
			}
		}

		public string FlightActivatedAbilityClass
		{
			get
			{
				return "Physical Mutation";
			}
		}

		public string FlightSourceDescription
		{
			get
			{
				return null;
			}
		}

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

		public string ManagerID
		{
			get
			{
				return this.ParentObject.ID + "::Wings";
			}
		}

		public string BlueprintName
		{
			get
			{
				return this.Variant.Coalesce("Wings");
			}
		}

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

		public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
		{
			Improved_Wings wings = base.DeepCopy(Parent, MapInv) as Improved_Wings;
			wings.WingsObject = null;
			return wings;
		}

		public Improved_Wings()
		{
			this.DisplayName = "Wings";
		}

		public override bool GeneratesEquipment()
		{
			return true;
		}

		public override bool WantEvent(int ID, int cascade)
		{
			return base.WantEvent(ID, cascade) || ID == AIGetOffensiveAbilityListEvent.ID || ID == AIGetPassiveAbilityListEvent.ID || ID == PooledEvent<AttemptToLandEvent>.ID || ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID || ID == PooledEvent<BodyPositionChangedEvent>.ID || ID == PooledEvent<CommandEvent>.ID || ID == EffectAppliedEvent.ID || ID == EffectRemovedEvent.ID || ID == SingletonEvent<EndTurnEvent>.ID || ID == EnteredCellEvent.ID || ID == GetItemElementsEvent.ID || ID == PooledEvent<GetJumpingBehaviorEvent>.ID || ID == GetLostChanceEvent.ID || ID == GetMovementCapabilitiesEvent.ID || ID == PooledEvent<MovementModeChangedEvent>.ID || ID == ObjectStoppedFlyingEvent.ID || ID == ReplicaCreatedEvent.ID || ID == PooledEvent<TransparentToEMPEvent>.ID || ID == TravelSpeedEvent.ID;
		}

		public override void CollectStats(Templates.StatCollector stats, int Level)
		{
			stats.Set("CrashChance", Flight.GetMoveFallChance(this.ParentObject, this), false, 0);
			stats.Set("SwoopCrashChance", Flight.GetSwoopFallChance(this.ParentObject, this), false, 0);
		}

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

		public override bool HandleEvent(GetMovementCapabilitiesEvent E)
		{
			if (E.Actor == this.ParentObject)
			{
				ActivatedAbilityEntry activatedAbilityEntry = base.MyActivatedAbility(this.FlightActivatedAbilityID, null);
				E.Add(activatedAbilityEntry.DisplayName, this.FlightEvent, 19000, activatedAbilityEntry, false);
			}
			return base.HandleEvent(E);
		}

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

		public override bool HandleEvent(ObjectStoppedFlyingEvent E)
		{
			Acrobatics_Jump.SyncAbility(this.ParentObject, false);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
		{
			if (!this.FlightFlying && E.Actor == this.ParentObject && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor) && (!E.Actor.IsEMPed() || !MutationsSubjectToEMPEvent.Check(E.Actor)))
			{
				E.Add(this.FlightEvent, 1, null, false, false, null, null);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
		{
			if (!this.FlightFlying && Flight.EnvironmentAllowsFlight(E.Actor) && Flight.IsAbilityAIUsable(this, E.Actor) && (!E.Actor.IsEMPed() || !MutationsSubjectToEMPEvent.Check(E.Actor)))
			{
				E.Add(this.FlightEvent, 1, null, false, false, null, null);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(TransparentToEMPEvent E)
		{
			return !MutationsSubjectToEMPEvent.Check(this.ParentObject) && base.HandleEvent(E);
		}

		public override bool HandleEvent(EffectAppliedEvent E)
		{
			this.CheckEMP();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EffectRemovedEvent E)
		{
			this.CheckEMP();
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetLostChanceEvent E)
		{
			E.PercentageBonus += 36 + 4 * base.Level;
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(ReplicaCreatedEvent E)
		{
			if (E.Object == this.ParentObject)
			{
				Flight.SyncFlying(this.ParentObject, this.ParentObject, this);
			}
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(TravelSpeedEvent E)
		{
			E.PercentageBonus += 50 + 50 * base.Level;
			return base.HandleEvent(E);
		}

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

		public override bool HandleEvent(AttemptToLandEvent E)
		{
			return (!this.FlightFlying || !Flight.StopFlying(this.ParentObject, this.ParentObject, this, false, false)) && base.HandleEvent(E);
		}

		public override bool HandleEvent(EndTurnEvent E)
		{
			this.CheckEMP();
			Flight.MaintainFlight(this.ParentObject, this.ParentObject, this);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(EnteredCellEvent E)
		{
			this.CheckEMP();
			Flight.CheckFlight(this.ParentObject, this.ParentObject, this);
			return base.HandleEvent(E);
		}

		public override bool HandleEvent(GetItemElementsEvent E)
		{
			if (E.IsRelevantCreature(this.ParentObject))
			{
				E.Add("travel", 1);
			}
			return base.HandleEvent(E);
		}

		public override string GetDescription()
		{
			return this.Blueprint.GetTag("VariantDescription", "").Coalesce("You fly.");
		}

		public float SprintingMoveSpeedBonus(int Level)
		{
			return 0.1f + 0.1f * (float)Level;
		}

		public int GetJumpDistanceBonus(int Level)
		{
			return 1 + Level / 3;
		}

		public int GetChargeDistanceBonus(int Level)
		{
			return 2 + Level / 3;
		}

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

		public override void OnRegenerateDefaultEquipment(Body body)
		{
            foreach (BodyPart currentBack in body.GetPart(this.BodyPartType))
            {
                currentBack.Description = this.Blueprint.GetTag("PartDescription", "Worn around Wings");
                currentBack.DescriptionPrefix = null;
                currentBack.DefaultBehavior = GameObject.Create(this.Blueprint, 0, 0, null, null, null, null, null);
            }
		}

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

		public override void SetVariant(string Variant)
		{
			base.SetVariant(Variant);
			this._Blueprint = null;
		}

		public override bool Mutate(GameObject GO, int Level)
		{
			Flight.AbilitySetup(GO, GO, this);
			Acrobatics_Jump.SyncAbility(GO, true);
			GO.WantToReequip();
			return base.Mutate(GO, Level);
		}

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

		public void CheckEMP()
		{
			if (this.FlightFlying && this.ParentObject.IsEMPed() && MutationsSubjectToEMPEvent.Check(this.ParentObject))
			{
				Flight.FailFlying(this.ParentObject, this.ParentObject, this);
			}
		}

		public static readonly string COMMAND_NAME = "CommandFlight";

		public GameObject WingsObject;

		public string BodyPartType = "Back";

		public int BaseFallChance = 6;

		public bool _FlightFlying;

		public Guid _FlightActivatedAbilityID = Guid.Empty;

		[NonSerialized]
		protected GameObjectBlueprint _Blueprint;

		public int appliedChargeBonus;

		public int appliedJumpBonus;
	}
}
