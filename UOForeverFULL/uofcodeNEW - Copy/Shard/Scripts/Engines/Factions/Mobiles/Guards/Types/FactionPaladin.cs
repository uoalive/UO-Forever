using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Factions
{
	public class FactionPaladin : BaseFactionGuard
	{
		public override GuardAI GuardAI{ get{ return GuardAI.Magic | GuardAI.Melee | GuardAI.Smart | GuardAI.Curse | GuardAI.Bless; } }

		[Constructable]
		public FactionPaladin() : base( "the paladin" )
		{
			GenerateBody( false, false );

			SetStr( 161, 190 );
			SetDex( 61, 85 );
			SetInt( 251, 275 );

			
			
			
			
			

			VirtualArmor = 32;

			SetSkill( SkillName.Swords, 110.0, 120.0 );
			SetSkill( SkillName.Wrestling, 110.0, 120.0 );
			SetSkill( SkillName.Tactics, 110.0, 120.0 );
			SetSkill( SkillName.MagicResist, 110.0, 120.0 );
			SetSkill( SkillName.Healing, 110.0, 120.0 );
			SetSkill( SkillName.Anatomy, 110.0, 120.0 );

			SetSkill( SkillName.Magery, 110.0, 120.0 );
			SetSkill( SkillName.EvalInt, 110.0, 120.0 );
			SetSkill( SkillName.Meditation, 110.0, 120.0 );

			AddItem( Immovable( Rehued( new PlateChest(), 2125 ) ) );
			AddItem( Immovable( Rehued( new PlateLegs(), 2125 ) ) );
			AddItem( Immovable( Rehued( new PlateHelm(), 2125 ) ) );
			AddItem( Immovable( Rehued( new PlateGorget(), 2125 ) ) );
			AddItem( Immovable( Rehued( new PlateArms(), 2125 ) ) );
			AddItem( Immovable( Rehued( new PlateGloves(), 2125 ) ) );

			AddItem( Immovable( Rehued( new BodySash(), 1254 ) ) );
			AddItem( Immovable( Rehued( new Cloak(), 1254 ) ) );

			AddItem( new Halberd() );

			AddItem( Immovable( Rehued( new VirtualMountItem( this ), 1254 ) ) );

			PackItem( new Bandage( Utility.RandomMinMax( 30, 40 ) ) );
			PackStrongPotions( 6, 12 );
		}

		public FactionPaladin( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}