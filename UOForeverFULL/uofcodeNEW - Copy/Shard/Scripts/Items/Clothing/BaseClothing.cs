using System;
using System.Collections.Generic;
using Server;
using Server.Engines.Craft;
using Server.Factions;
using Server.Network;
using Server.Ethics;
using Server.Misc;
using Server.Engines.XmlSpawner2;

namespace Server.Items
{
	public enum ClothingQuality
	{
		Low,
		Regular,
		Exceptional
	}

	public interface IArcaneEquip
	{
		bool IsArcane{ get; }
		int CurArcaneCharges{ get; set; }
		int MaxArcaneCharges{ get; set; }
	}

	public abstract class BaseClothing : Item, IScissorable, IFactionItem, ICraftable, IWearableDurability, IEthicsItem
	{
		#region Ethics
		private EthicsItem m_EthicState;

		public EthicsItem EthicsItemState
		{
			get{ return m_EthicState; }
			set{ m_EthicState = value; }
		}
		#endregion

		#region Factions
		private FactionItem m_FactionState;

		public FactionItem FactionItemState
		{
			get{ return m_FactionState; }
			set{ m_FactionState = value; }
		}
		#endregion

		public override DeathMoveResult OnParentDeath( Mobile parent )
		{
			DeathMoveResult result = base.OnParentDeath( parent );
            Ethic parentState = Ethic.Find(parent);

            if (parentState != null && result == DeathMoveResult.MoveToCorpse && m_EthicState != null && parentState == m_EthicState.Ethic)
				return DeathMoveResult.MoveToBackpack;
			else
				return result;
		}

		public override DeathMoveResult OnInventoryDeath( Mobile parent )
		{
			DeathMoveResult result = base.OnParentDeath( parent );
            Ethic parentState = Ethic.Find(parent);

            if (parentState != null && result == DeathMoveResult.MoveToCorpse && m_EthicState != null && parentState == m_EthicState.Ethic)
				return DeathMoveResult.MoveToBackpack;
			else
				return result;
		}

		public virtual bool CanFortify{ get{ return true; } }

