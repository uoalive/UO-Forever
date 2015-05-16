using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a treefellow corpse" )]
	public class Treefellow : BaseCreature
	{
		public override WeaponAbility GetWeaponAbility()
		{
			return WeaponAbility.Dismount;
		}

		public override string DefaultName{ get{ return "a treefellow"; } }

		[Constructable]
		public Treefellow() : base( AIType.AI_Melee, FightMode.Evil, 10, 1, 0.2, 0.4 )
		{
			Body = 301;

			SetStr( 196, 220 );
			SetDex( 31, 55 );
			SetInt( 66, 90 );

			SetHits( 118, 132 );

			SetDamage( 12, 16 );

			

			
			
			
			

			SetSkill( SkillName.MagicResist, 40.1, 55.0 );
			SetSkill( SkillName.Tactics, 65.1, 90.0 );
			SetSkill( SkillName.Wrestling, 65.1, 85.0 );

			Fame = 500;
			Karma = 1500;

			VirtualArmor = 24;
			PackItem( new Log( Utility.RandomMinMax( 23, 34 ) ) );
		}

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.FeyAndUndead; }
		}

		public override int GetIdleSound()
		{
			return 443;
		}

		public override int GetDeathSound()
		{
			return 31;
		}

		public override int GetAttackSound()
		{
			return 672;
		}

		public override bool BleedImmune{ get{ return true; } }
		public override int DefaultBloodHue{ get{ return -1; } }

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
		}

		public Treefellow( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( BaseSoundID == 442 )
				BaseSoundID = -1;
		}
	}
}