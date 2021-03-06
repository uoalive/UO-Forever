﻿#region References
using System;

using Server.Factions;
using Server.Mobiles;
using Server.Mobiles.MetaPet;
using Server.Mobiles.MetaPet.Skills;
using Server.Mobiles.MetaSkills;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using VitaNex.Targets;

#endregion

namespace Server.Items
{
	public class MoltenRelic : BaseMetaRelic
	{
		[Constructable]
        public MoltenRelic()
		{
			Name = "a molten relic";
            Hue = 1360;
			Weight = 2.0;
			Stackable = false;
		    MetaSkillType = MetaSkillType.Molten;
            MetaSkillName = "Magma Breath";
		    NextLevelExperience = 100;
		    MaxLevel = 10;
		    ApplicationChance = 0.2;
		    CoolDown = TimeSpan.FromSeconds(25);
		    SkillMulti = 1;
		}

        public MoltenRelic(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

            int version = writer.SetVersion(0);

            switch (version)
            {
                case 0:
                    break;
            }
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

            int version = reader.GetVersion();


            switch (version)
            {
                case 0:
                    break;
            }
		}
	}
}