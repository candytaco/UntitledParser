using System;
using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components {

	/// <summary>
	/// Serves as a wrapper for all packets.
	/// </summary>
	public class PacketFrame : DemoComponent {

		public byte? PlayerSlot; // demo protocol 4 only
		public DemoPacket? Packet;
		public int Tick;
		public PacketType Type;


		public PacketFrame(SourceDemo? demoRef) : base(demoRef) {}


		protected override void Parse(ref BitStreamReader bsr) {

			byte typeVal = bsr.ReadByte();
			Type = DemoPacket.ByteToPacketType(DemoInfo, typeVal);

			if (Type == PacketType.Unknown)
				throw new ArgumentException("no byte->packet mapper found for this game!");
			else if (Type == PacketType.Invalid)
				throw new ArgumentException($"Illegal packet type: {typeVal}");

			// very last byte is cut off in (all?) demos, copy data from the previous frame if this is the case

			Tick = bsr.BitsRemaining >= 32
				? bsr.ReadSInt()
				: (int)bsr.ReadUInt(24) | (DemoRef.Frames[^2].Tick & (0xff << 24));

			if (DemoInfo.NewDemoProtocol)
				PlayerSlot = bsr.BitsRemaining >= 8
					? bsr.ReadByte()
					: DemoRef.Frames[^2].PlayerSlot;

			Packet = PacketFactory.CreatePacket(DemoRef!, this, Type);
			Packet.ParseStream(ref bsr);

			// make sure the next frame starts on a byte boundary
			if (bsr.CurrentBitIndex % 8 != 0)
				bsr.SkipBits(8 - bsr.CurrentBitIndex % 8);
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			if (Packet != null) {
				pw.Append($"[{Tick}] {Type.ToString().ToUpper()} ({DemoPacket.PacketTypeToByte(DemoInfo, Type)})");
				if (DemoInfo.NewDemoProtocol && PlayerSlot.HasValue)
					pw.Append($"\nplayer slot: {PlayerSlot.Value}");
				if (Packet.MayContainData) {
					pw.FutureIndent++;
					pw.AppendLine();
					Packet.PrettyWrite(pw);
					pw.FutureIndent--;
				}
			} else {
				pw.Append("demo parsing failed here, packet type doesn't correspond with any known packet");
			}
		}

		public override void XMLWrite(XElement parent)
		{
			if (Packet == null)
				parent.Add(new XComment("demo parsing failed here, packet type doesn't correspond with any known packet"));
			else
			{
				XElement thisFrame = new XElement("Frame");
				thisFrame.Add(new XAttribute("Tick", Tick));
				thisFrame.Add(new XAttribute("Type", Type.ToString()));
				if (DemoInfo.NewDemoProtocol && PlayerSlot.HasValue)
					thisFrame.Add(new XAttribute("Player-Slot", PlayerSlot.Value));
				if (Packet.MayContainData)
					Packet.XMLWrite(thisFrame);
				parent.Add(thisFrame);
			}
		}
	}
}
