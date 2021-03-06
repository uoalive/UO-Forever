using System;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	public class HolidayDecorativeTopiary : BaseHolidayGift
	{
		[Constructable]
		public HolidayDecorativeTopiary(int year)
		{
            ItemID = 0x2378;

            HolidayName = "Christmas";
            HolidayYear = year;

			Weight = 1.0;
			LootType = LootType.Blessed;
		}

		public HolidayDecorativeTopiary( Serial serial ) : base( serial )
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