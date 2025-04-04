using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {

	public class SvcSetView : DemoMessage {

		public uint EntityIndex;


		public SvcSetView(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			EntityIndex = bsr.ReadUInt(DemoInfo.MaxEdictBits);
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"entity index: {EntityIndex}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("SvcSetView", EntityIndex);
			parent.Add(thisElement);
		}
	}
}