		private int m_MaxHitPoints;
		private int m_HitPoints;
		private Mobile m_Crafter;
		private ClothingQuality m_Quality;
		private bool m_PlayerConstructed;
		protected CraftResource m_Resource;
		private int m_StrReq = -1;
		private bool m_Identified;
        private bool m_NotScissorable; // Alan mod
/*
		[Hue, CommandProperty( AccessLevel.GameMaster )]
		public override int Hue
		{
			get{ return Identified ? base.Hue : ( Resource == CraftResource.BlackRock ? 1155 : 0 ); }
			set{ base.Hue = value; InvalidateProperties(); }
		}
*/

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxHitPoints
		{
			get{ return m_MaxHitPoints; }
			set{ m_MaxHitPoints = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int HitPoints
		{
			get
			{
				return m_HitPoints;
			}
			set
			{
				if ( value != m_HitPoints && MaxHitPoints > 0 )
				{
					m_HitPoints = value;

					if ( m_HitPoints < 0 )
						Delete();
					else if ( m_HitPoints > MaxHitPoints )
						m_HitPoints = MaxHitPoints;

					InvalidateProperties();
				}
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get{ return m_Crafter; }
			set{ m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int StrRequirement
		{
			get{ return ( m_StrReq == -1 ? (EraAOS ? AosStrReq : OldStrReq) : m_StrReq ); }
			set{ m_StrReq = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public ClothingQuality Quality
		{
			get{ return m_Quality; }
			set{ m_Quality = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster, AccessLevel.Lead )]
		public bool PlayerConstructed
		{
			get{ return m_PlayerConstructed; }
			set{ m_PlayerConstructed = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public bool Identified
		{
			get{ return m_Identified; }
			set{ m_Identified = value; InvalidateProperties(); }
		}

        [CommandProperty(AccessLevel.GameMaster)] // Alan mod
        public bool NotScissorable
        {
            get { return m_NotScissorable; }
            set { m_NotScissorable = value; }
        }

		public virtual CraftResource DefaultResource{ get{ return CraftResource.None; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public CraftResource Resource
		{
			get{ return m_Resource; }
			set{ m_Resource = value; Hue = CraftResources.GetHue( m_Resource ); InvalidateProperties(); }
		}

		public virtual int ArtifactRarity{ get{ return 0; } }

		public virtual int BaseStrBonus{ get{ return 0; } }
		public virtual int BaseDexBonus{ get{ return 0; } }
		public virtual int BaseIntBonus { get { return 0; } }

		public override bool AllowSecureTrade( Mobile from, Mobile to, Mobile newOwner, bool accepted )
		{
			if ( !Ethics.Ethic.CheckTrade( from, to, newOwner, this ) )
				return false;

			return base.AllowSecureTrade( from, to, newOwner, accepted );
		}

		public virtual Race RequiredRace { get { return null; } }

		public override bool CanEquip( Mobile from )
		{
			if ( !Ethics.Ethic.CheckEquip( from, this ) )
				return false;

			if( from.AccessLevel < AccessLevel.GameMaster )
			{
				if( RequiredRace != null && from.Race != RequiredRace )
				{
					if( RequiredRace == Race.Elf )
						from.SendLocalizedMessage( 1072203 ); // Only Elves may use this.
					else
						from.SendMessage( "Only {0} may use this.", RequiredRace.PluralName );

					return false;
				}
				else if( !AllowMaleWearer && !from.Female )
				{
					if( AllowFemaleWearer )
						from.SendLocalizedMessage( 1010388 ); // Only females can wear this.
					else
						from.SendMessage( "You may not wear this." );

					return false;
				}
				else if( !AllowFemaleWearer && from.Female )
				{
					if( AllowMaleWearer )
						from.SendLocalizedMessage( 1063343 ); // Only males can wear this.
					else
						from.SendMessage( "You may not wear this." );

					return false;
				}
				else
				{
					int strBonus = ComputeStatBonus( StatType.Str );
					int strReq = ComputeStatReq( StatType.Str );

					if( from.Str < strReq || (from.Str + strBonus) < 1 )
					{
						from.SendLocalizedMessage( 500213 ); // You are not strong enough to equip that.
						return false;
					}
				}
			}

			return base.CanEquip( from );
		}

		public virtual int AosStrReq{ get{ return 10; } }
		public virtual int OldStrReq{ get{ return 0; } }

		public virtual int InitMinHits{ get{ return 0; } }
		public virtual int InitMaxHits{ get{ return 0; } }

		public virtual bool AllowMaleWearer{ get{ return true; } }
		public virtual bool AllowFemaleWearer{ get{ return true; } }
		public virtual bool CanBeBlessed{ get{ return true; } }

		public int ComputeStatReq( StatType type )
		{
			int v;
			v = StrRequirement;

            return v;
		}

		public int ComputeStatBonus( StatType type )
		{
			if ( type == StatType.Str )
				return BaseStrBonus;
			else if ( type == StatType.Dex )
				return BaseDexBonus;
			else
				return BaseIntBonus;
		}

		public virtual void AddStatBonuses( Mobile parent )
		{
			if ( parent == null )
				return;

			int strBonus = ComputeStatBonus( StatType.Str );
			int dexBonus = ComputeStatBonus( StatType.Dex );
			int intBonus = ComputeStatBonus( StatType.Int );

			if ( strBonus == 0 && dexBonus == 0 && intBonus == 0 )
				return;

			string modName = this.Serial.ToString();

			if ( strBonus != 0 )
				parent.AddStatMod( new StatMod( StatType.Str, modName + "Str", strBonus, TimeSpan.Zero ) );

			if ( dexBonus != 0 )
				parent.AddStatMod( new StatMod( StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero ) );

			if ( intBonus != 0 )
				parent.AddStatMod( new StatMod( StatType.Int, modName + "Int", intBonus, TimeSpan.Zero ) );
		}

		public static void ValidateMobile( Mobile m )
		{
			for ( int i = m.Items.Count - 1; i >= 0; --i )
			{
				if ( i >= m.Items.Count )
					continue;

				Item item = m.Items[i];

				if ( item is BaseClothing )
				{
					BaseClothing clothing = (BaseClothing)item;

					if( clothing.RequiredRace != null && m.Race != clothing.RequiredRace )
					{
						if( clothing.RequiredRace == Race.Elf )
							m.SendLocalizedMessage( 1072203 ); // Only Elves may use this.
						else
							m.SendMessage( "Only {0} may use this.", clothing.RequiredRace.PluralName );

						m.AddToBackpack( clothing );
					}
					else if ( !clothing.AllowMaleWearer && !m.Female && m.AccessLevel < AccessLevel.GameMaster )
					{
						if ( clothing.AllowFemaleWearer )
							m.SendLocalizedMessage( 1010388 ); // Only females can wear this.
						else
							m.SendMessage( "You may not wear this." );

						m.AddToBackpack( clothing );
					}
					else if ( !clothing.AllowFemaleWearer && m.Female && m.AccessLevel < AccessLevel.GameMaster )
					{
						if ( clothing.AllowMaleWearer )
							m.SendLocalizedMessage( 1063343 ); // Only males can wear this.
						else
							m.SendMessage( "You may not wear this." );

						m.AddToBackpack( clothing );
					}
				}
			}
		}

		public override void OnAdded( object parent )
		{
			Mobile mob = parent as Mobile;

			if ( mob != null )
			{
				AddStatBonuses( mob );
				mob.CheckStatTimers();
           
                if (XmlScript.HasTrigger(this, TriggerName.onEquip) && UberScriptTriggers.Trigger(this, (Mobile)parent, TriggerName.onEquip))
                {
                    ((Mobile)parent).AddToBackpack(this); // override, put it in their pack
                    base.OnAdded(parent);
                    return;
                }
            }
            else if (parent is Item)
            {
                Item parentItem = (Item)parent;
                if (XmlScript.HasTrigger(this, TriggerName.onAdded))
                    UberScriptTriggers.Trigger(this, parentItem.RootParentEntity as Mobile, TriggerName.onAdded, parentItem);
            }

			base.OnAdded( parent );
		}

		public override void OnRemoved( object parent )
		{
			Mobile mob = parent as Mobile;

			if ( mob != null )
			{
				string modName = this.Serial.ToString();

				mob.RemoveStatMod( modName + "Str" );
				mob.RemoveStatMod( modName + "Dex" );
				mob.RemoveStatMod( modName + "Int" );

                mob.CheckStatTimers();

                if (XmlScript.HasTrigger(this, TriggerName.onUnequip))
                    UberScriptTriggers.Trigger(this, (Mobile)parent, TriggerName.onUnequip);
            }
            else if (parent is Item)
            {
                Item parentItem = (Item)parent;
                if (XmlScript.HasTrigger(this, TriggerName.onRemove))
                    UberScriptTriggers.Trigger(this, parentItem.RootParentEntity as Mobile, TriggerName.onRemove, parentItem);
            }

			base.OnRemoved( parent );
		}

        public override void OnDelete()
        {
            if (XmlScript.HasTrigger(this, TriggerName.onDelete))
                UberScriptTriggers.Trigger(this, this.RootParentEntity as Mobile, TriggerName.onDelete);
            base.OnDelete();
        }

		public virtual int OnHit( BaseWeapon weapon, Mobile attacker, int damageTaken )
		{
			int Absorbed = Utility.RandomMinMax( 1, 4 );

			damageTaken -= Absorbed;

			if ( damageTaken < 0 )
				damageTaken = 0;

			if ( weapon.Type == WeaponType.Bashing && m_EthicState != null || 0.25 > Utility.RandomDouble() && m_EthicState != null) // 25% chance to lower durability
			{
				
				int wear;

				if ( weapon.Type == WeaponType.Bashing )
					wear = Absorbed / 2;
				else
					wear = Utility.Random( 2 );

				if ( wear > 0 && m_MaxHitPoints > 0 )
				{
					if ( m_HitPoints >= wear )
					{
						HitPoints -= wear;
						wear = 0;
					}
					else
					{
						wear -= HitPoints;
						HitPoints = 0;
					}

					if ( wear > 0 )
					{
						if ( m_MaxHitPoints > wear )
						{
							MaxHitPoints -= wear;

							if ( Parent is Mobile )
								((Mobile)Parent).LocalOverheadMessage( MessageType.Regular, 0x3B2, 1061121 ); // Your equipment is severely damaged.
						}
						else
						{
							Delete();
						}
					}
				}
             }
            if (XmlScript.HasTrigger(this, TriggerName.onArmorHit))
                UberScriptTriggers.Trigger(this, attacker, TriggerName.onArmorHit, weapon, null, null, damageTaken);

			return damageTaken;
		}

		public override bool DisplayDyable{ get{ return false; } }

		public override bool Dye( Mobile from, IDyeTub sender )
		{
			if ( m_Resource > CraftResource.RegularWood && m_Resource <= CraftResource.Frostwood )
				return false;

			return base.Dye( from, sender );
		}

		public override Type DyeType
		{
			get
			{
				if ( m_Resource >= CraftResource.Iron && m_Resource <= CraftResource.Valorite )
					return typeof(RewardMetalDyeTub);
				else if ( m_Resource >= CraftResource.RegularLeather && m_Resource <= CraftResource.BarbedLeather )
					return typeof(LeatherDyeTub);
				else
					return typeof(DyeTub);
			}
		}

		public BaseClothing( int itemID, Layer layer ) : this( itemID, layer, 0 )
		{
		}

		public BaseClothing( int itemID, Layer layer, int hue ) : base( itemID )
		{
			Identified = true;

			Layer = layer;
			Hue = hue;

			m_Resource = DefaultResource;
			m_Quality = ClothingQuality.Regular;

			m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax( InitMinHits, InitMaxHits );

			Dyable = true;
		}

		public override void OnAfterDuped( Item newItem )
		{
			BaseClothing clothing = newItem as BaseClothing;

			if ( clothing == null )
				return;
		}

		public BaseClothing( Serial serial ) : base( serial )
		{
		}

		public override bool AllowEquippedCast( Mobile from )
		{
			if ( base.AllowEquippedCast( from ) )
				return true;
            return false;
		}

		public void UnscaleDurability()
		{
			int scale = 100;

			m_HitPoints = ( ( m_HitPoints * 100 ) + ( scale - 1 ) ) / scale;
			m_MaxHitPoints = ( ( m_MaxHitPoints * 100 ) + ( scale - 1 ) ) / scale;

			InvalidateProperties();
		}

		public void ScaleDurability()
		{
			int scale = 100;

			m_HitPoints = ( ( m_HitPoints * scale ) + 99 ) / 100;
			m_MaxHitPoints = ( ( m_MaxHitPoints * scale ) + 99 ) / 100;

			InvalidateProperties();
		}

		public override bool CheckPropertyConfliction( Mobile m )
		{
			if ( base.CheckPropertyConfliction( m ) )
				return true;

			if ( Layer == Layer.Pants )
				return ( m.FindItemOnLayer( Layer.InnerLegs ) != null );

			if ( Layer == Layer.Shirt )
				return ( m.FindItemOnLayer( Layer.InnerTorso ) != null );

			return false;
		}

		private string GetNameString()
		{
			string name = this.Name;

			if ( name == null )
				name = String.Format( "#{0}", LabelNumber );

			return name;
		}

		public int GetLeatherLabel( int baselabel )
		{
			if ( Identified && Resource >= CraftResource.SpinedLeather && Resource <= CraftResource.BarbedLeather )
				return baselabel + (26 * (int)(Resource - CraftResource.SpinedLeather));
			else
				return base.LabelNumber;
		}

		public override void AddNameProperty( ObjectPropertyList list )
		{
			int oreType;

			switch ( m_Resource )
			{
				case CraftResource.DullCopper:		oreType = 1053108; break; // dull copper
				case CraftResource.ShadowIron:		oreType = 1053107; break; // shadow iron
				case CraftResource.Copper:			oreType = 1053106; break; // copper
				case CraftResource.Bronze:			oreType = 1053105; break; // bronze
				case CraftResource.Gold:			oreType = 1053104; break; // golden
				case CraftResource.Agapite:			oreType = 1053103; break; // agapite
				case CraftResource.Verite:			oreType = 1053102; break; // verite
				case CraftResource.Valorite:		oreType = 1053101; break; // valorite
				case CraftResource.SpinedLeather:	oreType = 1061118; break; // spined
				case CraftResource.HornedLeather:	oreType = 1061117; break; // horned
				case CraftResource.BarbedLeather:	oreType = 1061116; break; // barbed
				case CraftResource.RedScales:		oreType = 1060814; break; // red
				case CraftResource.YellowScales:	oreType = 1060818; break; // yellow
				case CraftResource.BlackScales:		oreType = 1060820; break; // black
				case CraftResource.GreenScales:		oreType = 1060819; break; // green
				case CraftResource.WhiteScales:		oreType = 1060821; break; // white
				case CraftResource.BlueScales:		oreType = 1060815; break; // blue
				default: oreType = 0; break;
			}

			if ( oreType != 0 )
				list.Add( 1053099, "#{0}\t{1}", oreType, GetNameString() ); // ~1_oretype~ ~2_armortype~
			else if ( Name == null )
				list.Add( LabelNumber );
			else
				list.Add( Name );
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Crafter != null )
				list.Add( 1050043, m_Crafter.RawName ); // crafted by ~1_NAME~

			#region Factions
			if ( m_FactionState != null )
				list.Add( 1041350 ); // faction item
			#endregion

			if ( m_Quality == ClothingQuality.Exceptional )
				list.Add( 1060636 ); // exceptional

			if( RequiredRace == Race.Elf )
				list.Add( 1075086 ); // Elves Only

			int prop;

			if ( (prop = ArtifactRarity) > 0 )
				list.Add( 1061078, prop.ToString() ); // artifact rarity ~1_val~

			if ( (prop = ComputeStatReq( StatType.Str )) > 0 )
				list.Add( 1061170, prop.ToString() ); // strength requirement ~1_val~

			if ( m_HitPoints >= 0 && m_MaxHitPoints > 0 )
				list.Add( 1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints ); // durability ~1_val~ / ~2_val~
		}

		public override void OnSingleClick( Mobile from )
		{
			if ( Deleted || !from.CanSee( this ) )
				return;

			LabelToExpansion(from);

			int number;

			if ( String.IsNullOrEmpty( Name ) )
				number = LabelNumber;
			else
			{
				this.LabelTo( from, Name );
				number = 1041000;
			}

			if ( DisplayDyable )
				LabelDyableTo( from );

			List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

			bool ismagical = AddEquipInfoAttributes( from, attrs );

			if ( attrs.Count > 0 || Crafter != null || number != 1041000 )
			{

				EquipmentInfo eqInfo = new EquipmentInfo( number, m_Crafter, from.AccessLevel < AccessLevel.GameMaster && ismagical && !m_Identified, attrs.ToArray() );
				from.Send( new DisplayEquipmentInfo( this, eqInfo ) );
			}
		}

		public virtual bool AddEquipInfoAttributes( Mobile from, List<EquipInfoAttribute> attrs )
		{
			if ( DisplayLootType )
			{
				if ( LootType == LootType.Blessed )
					attrs.Add( new EquipInfoAttribute( 1038021 ) ); // blessed
				else if ( LootType == LootType.Cursed )
					attrs.Add( new EquipInfoAttribute( 1049643 ) ); // cursed
			}

			#region Factions
			if ( m_FactionState != null )
				attrs.Add( new EquipInfoAttribute( 1041350 ) ); // faction item
			#endregion

			if ( m_Quality != ClothingQuality.Regular )
				attrs.Add( new EquipInfoAttribute( 1018305 - (int)m_Quality ) );

			if ( m_EthicState != null )
			{
				if ( m_EthicState.IsRunic )
					attrs.Add( new EquipInfoAttribute( m_EthicState.Ethic == Ethics.Ethic.Evil ? 1045017 : 1045002 ) );
				else
					attrs.Add( new EquipInfoAttribute( m_EthicState.Ethic == Ethics.Ethic.Evil ? 1042519 : 1042518 ) );
			}

			return false;
		}

		#region Serialization
		private static void SetSaveFlag( ref SaveFlag flags, SaveFlag toSet, bool setIf )
		{
			if ( setIf )
				flags |= toSet;
		}

		private static bool GetSaveFlag( SaveFlag flags, SaveFlag toGet )
		{
			return ( (flags & toGet) != 0 );
		}

		[Flags]
		private enum SaveFlag
		{
			None				= 0x00000000,
			Resource			= 0x00000001,
			Attributes			= 0x00000002,
			ClothingAttributes	= 0x00000004,
			SkillBonuses		= 0x00000008,
			Resistances			= 0x00000010,
			MaxHitPoints		= 0x00000020,
			HitPoints			= 0x00000040,
			PlayerConstructed	= 0x00000080,
			Crafter				= 0x00000100,
			Quality				= 0x00000200,
			StrReq				= 0x00000400,
			Identified			= 0x00000800,
            NotScissorable         = 0x01000000 // Alan Mod (I'm starting high up in-case there are RunUO updates
		}

		public override void Serialize( GenericWriter writer )
		{
			#region Ethics
			if ( m_EthicState != null && m_EthicState.HasExpired )
				m_EthicState.Detach();
			#endregion

			#region Factions
			if ( m_FactionState != null && m_FactionState.HasExpired )
				m_FactionState.Detach();
			#endregion

			base.Serialize( writer );

			writer.Write( (int) 5 ); // version

			SaveFlag flags = SaveFlag.None;

			SetSaveFlag( ref flags, SaveFlag.Resource,			m_Resource != DefaultResource );
			//SetSaveFlag( ref flags, SaveFlag.Attributes,		!m_AosAttributes.IsEmpty );
			//SetSaveFlag( ref flags, SaveFlag.ClothingAttributes,!m_AosClothingAttributes.IsEmpty );
			//SetSaveFlag( ref flags, SaveFlag.SkillBonuses,		!m_AosSkillBonuses.IsEmpty );
			//SetSaveFlag( ref flags, SaveFlag.Resistances,		!m_AosResistances.IsEmpty );
			SetSaveFlag( ref flags, SaveFlag.MaxHitPoints,		m_MaxHitPoints != 0 );
			SetSaveFlag( ref flags, SaveFlag.HitPoints,			m_HitPoints != 0 );
			SetSaveFlag( ref flags, SaveFlag.PlayerConstructed,	m_PlayerConstructed != false );
			SetSaveFlag( ref flags, SaveFlag.Crafter,			m_Crafter != null );
			SetSaveFlag( ref flags, SaveFlag.Quality,			m_Quality != ClothingQuality.Regular );
			SetSaveFlag( ref flags, SaveFlag.StrReq,			m_StrReq != -1 );
			SetSaveFlag( ref flags, SaveFlag.Identified,		m_Identified != false );
            SetSaveFlag(ref flags, SaveFlag.NotScissorable,  m_NotScissorable);

			writer.WriteEncodedInt( (int) flags );

			if ( GetSaveFlag( flags, SaveFlag.Resource ) )
				writer.WriteEncodedInt( (int) m_Resource );

			//if ( GetSaveFlag( flags, SaveFlag.Attributes ) )
			//	m_AosAttributes.Serialize( writer );

			//if ( GetSaveFlag( flags, SaveFlag.ClothingAttributes ) )
				//m_AosClothingAttributes.Serialize( writer );

			//if ( GetSaveFlag( flags, SaveFlag.SkillBonuses ) )
				//m_AosSkillBonuses.Serialize( writer );

			//if ( GetSaveFlag( flags, SaveFlag.Resistances ) )
				//m_AosResistances.Serialize( writer );

			if ( GetSaveFlag( flags, SaveFlag.MaxHitPoints ) )
				writer.WriteEncodedInt( (int) m_MaxHitPoints );

			if ( GetSaveFlag( flags, SaveFlag.HitPoints ) )
				writer.WriteEncodedInt( (int) m_HitPoints );

			if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
				writer.Write( (Mobile) m_Crafter );

			if ( GetSaveFlag( flags, SaveFlag.Quality ) )
				writer.WriteEncodedInt( (int) m_Quality );

			if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
				writer.WriteEncodedInt( (int) m_StrReq );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{                
                case 5:
				{
					SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

                    if (GetSaveFlag(flags, SaveFlag.NotScissorable)) // Alan Mod
                        m_NotScissorable = true;

					if ( GetSaveFlag( flags, SaveFlag.Resource ) )
						m_Resource = (CraftResource)reader.ReadEncodedInt();
					else
						m_Resource = DefaultResource;

					if ( GetSaveFlag( flags, SaveFlag.Attributes ) )
					    new AosAttributes( this, reader );
					//else
					//	m_AosAttributes = new AosAttributes( this );

					if ( GetSaveFlag( flags, SaveFlag.ClothingAttributes ) )
						new AosArmorAttributes( this, reader );
					//else
					//	m_AosClothingAttributes = new AosArmorAttributes( this );

					if ( GetSaveFlag( flags, SaveFlag.SkillBonuses ) )
						new AosSkillBonuses( this, reader );
					//else
					//	m_AosSkillBonuses = new AosSkillBonuses( this );

					if ( GetSaveFlag( flags, SaveFlag.Resistances ) )
						new AosElementAttributes( this, reader );
					//else
						//m_AosResistances = new AosElementAttributes( this );

					if ( GetSaveFlag( flags, SaveFlag.MaxHitPoints ) )
						m_MaxHitPoints = reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.HitPoints ) )
						m_HitPoints = reader.ReadEncodedInt();

					if ( GetSaveFlag( flags, SaveFlag.Crafter ) )
						m_Crafter = reader.ReadMobile();

					if ( GetSaveFlag( flags, SaveFlag.Quality ) )
						m_Quality = (ClothingQuality)reader.ReadEncodedInt();
					else
						m_Quality = ClothingQuality.Regular;

					if ( GetSaveFlag( flags, SaveFlag.StrReq ) )
						m_StrReq = reader.ReadEncodedInt();
					else
						m_StrReq = -1;

					if ( GetSaveFlag( flags, SaveFlag.PlayerConstructed ) )
						m_PlayerConstructed = true;

					if ( GetSaveFlag( flags, SaveFlag.Identified ) )
						m_Identified = true;

					break;
				}
				case 4:
				{
					m_Resource = (CraftResource)reader.ReadInt();

					goto case 3;
				}
				case 3:
				{
					//m_AosAttributes = new AosAttributes( this, reader );
					//m_AosClothingAttributes = new AosArmorAttributes( this, reader );
					//m_AosSkillBonuses = new AosSkillBonuses( this, reader );
					//m_AosResistances = new AosElementAttributes( this, reader );

					goto case 2;
				}
				case 2:
				{
					m_PlayerConstructed = reader.ReadBool();
					goto case 1;
				}
				case 1:
				{
					m_Crafter = reader.ReadMobile();
					m_Quality = (ClothingQuality)reader.ReadInt();
					break;
				}
				case 0:
				{
					m_Crafter = null;
					m_Quality = ClothingQuality.Regular;
					break;
				}
			}

			if ( version < 2 )
				m_PlayerConstructed = true; // we don't know, so, assume it's crafted

			if ( version < 3 )
			{
				//m_AosAttributes = new AosAttributes( this );
				//m_AosClothingAttributes = new AosArmorAttributes( this );
				//m_AosSkillBonuses = new AosSkillBonuses( this );
				//m_AosResistances = new AosElementAttributes( this );
			}

			if ( version < 4 )
				m_Resource = DefaultResource;

			if ( m_MaxHitPoints == 0 && m_HitPoints == 0 )
				m_HitPoints = m_MaxHitPoints = Utility.RandomMinMax( InitMinHits, InitMaxHits );

			Mobile parent = Parent as Mobile;

			if ( parent != null )
			{
				AddStatBonuses( parent );
				parent.CheckStatTimers();
			}
		}
		#endregion

		public virtual bool Scissor( Mobile from, Scissors scissors )
		{
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 502437 ); // Items you wish to cut must be in your backpack.
				return false;
			}

            if (Ethics.Ethic.IsImbued(this) || NotScissorable)
			{
				from.SendLocalizedMessage( 502440 ); // Scissors can not be used on that to produce anything.
				return false;
			}

			if ( TestCenter.Enabled && !String.IsNullOrEmpty( Name ) && Name.ToLower().IndexOf( "beta" ) > -1 )
			{
				from.SendMessage( "This item is too special to be cut." );
				return false;
			}

			CraftSystem system = DefTailoring.CraftSystem;

			CraftItem item = system.CraftItems.SearchFor( GetType() );

			if ( item != null && item.Resources.Count == 1 && item.Resources.GetAt( 0 ).Amount >= 2 )
			{
				try
				{
					Type resourceType = null;

					CraftResourceInfo info = CraftResources.GetInfo( m_Resource );

					if ( info != null && info.ResourceTypes.Length > 0 )
						resourceType = info.ResourceTypes[0];

					if ( resourceType == null )
						resourceType = item.Resources.GetAt( 0 ).ItemType;

					Item res = (Item)Activator.CreateInstance( resourceType );

					ScissorHelper( from, res, m_PlayerConstructed ? (item.Resources.GetAt( 0 ).Amount / 2) : 1, !resourceType.IsSubclassOf( typeof( BaseLeather ) ) );

					//Cutting up items with special hues only gives 1 cloth
					if ( res.Amount >= 1 && res.Hue >= 1000 )
					{
						from.SendMessage("The corrosive dye destroyed most of the cloth, yielding very little usable material.");
						res.Amount = 1;
					}

					res.LootType = LootType.Regular;

					return true;
				}
				catch
				{
				}
			}

			from.SendLocalizedMessage( 502440 ); // Scissors can not be used on that to produce anything.
			return false;
		}

		#region ICraftable Members

		public virtual int OnCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, IBaseTool tool, CraftItem craftItem, int resHue )
		{
			Quality = (ClothingQuality)quality;
			Identified = true;

			if ( makersMark )
				Crafter = from;

			if ( DefaultResource != CraftResource.None )
			{
				Type resourceType = typeRes;

				if ( resourceType == null )
					resourceType = craftItem.Resources.GetAt( 0 ).ItemType;

				Resource = CraftResources.GetFromType( resourceType );
			}
			else
			{
				Hue = resHue;
			}

			PlayerConstructed = true;

			CraftContext context = craftSystem.GetContext( from );

			if ( context != null && context.DoNotColor )
				Hue = 0;

			return quality;
		}

		#endregion
	}
}