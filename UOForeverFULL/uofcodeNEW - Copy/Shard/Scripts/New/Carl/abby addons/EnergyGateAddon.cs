
////////////////////////////////////////
//                                    //
//   Generated by CEO's YAAAG - V1.2  //
// (Yet Another Arya Addon Generator) //
//                                    //
////////////////////////////////////////
using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class EnergyGateAddon : BaseAddon
	{
         
            
		public override BaseAddonDeed Deed
		{
			get
			{
				return new EnergyGateAddonDeed();
			}
		}

		[ Constructable ]
		public EnergyGateAddon()
		{



			AddComplexComponent( (BaseAddon) this, 1007, 0, 1, 0, 1910, -1, "", 1);// 1
			AddComplexComponent( (BaseAddon) this, 1006, 0, 0, 0, 1910, -1, "", 1);// 2
			AddComplexComponent( (BaseAddon) this, 1009, -1, -1, 0, 1910, -1, "", 1);// 3
			AddComplexComponent( (BaseAddon) this, 1009, 0, -1, 0, 1910, -1, "", 1);// 4
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 0, 1910, -1, "", 1);// 5
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 2, 1910, -1, "", 1);// 6
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 4, 1910, -1, "", 1);// 7
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 6, 1910, -1, "", 1);// 8
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 9, 1910, -1, "", 1);// 9
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 14, 1910, -1, "", 1);// 10
			AddComplexComponent( (BaseAddon) this, 1006, 1, 0, 19, 1910, -1, "", 1);// 11
			AddComplexComponent( (BaseAddon) this, 1006, -1, 0, 4, 1910, -1, "", 1);// 12
			AddComplexComponent( (BaseAddon) this, 1006, -1, 0, 9, 1910, -1, "", 1);// 13
			AddComplexComponent( (BaseAddon) this, 1006, -1, 0, 14, 1910, -1, "", 1);// 14
			AddComplexComponent( (BaseAddon) this, 1006, -1, 0, 20, 1910, -1, "", 1);// 15
			AddComplexComponent( (BaseAddon) this, 6732, 0, 0, 15, 0, -1, "pure energy", 1);// 16
			AddComplexComponent( (BaseAddon) this, 6732, 0, 0, 4, 0, -1, "pure energy", 1);// 17
			AddComplexComponent( (BaseAddon) this, 1006, 0, 0, 23, 1910, -1, "", 1);// 18
			AddComplexComponent( (BaseAddon) this, 1006, -1, 0, 0, 1910, -1, "", 1);// 19
			AddComplexComponent( (BaseAddon) this, 1016, 1, 1, 0, 1910, -1, "", 1);// 20
			AddComplexComponent( (BaseAddon) this, 1017, -1, 1, 0, 1910, -1, "", 1);// 21
			AddComplexComponent( (BaseAddon) this, 1018, 1, -1, 0, 1910, -1, "", 1);// 22

		}

		public EnergyGateAddon( Serial serial ) : base( serial )
		{
		}

        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource)
        {
            AddComplexComponent(addon, item, xoffset, yoffset, zoffset, hue, lightsource, null, 1);
        }

        private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset, int hue, int lightsource, string name, int amount)
        {
            AddonComponent ac;
            ac = new AddonComponent(item);
            if (name != null && name.Length > 0)
                ac.Name = name;
            if (hue != 0)
                ac.Hue = hue;
            if (amount > 1)
            {
                ac.Stackable = true;
                ac.Amount = amount;
            }
            if (lightsource != -1)
                ac.Light = (LightType) lightsource;
            addon.AddComponent(ac, xoffset, yoffset, zoffset);
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

	public class EnergyGateAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new EnergyGateAddon();
			}
		}

		[Constructable]
		public EnergyGateAddonDeed()
		{
			Name = "EnergyGate";
		}

		public EnergyGateAddonDeed( Serial serial ) : base( serial )
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