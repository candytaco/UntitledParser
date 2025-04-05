using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Packets {

	/// <summary>
	/// Synchronizes server and client clock. Contains no data.
	/// </summary>
	public class SyncTick: DemoPacket {

		public override bool MayContainData => false;

		public SyncTick(SourceDemo? demoRef, PacketFrame frameRef): base(demoRef, frameRef) {}

		protected override void Parse(ref BitStreamReader bsr) {}
		public override void XMLWrite(XElement parent)
		{
			parent.Add(new XElement("SyncTick"));
		}
	}
}
