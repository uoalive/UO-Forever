﻿#region References

using System.Linq;
using Server.Network;

#endregion

namespace Server.Items
{
    public class PutridCapArtifact : JesterHat
    {
        [Constructable]
        public PutridCapArtifact()
        {
            Name = "putrid jester hat";
            Weight = 2;
            Movable = true;
            Hue = 2966;
        }

        public PutridCapArtifact(Serial serial)
            : base(serial)
        {}

        public override void OnSingleClick(Mobile m)
        {
            base.OnSingleClick(m);
            LabelTo(m,
                "[Champion Artifact]",
                134);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int) 0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}