/////////////////////////////////////////////////
//                                             //
// Automatically generated by the              //
// AddonGenerator script by Arya               //
//                                             //
/////////////////////////////////////////////////
using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class AG_ShowerSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed
		{
			get
			{
				return new AG_ShowerSouthAddonDeed();
			}
		}

		[ Constructable ]
		public AG_ShowerSouthAddon()
		{
			AddComponent( new AddonComponent( 5535 ), 3, -1, 0 );
			AddComponent( new AddonComponent( 1293 ), -1, 1, 0 );
			AddComponent( new AddonComponent( 262 ), -2, 1, 0 );
			AddComponent( new AddonComponent( 1293 ), 1, 1, 0 );
			AddComponent( new AddonComponent( 263 ), 1, 1, 0 );
			AddComponent( new AddonComponent( 4844 ), 0, 2, 0 );
			AddComponent( new AddonComponent( 4833 ), 2, 0, 0 );
			AddComponent( new AddonComponent( 14138 ), 0, -1, 0 );
			AddComponent( new AddonComponent( 1293 ), 0, -1, 0 );
			AddComponent( new AddonComponent( 1293 ), 0, 0, 0 );
			AddComponent( new AddonComponent( 262 ), -2, -1, 0 );
			AddComponent( new AddonComponent( 6197 ), 1, 0, 26 );
			AddComponent( new AddonComponent( 1293 ), 1, 0, 0 );
			AddComponent( new AddonComponent( 1293 ), 1, -1, 0 );
			AddComponent( new AddonComponent( 5154 ), -1, 0, 12 );
			AddComponent( new AddonComponent( 1293 ), -1, 0, 0 );
			AddComponent( new AddonComponent( 2913 ), -1, 0, 10 );
			AddComponent( new AddonComponent( 3626 ), -1, 0, 11 );
			AddComponent( new AddonComponent( 1293 ), -1, -1, 0 );
			AddComponent( new AddonComponent( 1293 ), 0, 1, 0 );
			AddComponent( new AddonComponent( 262 ), -2, 0, 0 );
			AddComponent( new AddonComponent( 263 ), -2, -2, 0 );
			AddComponent( new AddonComponent( 261 ), -1, -2, 0 );
			AddComponent( new AddonComponent( 261 ), 0, -2, 0 );
			AddComponent( new AddonComponent( 261 ), 1, -2, 0 );
			AddonComponent ac;
			ac = new AddonComponent( 261 );
			AddComponent( ac, -1, -2, 0 );
			ac = new AddonComponent( 261 );
			AddComponent( ac, 0, -2, 0 );
			ac = new AddonComponent( 261 );
			AddComponent( ac, 1, -2, 0 );
			ac = new AddonComponent( 263 );
			AddComponent( ac, -2, -2, 0 );
			ac = new AddonComponent( 5154 );
			ac.Name = "Soap";
			AddComponent( ac, -1, 0, 12 );
			ac = new AddonComponent( 6197 );
            ac.Name = "Massage-O-Matic";
			AddComponent( ac, 1, 0, 26 );
			ac = new AddonComponent( 14138 );
			AddComponent( ac, 0, -1, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, -1, -1, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, -1, 0, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, -1, 1, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, 0, -1, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, 0, 0, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, 0, 1, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, 1, -1, 0 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, 1, 0, 0 );
			ac = new AddonComponent( 2913 );
			ac.Hue = 2101;
			AddComponent( ac, -1, 0, 10 );
			ac = new AddonComponent( 1293 );
			AddComponent( ac, 1, 1, 0 );
			ac = new AddonComponent( 262 );
			AddComponent( ac, -2, 1, 0 );
			ac = new AddonComponent( 262 );
			AddComponent( ac, -2, 0, 0 );
			ac = new AddonComponent( 262 );
			AddComponent( ac, -2, -1, 0 );
			ac = new AddonComponent( 263 );
			AddComponent( ac, 1, 1, 0 );
			ac = new AddonComponent( 4844 );
			AddComponent( ac, 0, 2, 0 );
			ac = new AddonComponent( 4833 );
			AddComponent( ac, 2, 0, 0 );
			ac = new AddonComponent( 3626 );
			ac.Name = "Shampoo";
			AddComponent( ac, -1, 0, 11 );

		}

		public AG_ShowerSouthAddon( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class AG_ShowerSouthAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new AG_ShowerSouthAddon();
			}
		}

		[Constructable]
		public AG_ShowerSouthAddonDeed()
		{
			Name = "AG_ShowerSouth";
		}

		public AG_ShowerSouthAddonDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void	Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}