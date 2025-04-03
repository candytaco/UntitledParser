using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Packets.StringTableEntryTypes {

	public class UnknownStringTableEntryData : StringTableEntryData {

		internal override bool ContentsKnown => false;


		public UnknownStringTableEntryData(SourceDemo? demoRef, int? decompressedIndex = null) : base(demoRef, decompressedIndex) {}


		protected override void Parse(ref BitStreamReader bsr) {}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append(Reader.BitLength > 64
				? $"{Reader.BitLength, 5} bit{(Reader.BitLength > 1 ? "s" : "")}"
				: $"({Reader.ToBinaryString()})");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("UnknownStringTableEntryData", new XAttribute("Bit-Length", Reader.BitLength));
			thisElement.Value = Reader.ToBinaryString();
			parent.Add(thisElement);
		}
	}
}
